using SharedKernel;

namespace Domain.Users;

/// <summary> Entity representing a building. </summary>
public class Building : AuditableEntity
{
    /// <summary> Get or set the unique identifier for the building. </summary>
    public required Guid Id { get; init; }
    /// <summary> Get or set the name of the building. </summary>
    public required string Name { get; init; }
    /// <summary> Get or set the number of floors in the building. </summary>
    public required int NumberOfFloors { get; init; }
}