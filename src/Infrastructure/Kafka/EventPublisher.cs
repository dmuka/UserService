using System.Text.Json;
using Application.Abstractions.Kafka;
using Confluent.Kafka;
using Core;
using Microsoft.Extensions.Configuration;
using Error = Confluent.Kafka.Error;

namespace Infrastructure.Kafka;

public class EventPublisher(IConfiguration configuration) : IEventPublisher
{
    private readonly IProducer<Null, string> _producer = new ProducerBuilder<Null, string>(new ProducerConfig { BootstrapServers = configuration["Kafka:BootstrapServers"] }).Build();
    private readonly string _topicPrefix = configuration["Kafka:TopicPrefix"] ?? throw new ProduceException<Null, string>(new Error(ErrorCode.TopicException), new DeliveryResult<Null, string>());

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