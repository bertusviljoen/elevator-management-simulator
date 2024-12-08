using FluentValidation;

namespace Application.Buildings.Create;

internal sealed class CreateBuildingCommandValidator
    : AbstractValidator<CreateBuildingCommand>
{
    public CreateBuildingCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.NumberOfFloors)
            .GreaterThan(0);
    }
}
