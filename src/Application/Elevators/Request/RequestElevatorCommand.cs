using Application;

namespace Application;

/// <summary> Command to request an elevator to a specific floor. </summary>
public record RequestElevatorCommand(Guid BuildingId, int FloorNumber) : ICommand<Guid>;
