using System.Text.Json;
using Application.Abstractions.Kafka;
using Confluent.Kafka;
using Core;
using Infrastructure.Options.Kafka;
using Microsoft.Extensions.Options;

namespace Infrastructure.Kafka;

public class EventPublisher(IOptions<ProduceOptions> options) : IEventPublisher
{
    private readonly IProducer<Null, string> _producer = new ProducerBuilder<Null, string>(
        new ProducerConfig { BootstrapServers = options.Value.BootstrapServers })
        .Build();
    private readonly string _topicPrefix = options.Value.TopicPrefix;

    public async Task PublishAsync<T>(string topic, T @event, CancellationToken cancellationToken = default) where T : IIntegrationEvent
    {
        var message = JsonSerializer.Serialize(@event);
        
        await _producer.ProduceAsync($"{_topicPrefix}{topic}", new Message<Null, string> { Value = message }, cancellationToken);
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}