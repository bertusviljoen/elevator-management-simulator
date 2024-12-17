using Application;

namespace Application.Users;

public sealed record LoginUserCommand(string Email, string Password) : ICommand<string>;
