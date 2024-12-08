using Domain.Users;

namespace Domain.Common;

/// <summary> Describes database entities that tracks users that modify data. </summary>
public interface IUserAuditedEntity
{
    /// <summary> The user that created the entity. This is required. </summary>
    Guid CreatedByUserId { get; set; }

    /// <summary> The user that modified the entity. This is required and will default to <seealso cref="CreatedByUserId"/>. </summary>
    Guid? UpdatedByUserId { get; set; }

    /// <summary> Created by User which is a navigation property to the User entity. </summary>
    public abstract  User CreatedByUser { get; set; }
    /// <summary> Updated by User which is a navigation property to the User entity. </summary>
    public abstract User? UpdatedByUser { get; set; }
}
