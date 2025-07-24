using Core;

namespace Domain.Users.Events.Integration;

public sealed record UserRegisteredIntegrationEvent(Guid UserId, string FirstName, string LastName, string Email, DateTime RegisteredAt) : IIntegrationEvent;