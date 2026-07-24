namespace ECommerce.Shared.Messaging;

/// <summary>Low-level broker send — <see cref="OutboxDispatcher"/> is the only intended caller.</summary>
public interface IRabbitMqPublisher
{
    /// <summary>Point-to-point: default exchange, routing key = queue name. Exactly one consumer processes the message.</summary>
    Task PublishToQueueAsync(string queue, string type, string payload, string messageId, string? correlationId, CancellationToken ct = default);

    /// <summary>Pub/sub: topic exchange. Every queue bound with a matching routing key gets its own copy.</summary>
    Task PublishToTopicAsync(string exchange, string routingKey, string type, string payload, string messageId, string? correlationId, CancellationToken ct = default);
}
