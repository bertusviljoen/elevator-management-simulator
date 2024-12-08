namespace Domain.Common;

/// <summary> Base abstract class for entities which have domain events. </summary>
public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public List<IDomainEvent> DomainEvents => [.. _domainEvents];

    /// <summary> Clear all domain events to ensure that they are not dispatched more than once. </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary> Add a domain event to the entity to be dispatched. Raise actually adds the event to the list. </summary>
    public void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
