using System.Text;
using RabbitMQ.Client;

namespace ECommerce.Shared.Messaging;

public sealed class RabbitMqPublisher(IRabbitMqConnectionProvider connections) : IRabbitMqPublisher
{
    public async Task PublishToQueueAsync(string queue, string type, string payload, string messageId, string? correlationId, CancellationToken ct = default)
    {
        IChannel channel = await connections.GetChannelAsync(ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("RabbitMQ is unreachable.");

        await using (channel.ConfigureAwait(false))
        {
            await channel.QueueDeclareAsync(queue, durable: true, exclusive: false, autoDelete: false, cancellationToken: ct).ConfigureAwait(false);

            BasicProperties props = new BasicProperties { Persistent = true, Type = type, MessageId = messageId, CorrelationId = correlationId };
            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: queue,
                mandatory: false,
                basicProperties: props,
                body: Encoding.UTF8.GetBytes(payload),
                cancellationToken: ct).ConfigureAwait(false);
        }
    }

    public async Task PublishToTopicAsync(string exchange, string routingKey, string type, string payload, string messageId, string? correlationId, CancellationToken ct = default)
    {
        IChannel channel = await connections.GetChannelAsync(ct).ConfigureAwait(false)
            ?? throw new InvalidOperationException("RabbitMQ is unreachable.");

        await using (channel.ConfigureAwait(false))
        {
            await channel.ExchangeDeclareAsync(exchange, ExchangeType.Topic, durable: true, cancellationToken: ct).ConfigureAwait(false);

            BasicProperties props = new BasicProperties { Persistent = true, Type = type, MessageId = messageId, CorrelationId = correlationId };
            await channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: props,
                body: Encoding.UTF8.GetBytes(payload),
                cancellationToken: ct).ConfigureAwait(false);
        }
    }
}
