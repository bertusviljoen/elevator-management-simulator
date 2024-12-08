using Domain.Users;
using FluentValidation;

namespace Application.Users.Register;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(c => c.FirstName).NotEmpty().WithMessage("First name is required.");
        RuleFor(c => c.LastName).NotEmpty().WithMessage("Last name is required.");
        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email address is required.");
        RuleFor(c => c.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");
    }
}
