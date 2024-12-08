namespace SharedKernel;

/// <summary> Describes database entities that tracks users that modify data. </summary>
public interface IUserAuditedEntity
{
    /// <summary> The user that created the entity. This is required. </summary>
    Guid CreatedByUserId { get; set; }

    /// <summary> The user that modified the entity. This is required and will default to <seealso cref="CreatedByUserId"/>. </summary>
    Guid? UpdatedByUserId { get; set; }

}
