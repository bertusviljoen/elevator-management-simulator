using Domain.Common;
using Domain.Elevators;
using Microsoft.Extensions.Logging;

namespace Application.Services.ElevatorSelection;

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
