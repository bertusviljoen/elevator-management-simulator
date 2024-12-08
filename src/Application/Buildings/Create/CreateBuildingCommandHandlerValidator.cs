using FluentValidation;

namespace Application.Building.Create;

internal sealed class CreateBuildingCommandHandlerValidator
    : AbstractValidator<CreateBuildingCommand>
{
    public CreateBuildingCommandHandlerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.NumberOfFloors)
            .GreaterThan(0);
    }
}
