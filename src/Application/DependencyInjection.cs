using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Application;
using Domain.Buildings;
using Domain.Common;
using Domain.Elevators;
using Domain.Users;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

            config.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        // Register elevator services
        services.AddSingleton<IInMemoryElevatorPoolService, InMemoryElevatorPoolService>();
        services.AddTransient<IElevatorOrchestratorService, ElevatorOrchestratorService>();

        // Strategy pattern for elevator selection
        services.AddTransient<IElevatorSelectionContext, ElevatorSelectionContext>();
        services.AddTransient<IClosestElevatorStrategy, ClosestElevatorStrategy>();
        services.AddTransient<IQueueCapacityStrategy, QueueCapacityStrategy>();
        return services;
    }
}


public enum ConfigurationMenuSelection
{
    /// <summary> Default value </summary>
    None,
    /// <summary> Register a user </summary>
    Register,
    /// <summary> Exit the configuration menu </summary>
    Exit,
}


/// <summary> Menu Selections </summary>
public enum MenuSelection
{
    /// <summary> Login </summary>
    Login,
    /// <summary> Exit </summary>
    Exit,
    /// <summary> Dashboard </summary>
    Dashboard,
    /// <summary> ElevatorControl </summary>
    ElevatorControl,
    //Multi Request Elevator Control
    MultiElevatorControl
}



/// <summary> Service for orchestrating elevator requests. </summary>
public class ElevatorOrchestratorService(
    ILogger<ElevatorOrchestratorService> logger,
    IInMemoryElevatorPoolService elevatorPoolService,
    IElevatorSelectionContext selectionContext
    ) : IElevatorOrchestratorService
{

    /// <summary> Request an elevator to a specific floor in a building. </summary>
    public async Task<Result<RequestElevatorResponse>> RequestElevatorAsync(Guid buildingId, int floor, CancellationToken cancellationToken)
    {
        logger.LogInformation("Requesting elevator to floor {Floor} in building {BuildingId}", floor, buildingId);
        var elevators = await elevatorPoolService.GetAllElevatorsAsync(buildingId, cancellationToken);
        if (elevators.IsFailure)
        {
            logger.LogError("Failed to retrieve elevators for building {BuildingId}", buildingId);
            return Result.Failure<RequestElevatorResponse>(elevators.Error);
        }

        // Use strategy pattern to select the most appropriate elevator
        var selectedElevator = selectionContext.SelectElevator(elevators.Value,
            floor);
        if (selectedElevator.IsFailure)
        {
            logger.LogWarning("No elevators available to service request to floor {Floor} in building {BuildingId}", floor, buildingId);
            return Result.Failure<RequestElevatorResponse>(ElevatorErrors.NoElevatorsAvailable());
        }

        // Queue the request to the selected elevator
        selectedElevator.Value.DestinationFloors.Enqueue(floor);

        await elevatorPoolService.UpdateElevatorAsync(selectedElevator.Value, cancellationToken);

        logger.LogInformation("Elevator {ElevatorId} has been requested to floor {Floor} in building {BuildingId}",
            selectedElevator.Value.Id, floor, buildingId);

        return Result.Success(new RequestElevatorResponse(
            IsSuccess: true,
            Message: $"Elevator Request to floor {floor} in building {buildingId} has been successfully queued to elevator {selectedElevator.Value.Id}"));
    }
}




///<inheritdoc cref="IInMemoryElevatorPoolService"/> 
public sealed class InMemoryElevatorPoolService(
    ILogger<InMemoryElevatorPoolService> logger,
    IServiceProvider serviceProvider)
    : IInMemoryElevatorPoolService, IDisposable
{
    private readonly ILogger<InMemoryElevatorPoolService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly ConcurrentDictionary<Guid, ElevatorItem> _elevators = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private DateTime _lastUpdate = DateTime.MinValue; // Initialize to MinValue to force first update
    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(360);
    private bool _disposed;
    private readonly Guid _instanceId = Guid.NewGuid(); 

    ///<inheritdoc cref="IInMemoryElevatorPoolService"/> 
    public async Task<Result<ElevatorItem>> GetElevatorByIdAsync(Guid elevatorId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Instance ID: {InstanceId}", _instanceId);
        _logger.LogInformation("Getting elevator by ID {ElevatorId}", elevatorId);
        try
        {
            await Task.Yield(); // Ensure async context

            // TryGetValue is already thread-safe in ConcurrentDictionary
            if (_elevators.TryGetValue(elevatorId, out var elevator))
            {
                _logger.LogInformation("Elevator found by ID {ElevatorId}", elevatorId);
                // Create a deep copy to ensure thread safety
                return Result.Success(elevator.Clone());
            }

            // If not in cache, try to fetch from database
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var dbElevator = await context.Elevators
                .FirstOrDefaultAsync(e => e.Id == elevatorId, cancellationToken);

            if (dbElevator != null)
            {
                var elevatorItem = ElevatorItem.FromElevator(dbElevator);
                _elevators.TryAdd(elevatorId, elevatorItem);
                return Result.Success(elevatorItem.Clone());
            }

            _logger.LogWarning("Elevator not found by ID {ElevatorId}", elevatorId);
            return Result.Failure<ElevatorItem>(
                new Error("GetElevatorById.NotFound", $"Elevator with ID {elevatorId} not found", ErrorType.NotFound));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting elevator by ID {ElevatorId}", elevatorId);
            return Result.Failure<ElevatorItem>(
                new Error("GetElevatorById.Error", ex.Message, ErrorType.Failure));
        }
    }

    ///<inheritdoc cref="IInMemoryElevatorPoolService"/> 
    public async Task<Result> UpdateElevatorAsync(ElevatorItem elevator, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Instance ID: {InstanceId}", _instanceId);
        _logger.LogInformation("Updating elevator {ElevatorId}", elevator.Id);
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                // Create a deep copy of the elevator to ensure thread safety
                var elevatorCopy = elevator.Clone();

                if (_elevators.TryGetValue(elevatorCopy.Id, out var existingElevator))
                {
                    if (_elevators.TryUpdate(elevatorCopy.Id, elevatorCopy, existingElevator))
                    {
                        _logger.LogInformation("Elevator updated in memory {ElevatorId}", elevator.Id);

                        // Update in database
                        using var scope = _serviceProvider.CreateScope();
                        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

                        var dbElevator = await context.Elevators
                            .FirstOrDefaultAsync(e => e.Id == elevator.Id, cancellationToken);

                        if (dbElevator != null)
                        {
                            // Update database entity with new values
                            dbElevator.CurrentFloor = elevator.CurrentFloor;
                            dbElevator.ElevatorStatus = elevator.ElevatorStatus;
                            dbElevator.ElevatorDirection = elevator.ElevatorDirection;
                            dbElevator.DoorStatus = elevator.DoorStatus;
                            dbElevator.DestinationFloor = elevator.DestinationFloor;
                            dbElevator.DestinationFloors = elevator.DestinationFloors.Count > 0 ? string.Join(",", elevator.DestinationFloors.ToList()) : "";
                            dbElevator.DomainEvents.Add(new ElevatorUpdatedDomainEvent(dbElevator));
                            await context.SaveChangesAsync(cancellationToken);
                        }

                        return Result.Success();
                    }

                    _logger.LogWarning("Failed to update elevator {ElevatorId}", elevator.Id);
                    return Result.Failure(
                        new Error("UpdateElevator.ConcurrencyError", "Failed to update elevator due to concurrent modification", ErrorType.Conflict));
                }

                _logger.LogInformation("Adding elevator {ElevatorId}", elevator.Id);
                if (_elevators.TryAdd(elevatorCopy.Id, elevatorCopy))
                {
                    return Result.Success();
                }

                _logger.LogWarning("Failed to add elevator {ElevatorId}", elevator.Id);
                return Result.Failure(
                    new Error("UpdateElevator.AddError", "Failed to add elevator to the pool", ErrorType.Failure));
            }
            finally
            {
                _semaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating elevator {ElevatorId}", elevator.Id);
            return Result.Failure(
                new Error("UpdateElevator.Error", ex.Message, ErrorType.Failure));
        }
    }

    ///<inheritdoc cref="IInMemoryElevatorPoolService"/> 
    public async Task<Result<IEnumerable<ElevatorItem>>> GetAllElevatorsAsync(Guid buildingId, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Instance ID: {InstanceId}", _instanceId);
        _logger.LogInformation("Getting all elevators in building {BuildingId}", buildingId);
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                // Always update from database if there are no elevators for this building
                var hasElevatorsForBuilding = _elevators.Values.Any(e => e.BuildingId == buildingId);
                var needsUpdate = DateTime.UtcNow - _lastUpdate > _updateInterval || !hasElevatorsForBuilding;

                if (needsUpdate)
                {
                    _logger.LogInformation("Updating elevators from database for building {BuildingId}. LastUpdate: {LastUpdate}", buildingId, _lastUpdate);

                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

                    var elevators = await context.Elevators
                        .AsNoTracking()
                        .Where(e => e.BuildingId == buildingId)
                        .ToListAsync(cancellationToken);

                    // Update only elevators for this building
                    foreach (var elevator in _elevators.Values.Where(e => e.BuildingId == buildingId).ToList())
                    {
                        _elevators.TryRemove(elevator.Id, out _);
                    }

                    foreach (var elevator in elevators)
                    {
                        _elevators.TryAdd(elevator.Id, ElevatorItem.FromElevator(elevator));
                    }

                    _lastUpdate = DateTime.UtcNow;
                }

                // Create a deep copy of each elevator to ensure thread safety
                var allElevators = _elevators.Values
                    .Where(e => e.BuildingId == buildingId)
                    .Select(e => e.Clone())
                    .ToList();

                _logger.LogInformation("Returning {ElevatorCount} elevators in building {BuildingId}",
                    allElevators.Count, buildingId);
                return Result.Success<IEnumerable<ElevatorItem>>(allElevators);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all elevators for building {BuildingId}", buildingId);
            return Result.Failure<IEnumerable<ElevatorItem>>(
                new Error("GetAllElevators.Error", ex.Message, ErrorType.Failure));
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _semaphore.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}


public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}




public interface ITokenProvider
{
    string Create(User user);
}


public interface IUserContext
{
    Guid UserId { get; }
}




internal sealed class RequestLoggingPipelineBehavior<TRequest, TResponse>(
    ILogger<RequestLoggingPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string requestName = typeof(TRequest).Name;

        logger.LogInformation("Processing request {RequestName}", requestName);

        TResponse result = await next();

        if (result.IsSuccess)
        {
            logger.LogInformation("Completed request {RequestName}", requestName);
        }
        else
        {
            using (LogContext.PushProperty("Error", result.Error, true))
            {
                logger.LogError("Completed request {RequestName} with error", requestName);
            }
        }

        return result;
    }
}




internal sealed class ValidationPipelineBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators,
    ILogger<ValidationPipelineBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Validating request {Request}", request);
        ValidationFailure[] validationFailures = await ValidateAsync(request);

        logger.LogInformation("Validation failures Count: {ValidationFailures}", validationFailures.Length);
        if (validationFailures.Length == 0)
        {
            return await next();
        }

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
        {
            logger.LogInformation("Returning validation failure result");
            Type resultType = typeof(TResponse).GetGenericArguments()[0];
            logger.LogInformation("Result type: {ResultType}", resultType);

            MethodInfo? failureMethod = typeof(Result<>)
                .MakeGenericType(resultType)
                .GetMethod(nameof(Result<object>.ValidationFailure));

            logger.LogInformation("Failure method: {FailureMethod}", failureMethod);
            if (failureMethod is not null)
            {
                logger.LogInformation("Invoking failure method");
                return (TResponse)failureMethod.Invoke(
                    null,
                    [CreateValidationError(validationFailures)]);
            }
        }
        else if (typeof(TResponse) == typeof(Result))
        {
            logger.LogInformation("Returning validation failure result");
            return (TResponse)(object)Result.Failure(CreateValidationError(validationFailures));
        }

        logger.LogInformation("Throwing validation exception");
        throw new ValidationException(validationFailures);
    }

    private async Task<ValidationFailure[]> ValidateAsync(TRequest request)
    {
        logger.LogInformation("Validating request {Request}", request);
        logger.LogInformation("Validators Count: {ValidatorsCount}", validators.Count());
        if (!validators.Any())
        {
            return [];
        }

        logger.LogInformation("Validating request {Request}", request);
        var context = new ValidationContext<TRequest>(request);

        
        ValidationResult[] validationResults = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(context)));
        
        logger.LogInformation("Validation results Count: {ValidationResultsCount}", validationResults.Length);

        ValidationFailure[] validationFailures = validationResults
            .Where(validationResult => !validationResult.IsValid)
            .SelectMany(validationResult => validationResult.Errors)
            .ToArray();

        logger.LogInformation("Validation failures Count: {ValidationFailuresCount}", validationFailures.Length);
        return validationFailures;
    }

    private static ValidationError CreateValidationError(ValidationFailure[] validationFailures) =>
        new(validationFailures.Select(f => Error.Problem(f.ErrorCode, f.ErrorMessage)).ToArray());
}


/// <summary> Application database context. </summary>
public interface IApplicationDbContext
{
    /// <summary> Get the DbSet of User entities. </summary>
    DbSet<User> Users { get; }
    /// <summary> Get the DbSet of Building entities. </summary>
    DbSet<Building> Buildings { get; }
    /// <summary> Get the DbSet of Elevator entities. </summary>
    DbSet<Elevator> Elevators { get; }
    /// <summary> Save the changes to the database. </summary>
    /// <param name="cancellationToken"> The cancellation token. </param>
    /// <returns> The number of state entries written to the database. </returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}




public interface ICommand : IRequest<Result>, IBaseCommand;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand;

public interface IBaseCommand;



public interface ICommandHandler<in TCommand>
    : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

public interface ICommandHandler<in TCommand, TResponse>
    : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;




public interface IQuery<TResponse> : IRequest<Result<TResponse>>;



public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;




/// <summary>
/// Represents a screen in the application.
/// </summary>
/// <typeparam name="TResult">The type of result this screen produces.</typeparam>
public interface IScreen<TResult>
{
    /// <summary>
    /// Gets the supported result type for this screen.
    /// </summary>
    Type ResultType => typeof(TResult);

    /// <summary>
    /// Shows the screen and handles user interaction.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns>A result containing the screen's output.</returns>
    Task<Result<TResult>> ShowAsync(CancellationToken token);
}


/// <summary> Strategy that selects the closest elevator to the requested floor. </summary>
public interface IClosestElevatorStrategy : IElevatorSelectionStrategy { }




/// <summary> Elevator Orchestrator service for managing elevator requests. </summary>
public interface IElevatorOrchestratorService
{
    /// <summary> Requests an elevator to a specific floor in a building. </summary>
    Task<Result<RequestElevatorResponse>> RequestElevatorAsync(Guid buildingId, int floor, CancellationToken cancellationToken);
}

/// <summary> Response for requesting an elevator. </summary>
public record RequestElevatorResponse(bool IsSuccess, string Message);



/// <summary> Elevator selection context for selecting an elevator based on a strategy. </summary>
public interface IElevatorSelectionContext
{
    /// <summary> Selects an elevator based on a strategy. </summary>
    Result<ElevatorItem> SelectElevator(IEnumerable<ElevatorItem> elevators, int requestedFloor);
}



/// <summary> Interface for elevator selection strategies </summary>
public interface IElevatorSelectionStrategy
{
    /// <summary> Selects the most appropriate elevator based on the strategy's criteria </summary>
    /// <param name="elevators">Available elevators</param>
    /// <param name="requestedFloor">The floor where the request originated</param>
    /// <returns>Result containing the selected elevator or an error</returns>
    Result<IEnumerable<ElevatorItem>> SelectElevator(
        IEnumerable<ElevatorItem> elevators,
        int requestedFloor);
}



/// <summary> Interface for managing the in-memory pool of elevators. </summary>
public interface IInMemoryElevatorPoolService
{
    /// <summary> Gets an elevator by its ID. </summary>
    /// <param name="elevatorId">The ID of the elevator.</param>
    /// <param name="cancellationToken">The cancellation token for cancelling the operation.</param>
    /// <returns>A Result containing the elevator if found.</returns>
    Task<Result<ElevatorItem>> GetElevatorByIdAsync(Guid elevatorId, CancellationToken cancellationToken);

    /// <summary> Updates an elevator's information in the pool. </summary>
    /// <param name="elevator">The elevator with updated information.</param>
    /// <param name="cancellationToken">The cancellation token for cancelling the operation.</param>
    /// <returns>A Result indicating success or failure.</returns>
    Task<Result> UpdateElevatorAsync(ElevatorItem elevator, CancellationToken cancellationToken);

    /// <summary> Gets all elevators in a building. </summary>
    /// <param name="buildingId">The ID of the building.</param>
    /// <param name="cancellationToken">The cancellation token for cancelling the operation.</param>
    /// <returns>A Result containing a list of all elevators in the building.</returns>
    Task<Result<IEnumerable<ElevatorItem>>> GetAllElevatorsAsync(Guid buildingId, CancellationToken cancellationToken);
}


/// <summary> Strategy that selects an elevator based on its current queue capacity. </summary>
public interface IQueueCapacityStrategy : IElevatorSelectionStrategy { }




public sealed record CreateBuildingCommand(string Name, int NumberOfFloors)
    : ICommand<Guid>;



public sealed class CreateBuildingCommandHandler(
    IApplicationDbContext applicationDbContext,
    IUserContext userContext) : ICommandHandler<CreateBuildingCommand,Guid>
{
    public async Task<Result<Guid>> Handle(CreateBuildingCommand request, CancellationToken cancellationToken)
    {
        //check if building with the same name exists
        Domain.Buildings.Building? building = await applicationDbContext.Buildings
            .SingleOrDefaultAsync(a => a.Name == request.Name, cancellationToken);

        if (building is not null)
        {
            return Result.Failure<Guid>(BuildingErrors.NameNotUnique(request.Name));
        }
        
        //create the building
        building = new Domain.Buildings.Building
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            NumberOfFloors = request.NumberOfFloors
        };
        
        building.Raise(new BuildingCreatedDomainEvent(building.Id));
        
        applicationDbContext.Buildings.Add(building);
        
        await applicationDbContext.SaveChangesAsync(cancellationToken);
        
        return building.Id;
    }
}




internal sealed class CreateBuildingCommandValidator
    : AbstractValidator<CreateBuildingCommand>
{
    public CreateBuildingCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.NumberOfFloors)
            .GreaterThan(0);
    }
}




/// <summary> Command to update a building's name and floors </summary>
public sealed record UpdateBuildingCommand(Guid Id, string Name, int NumberOfFloors)
    : ICommand;



public class UpdateBuildingCommandHandler(
    IApplicationDbContext applicationDbContext) : ICommandHandler<UpdateBuildingCommand>
{
    public async Task<Result> Handle(UpdateBuildingCommand request, CancellationToken cancellationToken)
    {
        Building? building = await applicationDbContext.Buildings
            .SingleOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (building is null)
        {
            return Result.Failure(BuildingErrors.NotFound(request.Id));
        }

        //update the building
        building.Name = request.Name;
        building.NumberOfFloors = request.NumberOfFloors;

        applicationDbContext.Buildings.Update(building);

        await applicationDbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}




public class UpdateBuildingCommandValidator : AbstractValidator<UpdateBuildingCommand>
{
    public UpdateBuildingCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 200 characters");
        
        RuleFor(x => x.NumberOfFloors)
            .GreaterThan(0).WithMessage("Number of floors must be greater than 0");
    }
}



internal sealed class ElevatorUpdatedDomainEventHandler(ILogger<ElevatorUpdatedDomainEventHandler> logger) : INotificationHandler<ElevatorUpdatedDomainEvent>
{
    public Task Handle(ElevatorUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Elevator updated with the following state: {@Elevator}", notification.Elevator);
        return Task.CompletedTask;
    }
}




/// <summary> Command to request an elevator to a specific floor. </summary>
public record RequestElevatorCommand(Guid BuildingId, int FloorNumber) : ICommand<Guid>;




/// <summary> Request an elevator to a specific floor. </summary>
public class RequestElevatorCommandHandler(
    ILogger<RequestElevatorCommandHandler> logger,
    IElevatorOrchestratorService elevatorOrchestratorService,
    IApplicationDbContext applicationDbContext
    ) : ICommandHandler<RequestElevatorCommand,Guid>
{
    /// <summary> Handle the request to send an elevator to a specific floor. </summary>
    public async Task<Result<Guid>> Handle(RequestElevatorCommand request, CancellationToken cancellationToken)
    {
        //Check if building exists
        var building = await applicationDbContext.Buildings.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == request.BuildingId, cancellationToken);

        if (building == null)
        {
            return Result.Failure<Guid>(BuildingErrors.NotFound(request.BuildingId));
        }
        
        if (request.FloorNumber < 1 || request.FloorNumber > building.NumberOfFloors)
        {
            return Result.Failure<Guid>(BuildingErrors.InvalidFloorNumber(request.FloorNumber));
        }
        
        logger.LogInformation("Requesting elevator to floor {Floor}", request.FloorNumber);
        var requestResponse =  await elevatorOrchestratorService
            .RequestElevatorAsync(request.BuildingId, request.FloorNumber, cancellationToken);
        
        if (requestResponse.IsFailure)
        {
            logger.LogError("Failed to request elevator to floor {Floor}", request.FloorNumber);
            return Result.Failure<Guid>(ElevatorErrors.ElevatorRequestFailed(request.FloorNumber));
        }
        
        logger.LogInformation("Elevator requested to floor {Floor}", request.FloorNumber);
        return Guid.NewGuid();
    }
}




public class RequestElevatorCommandValidator : AbstractValidator<RequestElevatorCommand>
{
    public RequestElevatorCommandValidator() 
    {
        RuleFor(x => x.BuildingId)
            .NotEmpty();

        RuleFor(x => x.FloorNumber)
            .NotEmpty();
        
        RuleFor(x => x.FloorNumber)
            .GreaterThan(0);
    }
}



/// <inheritdoc />
public class ClosestElevatorStrategy(ILogger<ClosestElevatorStrategy> logger)
    : IClosestElevatorStrategy
{
    /// <inheritdoc />
    public Result<IEnumerable<ElevatorItem>> SelectElevator(
        IEnumerable<ElevatorItem> elevators,
        int requestedFloor)
    {
        logger.LogInformation("Selecting closest elevator to floor {RequestedFloor}", requestedFloor);
        var availableElevators = elevators.Where(e =>
            e.ElevatorStatus != ElevatorStatus.OutOfService &&
            e.ElevatorStatus != ElevatorStatus.Maintenance);

        IEnumerable<ElevatorItem> elevatorItems = availableElevators as ElevatorItem[] ?? availableElevators.ToArray();
        logger.LogInformation("Found {ElevatorCount} available elevators", elevatorItems.Count());
        if (!elevatorItems.Any())
        {
            return Result.Failure<IEnumerable<ElevatorItem>>(
                ElevatorSectionErrors.NoElevatorsAvailable());
        }

        logger.LogInformation("Selecting closest elevator to floor {RequestedFloor}", requestedFloor);
        var closestElevators = elevatorItems
            .OrderBy(e => Math.Abs(e.CurrentFloor - requestedFloor))
            .ToArray();
                
        return Result.Success(closestElevators.AsEnumerable());
    }
}



// <inheritdoc />
public class ElevatorSelectionContext(
    ILogger<ElevatorSelectionContext> logger,
    IClosestElevatorStrategy closestElevatorStrategy,
    IQueueCapacityStrategy queueCapacityStrategy) : IElevatorSelectionContext
{
    // <inheritdoc />
    public Result<ElevatorItem> SelectElevator(
        IEnumerable<ElevatorItem> elevators,
        int requestedFloor)
    {
        
        logger.LogInformation("Selecting elevator for floor {RequestedFloor}", requestedFloor);
        var results = closestElevatorStrategy.SelectElevator(elevators, requestedFloor);
        if (results.IsFailure)
        {
            logger.LogWarning("Failed to select elevator using closest strategy. No elevators available");
            return Result.Failure<ElevatorItem>(ElevatorSectionErrors.NoElevatorsAvailable());
        }
        
        logger.LogInformation("Selected elevator using closest strategy and not at capacity");
        results = queueCapacityStrategy.SelectElevator(results.Value, requestedFloor);
        if (results.IsFailure)
        {
            logger.LogWarning("Failed to select elevator using queue capacity strategy. No elevators available");
            return Result.Failure<ElevatorItem>(ElevatorSectionErrors.NoElevatorsAvailable());
        }
        
        return results.Value.FirstOrDefault();
        
    }
}



/// <summary> Strategy that selects an elevator based on its current queue capacity </summary>
public class QueueCapacityStrategy(ILogger<QueueCapacityStrategy> logger) : IQueueCapacityStrategy
{
    // <inheritdoc />
    public Result<IEnumerable<ElevatorItem>> SelectElevator(
        IEnumerable<ElevatorItem> elevators,
        int requestedFloor)
    {
        logger.LogInformation("Selecting elevator with available queue capacity to floor {RequestedFloor}", requestedFloor);
        var availableElevators = elevators.Where(e =>
            e.ElevatorStatus != ElevatorStatus.OutOfService &&
            e.ElevatorStatus != ElevatorStatus.Maintenance &&
            e.DestinationFloors.Count < e.QueueCapacity);

        IEnumerable<ElevatorItem> elevatorItems = availableElevators as ElevatorItem[] ?? availableElevators.ToArray();
        logger.LogInformation("Found {ElevatorCount} available elevators with queue capacity", elevatorItems.Count());
        if (!elevatorItems.Any())
        {
            return Result.Failure<IEnumerable<ElevatorItem>>(
                ElevatorSectionErrors.NoElevatorsAvailable());
        }

        logger.LogInformation("Selecting elevator with available queue capacity to floor {RequestedFloor}", requestedFloor);
        var selectedElevator = elevatorItems
            .OrderBy(e => e.DestinationFloors.Count)
            .ToList();
        
        return Result.Success(selectedElevator.AsEnumerable());
    }
}




public sealed record GetUserByEmailQuery(string Email) : IQuery<UserResponse>;



internal sealed class GetUserByEmailQueryHandler(IApplicationDbContext context, IUserContext userContext)
    : IQueryHandler<GetUserByEmailQuery, UserResponse>
{
    public async Task<Result<UserResponse>> Handle(GetUserByEmailQuery query, CancellationToken cancellationToken)
    {
        UserResponse? user = await context.Users
            .Where(u => u.Email == query.Email)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserResponse>(UserErrors.NotFoundByEmail);
        }

        if (user.Id != userContext.UserId)
        {
            return Result.Failure<UserResponse>(UserErrors.Unauthorized());
        }

        return user;
    }
}


public sealed record UserResponse
{
    public Guid Id { get; init; }

    public string Email { get; init; }

    public string FirstName { get; init; }

    public string LastName { get; init; }
}


public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserResponseByIdQuery>;


public sealed class GetUserByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetUserByIdQuery, UserResponseByIdQuery>
{
    public async Task<Result<UserResponseByIdQuery>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        UserResponseByIdQuery? user = await context.Users
            .Where(u => u.Id == query.UserId)
            .Select(u => new UserResponseByIdQuery
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserResponseByIdQuery>(UserErrors.NotFound(query.UserId));
        }

        return user;
    }
}


public sealed record UserResponseByIdQuery
{
    public Guid Id { get; init; }

    public string Email { get; init; }

    public string FirstName { get; init; }

    public string LastName { get; init; }
}


public sealed record LoginUserCommand(string Email, string Password) : ICommand<string>;



internal sealed class LoginUserCommandHandler(
    IApplicationDbContext context,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider) : ICommandHandler<LoginUserCommand, string>
{
    public async Task<Result<string>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        User? user = await context.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Email == command.Email, cancellationToken);

        if (user is null)
        {
            return Result.Failure<string>(UserErrors.NotFoundByEmail);
        }

        bool verified = passwordHasher.Verify(command.Password, user.PasswordHash);

        if (!verified)
        {
            return Result.Failure<string>(UserErrors.NotFoundByEmail);
        }

        string token = tokenProvider.Create(user);

        return token;
    }
}


public sealed record RegisterUserCommand(string Email, string FirstName, string LastName, string Password)
    : ICommand<Guid>;

internal sealed class RegisterUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(u => u.Email == command.Email, cancellationToken))
        {
            return Result.Failure<Guid>(UserErrors.EmailNotUnique);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            FirstName = command.FirstName,
            LastName = command.LastName,
            PasswordHash = passwordHasher.Hash(command.Password)
        };

        user.Raise(new UserRegisteredDomainEvent(user.Id));

        context.Users.Add(user);

        await context.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}


internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(c => c.FirstName).NotEmpty().WithMessage("First name is required.");
        RuleFor(c => c.LastName).NotEmpty().WithMessage("Last name is required.");
        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email address is required.");
        RuleFor(c => c.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");
    }
}

internal sealed class UserRegisteredDomainEventHandler : INotificationHandler<UserRegisteredDomainEvent>
{
    public Task Handle(UserRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        // TODO: Send an email verification link, etc.
        return Task.CompletedTask;
    }
}

