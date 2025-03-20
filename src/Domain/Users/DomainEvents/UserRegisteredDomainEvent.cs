using Core;

namespace Domain.Users.DomainEvents;

public sealed record UserRegisteredDomainEvent(Guid UserId) : IDomainEvent;