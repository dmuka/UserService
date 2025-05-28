using Core;

namespace Domain.Users.Events.Domain;

/// <summary>
/// Event raised when a user's email is changed.
/// </summary>
public class UserEmailChangedDomainEvent(Guid userId, string newEmail) : IDomainEvent
{
    public Guid UserId { get; } = userId;
    public string NewEmail { get; } = newEmail;
}