using Domain;
using Domain.Users;
using Domain.Users.DomainEvents;

namespace Application.Users.Events;

public class UserRegisteredDomainEventHandler
{
    /// <summary>
    /// Handles the UserRegisteredDomainEvent.
    /// </summary>
    public class UserEmailChangedDomainEventHandler() : IEventHandler<UserEmailChangedEvent>
    {
        public async Task HandleAsync(UserEmailChangedEvent domainEvent, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}