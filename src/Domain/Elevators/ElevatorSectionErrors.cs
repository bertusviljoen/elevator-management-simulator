using Domain.Common;

namespace Domain.Elevators;

public static class ElevatorSectionErrors
{
    public static Error NoElevatorsAvailable() => Error.Problem(
        "ElevatorSection.NoElevatorsAvailable",
            "No elevators are available to service the request.");
    public static Error NoElevatorFound() => Error.NotFound(
        "ElevatorSection.NoElevatorFound",
            $"No elevator found");
}
