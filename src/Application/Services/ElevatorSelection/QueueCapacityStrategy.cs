using Domain.Common;
using Domain.Elevators;
using Microsoft.Extensions.Logging;

namespace Application.Services.ElevatorSelection;

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
