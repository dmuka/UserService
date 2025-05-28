using Core;
using Domain.ValueObjects.Emails;

namespace Domain.Users.Events.Integration;

public sealed record UserRegisteredIntegrationEvent(UserId UserId, Email Email, DateTime RegisteredAt) : IIntegrationEvent;