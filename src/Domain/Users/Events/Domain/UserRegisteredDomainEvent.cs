using Core;
using Domain.ValueObjects.Emails;

namespace Domain.Users.Events.Domain;

public sealed record UserRegisteredDomainEvent(UserId UserId, Email Email, DateTime RegisteredAt) : IDomainEvent;