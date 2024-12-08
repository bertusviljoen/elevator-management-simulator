namespace Domain.Common;

/// <summary> Describes database entities that tracks timestamps when data is modified. </summary>
public interface ITimeAuditedEntity
{
    /// <summary> The timestamp of when the entity was created. This is required. </summary>
    DateTime CreatedDateTimeUtc { get; set; }
    /// <summary> The timestamp of when the entity was modified.</summary>
    DateTime? UpdatedDateTimeUtc { get; set; }
}
