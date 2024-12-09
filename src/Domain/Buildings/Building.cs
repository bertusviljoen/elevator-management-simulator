using Domain.Common;

namespace Domain.Buildings;

/// <summary> Entity representing a building. </summary>
public sealed class Building : AuditableEntity
{
    /// <summary> Get or set the unique identifier for the building. </summary>
    public required Guid Id { get; init; }
    /// <summary> Get or set the name of the building. </summary>
    public required string Name { get; set; }
    /// <summary> Get or set the number of floors in the building. </summary>
    public required int NumberOfFloors { get; set; }

    /// <summary> Get or set if this building is the default building. </summary>
    public bool IsDefault { get; set; } = true;
}
