using Domain.Common;

namespace Application.Abstractions.Services;

/// <summary> Elevator Orchestrator service for managing elevator requests. </summary>
public interface IElevatorOrchestratorService
{
    /// <summary> Requests an elevator to a specific floor in a building. </summary>
    Task<Result<RequestElevatorResponse>> RequestElevatorAsync(Guid buildingId, int floor, CancellationToken cancellationToken);
}

/// <summary> Response for requesting an elevator. </summary>
public record RequestElevatorResponse(bool IsSuccess, string Message);
