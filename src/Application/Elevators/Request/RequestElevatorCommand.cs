using Application.Abstractions.Messaging;

namespace Application.Elevators.Request;

/// <summary> Command to request an elevator to a specific floor. </summary>
public record RequestElevatorCommand(Guid BuildingId, int FloorNumber) : ICommand<Guid>;
