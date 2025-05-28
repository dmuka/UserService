using Core;
using Domain.ValueObjects.Emails;

namespace Domain.Users.DomainEvents;

public sealed record UserRegisteredDomainEvent(UserId UserId, Email Email, DateTime RegisteredAt) : IDomainEvent;