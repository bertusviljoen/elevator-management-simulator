using Domain.Elevators;
using Domain.Common;

namespace Application.Services.ElevatorSelection;

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
