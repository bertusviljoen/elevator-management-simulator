using Application;
using Domain.Users;
using Microsoft.EntityFrameworkCore;
using Domain.Common;

namespace Application.Users;

internal sealed class GetUserByIdQueryHandler(IApplicationDbContext context)
    : IQueryHandler<GetUserByIdQuery, UserResponseByIdQuery>
{
    public async Task<Result<UserResponseByIdQuery>> Handle(GetUserByIdQuery query, CancellationToken cancellationToken)
    {
        UserResponseByIdQuery? user = await context.Users
            .Where(u => u.Id == query.UserId)
            .Select(u => new UserResponseByIdQuery
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserResponseByIdQuery>(UserErrors.NotFound(query.UserId));
        }

        return user;
    }
}
