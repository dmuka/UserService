using Core;
using Domain.ValueObjects.Emails;

namespace Domain.Users.Events.Domain;

public sealed record UserRegisteredDomainEvent(
    UserId UserId, 
    string FirstName, 
    string LastName, 
    Email Email, 
    DateTime RegisteredAt) : IDomainEvent;