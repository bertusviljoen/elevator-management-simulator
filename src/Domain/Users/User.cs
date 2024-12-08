using Domain.Common;

namespace Domain.Users;

/// <summary> The following class represents a user entity. </summary>
public sealed class User : Entity
{
    /// <summary> Get the user's email. </summary>
    public required Guid Id { get; init; }
    /// <summary> Get the user's email. </summary>
    public required string Email { get; init; }
    /// <summary> Get the user's first name. </summary>
    public required string FirstName { get; init; }
    /// <summary> Get the user's last name. </summary>
    public required string LastName { get; init; }
    /// <summary> Get the user's password hash. </summary>
    public required string PasswordHash { get; init; }
}
