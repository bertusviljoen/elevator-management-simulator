using Application;

namespace Application;

public sealed record GetUserByEmailQuery(string Email) : IQuery<UserResponse>;
