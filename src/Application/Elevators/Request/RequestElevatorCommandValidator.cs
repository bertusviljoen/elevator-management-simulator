using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Application;

public class RequestElevatorCommandValidator : AbstractValidator<RequestElevatorCommand>
{
    public RequestElevatorCommandValidator() 
    {
        RuleFor(x => x.BuildingId)
            .NotEmpty();

        RuleFor(x => x.FloorNumber)
            .NotEmpty();
        
        RuleFor(x => x.FloorNumber)
            .GreaterThan(0);
    }
}
