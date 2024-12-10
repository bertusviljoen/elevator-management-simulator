using Domain.Common;

namespace Domain.Elevators;

/// <summary> Domain event for when an elevator is updated. </summary>
public record ElevatorUpdatedDomainEvent(Elevator Elevator) : IDomainEvent;


