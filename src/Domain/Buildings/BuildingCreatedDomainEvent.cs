namespace Domain;

/// <summary> Event that is raised when a new building is created. </summary>
/// <param name="BuildingId"> The unique identifier of the building. </param>
public sealed record BuildingCreatedDomainEvent(Guid BuildingId) : IDomainEvent;
