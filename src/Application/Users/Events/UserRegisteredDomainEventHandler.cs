using Domain.Users;

namespace Application.Users.Events;

public class UserRegisteredDomainEventHandler
{
    /// <summary>
    /// Handles the UserRegisteredDomainEvent.
    /// </summary>
    public class UserEmailChangedDomainEventHandler(CancellationToken cancellationToken)
    {
        public async Task Handle(UserRegisteredDomainEvent domainEvent)
        {
            throw new NotImplementedException();
        }
    }
}