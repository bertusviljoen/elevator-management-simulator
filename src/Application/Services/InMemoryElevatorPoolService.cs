using System.Collections.Concurrent;
using Application.Abstractions.Data;
using Application.Abstractions.Services;
using Domain.Common;
using Domain.Elevators;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Application.Services;

///<inheritdoc cref="IInMemoryElevatorPoolService"/> 
public class InMemoryElevatorPoolService(
    ILogger<InMemoryElevatorPoolService> logger,
    IApplicationDbContext context)
    : IInMemoryElevatorPoolService, IDisposable
{
    private readonly ConcurrentDictionary<Guid, ElevatorItem> _elevators = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    ///<inheritdoc cref="IInMemoryElevatorPoolService"/> 
    public async Task<Result<ElevatorItem>> GetElevatorByIdAsync(Guid elevatorId, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Yield(); // Ensure async context

            // TryGetValue is already thread-safe in ConcurrentDictionary
            if (_elevators.TryGetValue(elevatorId, out var elevator))
            {
                // Create a deep copy to ensure thread safety
                return Result.Success(elevator.Clone());
            }

            return Result.Failure<ElevatorItem>(
                new Error("GetElevatorById.NotFound", $"Elevator with ID {elevatorId} not found", ErrorType.NotFound));
        }
        catch (Exception ex)
        {
            return Result.Failure<ElevatorItem>(
                new Error("GetElevatorById.Error", ex.Message, ErrorType.Failure));
        }
    }

    ///<inheritdoc cref="IInMemoryElevatorPoolService"/> 
    public async Task<Result> UpdateElevatorAsync(ElevatorItem elevator, CancellationToken cancellationToken)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                await Task.Yield(); // Ensure async context

                // Create a deep copy of the elevator to ensure thread safety
                var elevatorCopy = elevator.Clone();

                if (_elevators.TryGetValue(elevatorCopy.Id, out var existingElevator))
                {
                    if (_elevators.TryUpdate(elevatorCopy.Id, elevatorCopy, existingElevator))
                    {
                        return Result.Success();
                    }

                    return Result.Failure(
                        new Error("UpdateElevator.ConcurrencyError", "Failed to update elevator due to concurrent modification", ErrorType.Conflict));
                }

                if (_elevators.TryAdd(elevatorCopy.Id, elevatorCopy))
                {
                    return Result.Success();
                }

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
            return Result.Failure(
                new Error("UpdateElevator.Error", ex.Message, ErrorType.Failure));
        }
    }

    ///<inheritdoc cref="IInMemoryElevatorPoolService"/> 
    public async Task<Result<IEnumerable<ElevatorItem>>> GetAllElevatorsAsync(Guid buildingId, CancellationToken cancellationToken)
    {
        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                await Task.Yield(); // Ensure async context

                // Create a deep copy of each elevator to ensure thread safety
                var allElevators = _elevators.Values
                    .Where(e => e.BuildingId == buildingId)
                    .Select(e => e.Clone())
                    .ToList();

                return Result.Success<IEnumerable<ElevatorItem>>(allElevators);
            }
            finally
            {
                _semaphore.Release();
            }
        }
        catch (Exception ex)
        {
            return Result.Failure<IEnumerable<ElevatorItem>>(
                new Error("GetAllElevators.Error", ex.Message, ErrorType.Failure));
        }
    }

    public void Dispose()
    {
        _semaphore.Dispose();
    }
}
