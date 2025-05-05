using Core;

namespace Domain.Roles.DomainEvents;

public sealed record RoleCreatedDomainEvent(Guid RoleId) : IDomainEvent;