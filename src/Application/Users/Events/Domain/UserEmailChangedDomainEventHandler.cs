using Domain;
using Domain.Users.Events.Domain;

namespace Application.Users.Events.Domain;

/// <summary>
/// Handles the UserEmailChangedEvent.
/// </summary>
public class UserEmailChangedDomainEventHandler() : IEventHandler<UserEmailChangedDomainEvent>
{
    public async Task HandleAsync(UserEmailChangedDomainEvent domainDomainEvent, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}