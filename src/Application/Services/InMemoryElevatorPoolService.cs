using System.Collections.Concurrent;
using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Domain.Common;
using Domain.Elevators;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Services;

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
