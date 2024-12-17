using Domain.Common;
using Domain.Elevators;

namespace Application;

/// <summary> Elevator selection context for selecting an elevator based on a strategy. </summary>
public interface IElevatorSelectionContext
{
    /// <summary> Selects an elevator based on a strategy. </summary>
    Result<ElevatorItem> SelectElevator(IEnumerable<ElevatorItem> elevators, int requestedFloor);
}
