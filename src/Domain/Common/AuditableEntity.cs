using System.ComponentModel.DataAnnotations;
using Domain.Users;

namespace Domain.Common;

/// <summary> Base abstract class for entities that have audit and timestamp tracking. </summary>
public abstract class AuditableEntity 
    : Entity, ITimeAuditedEntity, IUserAuditedEntity
{
    /// <inheritdoc />
    [Required]
    public DateTime CreatedDateTimeUtc { get; set; }
    /// <inheritdoc />
    public DateTime? UpdatedDateTimeUtc { get; set; }
    /// <inheritdoc />
    [Required]
    public Guid CreatedByUserId { get; set; }
    /// <inheritdoc />
    public Guid? UpdatedByUserId { get; set; }
    /// <inheritdoc />
    [Required]
    public User CreatedByUser { get; set; }
    /// <inheritdoc />
    public User? UpdatedByUser { get; set; }
}
