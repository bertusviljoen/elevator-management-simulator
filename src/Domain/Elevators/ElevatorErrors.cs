using Domain.Common;

namespace Domain.Elevators;

public static class ElevatorErrors
{
    public static Error NoElevatorFound(Guid elevatorId) => Error.NotFound(
        "Elevator.NotFound",
            $"No elevator found with ID {elevatorId}.");
    
    public static Error NoElevatorFound(int elevatorNumber) => Error.NotFound(
        "Elevator.NotFound",
            $"No elevator found with number {elevatorNumber}.");
    
    public static Error ElevatorRequestFailed(int floorNumber) => Error.Problem(
        "Elevator.RequestFailed",
            $"The request to the elevator service failed for floor number {floorNumber}.");
    
    public static Error NoElevatorsAvailable() => Error.Problem(
        "Elevator.NoElevatorsAvailable",
            "No elevators are available to service the request.");
}
