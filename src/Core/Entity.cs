namespace Core;

public abstract class Entity
{
    public TypedId Id { get; set; }
    
    /// <summary>
    /// A private list to hold domain events associated with this entity.
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Gets a read-only collection of domain events associated with this entity.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the list of domain events.
    /// </summary>
    /// <param name="eventItem">The domain event to add.</param>
    public void AddDomainEvent(IDomainEvent eventItem) => _domainEvents.Add(eventItem);

    /// <summary>
    /// Removes a domain event from the list of domain events.
    /// </summary>
    /// <param name="eventItem">The domain event to remove.</param>
    public void RemoveDomainEvent(IDomainEvent eventItem) => _domainEvents.Remove(eventItem);

    /// <summary>
    /// Clears all domain events from the list.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}