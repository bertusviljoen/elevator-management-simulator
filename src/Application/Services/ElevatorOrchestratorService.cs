using Application.Abstractions.Services;
using Application.Services.ElevatorSelection;
using Domain.Common;
using Domain.Elevators;
using Microsoft.Extensions.Logging;

namespace Application.Services;

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
