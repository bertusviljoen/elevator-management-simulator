namespace SharedKernel;

/// <summary> Base abstract class for entities that have audit and timestamp tracking. </summary>
public abstract class AuditableEntity 
    : Entity, ITimeAuditedEntity, IUserAuditedEntity
{
    /// <inheritdoc />
    public DateTime CreatedDateTimeUtc { get; set; }
    /// <inheritdoc />
    public DateTime? UpdatedDateTimeUtc { get; set; }
    /// <inheritdoc />
    public Guid CreatedByUserId { get; set; }
    /// <inheritdoc />
    public Guid? UpdatedByUserId { get; set; }
}
