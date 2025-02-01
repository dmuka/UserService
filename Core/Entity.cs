namespace Core;

public abstract class Entity
{
    /// <summary>
    /// Holds the requested hash code for this entity if it has been computed.
    /// </summary>
    private int? _requestedHashCode;
    
    /// <summary>
    /// Prime number for better hash distribution
    /// </summary>
    private const int HashSeed = 31;
    
    /// <summary>
    /// Gets the unique identifier for this entity.
    /// </summary>
    public long Id { get; protected set; }

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

    /// <summary>
    /// Checks if the entity is transient (i.e., it does not have an assigned ID).
    /// </summary>
    /// <returns>True if the entity is transient, otherwise false.</returns>
    private bool IsTransient() => Id.Equals(0);

    /// <summary>
    /// Determines whether the specified object is equal to the current entity.
    /// </summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns>True if the specified object is equal to the current entity, otherwise false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not Entity entity) return false;

        if (ReferenceEquals(this, entity)) return true;

        if (GetType() != entity.GetType()) return false;

        if (entity.IsTransient() || IsTransient()) return false;

        return entity.Id.Equals(Id);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current entity.</returns>
    public override int GetHashCode()
    {
        if (_requestedHashCode.HasValue)
        {
            return _requestedHashCode.Value;
        }

        if (!IsTransient())
        {
            _requestedHashCode = HashCode.Combine(Id, HashSeed);
            return _requestedHashCode.Value;
        }

        return base.GetHashCode();
    }

    /// <summary>
    /// Equality operator to compare two entities.
    /// </summary>
    /// <param name="left">The first entity to compare.</param>
    /// <param name="right">The second entity to compare.</param>
    /// <returns>True if the entities are equal, otherwise false.</returns>
    public static bool operator ==(Entity left, Entity right)
    {
        return left?.Equals(right) ?? Equals(right, null);
    }

    /// <summary>
    /// Inequality operator to compare two entities.
    /// </summary>
    /// <param name="left">The first entity to compare.</param>
    /// <param name="right">The second entity to compare.</param>
    /// <returns>True if the entities are not equal, otherwise false.</returns>
    public static bool operator !=(Entity left, Entity right)
    {
        return !(left == right);
    }
}