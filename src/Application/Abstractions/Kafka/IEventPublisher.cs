using Core;

namespace Application.Abstractions.Kafka;

public interface IEventPublisher
{
    Task PublishAsync<T>(string topic, T @event, CancellationToken cancellationToken = default)  where T : IIntegrationEvent;
}