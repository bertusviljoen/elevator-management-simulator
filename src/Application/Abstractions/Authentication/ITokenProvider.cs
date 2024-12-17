using Domain.Users;

namespace Application;

public interface ITokenProvider
{
    string Create(User user);
}
