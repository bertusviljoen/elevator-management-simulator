using FluentValidation;

namespace Application.Buildings.Update;

public class UpdateBuildingCommandValidator : AbstractValidator<UpdateBuildingCommand>
{
    public UpdateBuildingCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 200 characters");
        
        RuleFor(x => x.NumberOfFloors)
            .GreaterThan(0).WithMessage("Number of floors must be greater than 0");
    }
}
