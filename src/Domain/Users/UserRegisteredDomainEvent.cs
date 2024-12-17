namespace Domain;

public sealed record UserRegisteredDomainEvent(Guid UserId) : IDomainEvent;
