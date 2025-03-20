using Application.Users.GetById;
using Core;
using Domain.Users;
using Domain.Users.DomainEvents;

namespace Application.Users.Events;

/// <summary>
/// Handles the UserEmailChangedEvent.
/// </summary>
public class UserEmailChangedDomainEventHandler(CancellationToken cancellationToken)
{
    public async Task Handle(UserEmailChangedEvent domainEvent)
    {
        throw new NotImplementedException();
    }
}