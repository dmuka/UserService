using Application.Users.GetById;
using Core;
using Domain;
using Domain.Users;
using Domain.Users.DomainEvents;

namespace Application.Users.Events;

/// <summary>
/// Handles the UserEmailChangedEvent.
/// </summary>
public class UserEmailChangedDomainEventHandler() : IEventHandler<UserEmailChangedEvent>
{
    public async Task HandleAsync(UserEmailChangedEvent domainEvent, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}