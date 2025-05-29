using Core;
using Domain;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Events;

public class EventDispatcher(IServiceProvider serviceProvider) : IEventDispatcher
{
    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var eventType = domainEvent.GetType();
        var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
        var handlers = serviceProvider.GetServices(handlerType);

        foreach (var handler in handlers)
        {
            var handleMethod = handlerType.GetMethod("HandleAsync");
            var task = handleMethod?.Invoke(handler, [domainEvent, cancellationToken]);
            if (task != null) await (Task)task;
        }
    }
}