using Application;

namespace Application.Users;

public sealed record GetUserByIdQuery(Guid UserId) : IQuery<UserResponseByIdQuery>;
