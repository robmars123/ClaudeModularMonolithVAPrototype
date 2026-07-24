namespace ECommerce.Shared.Messaging;

/// <summary>Consumer-side dedup so RabbitMQ's at-least-once delivery never processes the same message twice.</summary>
public interface IInboxStore
{
    Task<bool> IsProcessedAsync(string messageId, string consumer, CancellationToken ct = default);

    Task MarkProcessedAsync(string messageId, string consumer, CancellationToken ct = default);
}
