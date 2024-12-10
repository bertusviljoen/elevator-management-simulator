using Domain.Elevators;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Elevators.Request;

internal sealed class ElevatorUpdatedDomainEventHandler(ILogger<ElevatorUpdatedDomainEventHandler> logger) : INotificationHandler<ElevatorUpdatedDomainEvent>
{
    public Task Handle(ElevatorUpdatedDomainEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Elevator updated with the following state: {@Elevator}", notification.Elevator);
        return Task.CompletedTask;
    }
}
