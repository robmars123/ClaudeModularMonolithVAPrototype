using RabbitMQ.Client;

namespace ECommerce.Shared.Messaging;

/// <summary>
/// Lazily-connected, tolerant of the broker being unreachable: callers get a null channel instead
/// of an exception so a down RabbitMQ never crashes the API — messages simply stay in the outbox.
/// </summary>
public interface IRabbitMqConnectionProvider
{
    Task<IChannel?> GetChannelAsync(CancellationToken ct = default);
}
