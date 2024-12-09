using Domain.Common;
using Domain.Elevators;

namespace Application.Abstractions.Services;

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
