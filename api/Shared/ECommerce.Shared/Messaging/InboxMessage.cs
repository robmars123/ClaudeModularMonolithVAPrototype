namespace ECommerce.Shared.Messaging;

/// <summary>
/// Records that a specific broker message has already been handled by a specific consumer, so
/// RabbitMQ's at-least-once delivery (redelivery after a crash, a nack, a requeued retry) never
/// causes the same message to be processed twice.
/// </summary>
public sealed class InboxMessage
{
    /// <summary>The publisher-assigned message id (see <see cref="OutboxMessage.Id"/>), not a new id of our own.</summary>
    public required string MessageId { get; init; }

    /// <summary>Consumer name (e.g. the queue), since the same message id could be consumed by more than one subscriber.</summary>
    public required string Consumer { get; init; }

    public DateTime ProcessedAtUtc { get; init; } = DateTime.UtcNow;
}
