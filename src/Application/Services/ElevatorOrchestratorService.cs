using Application.Abstractions.Services;
using Domain.Common;
using Domain.Elevators;
using Microsoft.Extensions.Logging;

namespace Application.Services;

/// <summary> Service for orchestrating elevator requests. </summary>
public class ElevatorOrchestratorService(
    ILogger<ElevatorOrchestratorService> logger,
    IInMemoryElevatorPoolService elevatorPoolService) : IElevatorOrchestratorService
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
        
        //ToDo: Will do strategy pattern here next to see which elevator to call based on the request
        
        // Find the closest elevator to the requested floor
        var closestElevator = elevators.Value
            .Where(elevator => elevator.ElevatorStatus == ElevatorStatus.Active)
            .OrderBy(elevator => Math.Abs(elevator.CurrentFloor - floor))
            .FirstOrDefault();
        
        if (closestElevator == null)
        {
            logger.LogWarning("No elevators available to service request to floor {Floor} in building {BuildingId}", floor, buildingId);
            return Result.Failure<RequestElevatorResponse>(ElevatorErrors.NoElevatorsAvailable());
        }
        
        //Queue the request to the elevator
        closestElevator.DestinationFloors.Enqueue(floor);
        
        await elevatorPoolService.UpdateElevatorAsync(closestElevator, cancellationToken);
        
        logger.LogInformation("Elevator {ElevatorId} has been requested to floor {Floor} in building {BuildingId}", closestElevator.Id, floor, buildingId);
        
        return Result.Success(new RequestElevatorResponse(true,
            $"Elevator Request to floor {floor} in building {buildingId} has been successfully queued to elevator {closestElevator.Id}"));
        
    }
}
