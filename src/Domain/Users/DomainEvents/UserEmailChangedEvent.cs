using Core;

namespace Domain.Users.DomainEvents;

/// <summary>
/// Event raised when a user's email is changed.
/// </summary>
public class UserEmailChangedEvent : IDomainEvent
{
    public Guid UserId { get; }
    public string NewEmail { get; }

    public UserEmailChangedEvent(Guid userId, string newEmail)
    {
        UserId = userId;
        NewEmail = newEmail;
    }
}