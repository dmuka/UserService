using Core;
using Domain.ValueObjects.Emails;

namespace Domain.Users.Events.Domain;

public sealed record UserRegisteredDomainEvent(
    Guid UserId, 
    string FirstName, 
    string LastName, 
    string Email, 
    DateTime RegisteredAt) : IDomainEvent;