using Domain.Common;
using Domain.Elevators;
using Microsoft.Extensions.Logging;

namespace Application.Services.ElevatorSelection;

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
