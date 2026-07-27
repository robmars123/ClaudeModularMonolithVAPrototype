using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ECommerce.Shared.Messaging;

/// <summary>
/// Drains the outbox into RabbitMQ. Runs continuously so it also acts as a retry loop — a message
/// that failed because the broker was briefly down simply gets picked up again on the next tick.
/// </summary>
public sealed class OutboxDispatcher(IServiceScopeFactory scopeFactory, IOptions<RabbitMqOptions> options, ILogger<OutboxDispatcher> logger) : BackgroundService
{
    private readonly RabbitMqOptions _options = options.Value;

    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(3);
    private const int BatchSize = 50;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new PeriodicTimer(PollInterval);

        try
        {
            do
            {
                try
                {
                    await DispatchPendingAsync(stoppingToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Outbox dispatch tick failed.");
                }
            } while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false));
        }
        catch (OperationCanceledException)
        {
            // Expected during host shutdown — the timer wait was cancelled, not a fault.
        }
    }

    private async Task DispatchPendingAsync(CancellationToken ct)
    {
        // Every module registers its own IOutboxStore (backed by its own DbContext/schema) — drain them all each tick.
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        IEnumerable<IOutboxStore> outboxStores = scope.ServiceProvider.GetServices<IOutboxStore>();
        IRabbitMqPublisher publisher = scope.ServiceProvider.GetRequiredService<IRabbitMqPublisher>();

        foreach (IOutboxStore outbox in outboxStores)
            await DispatchFromStoreAsync(outbox, publisher, ct).ConfigureAwait(false);
    }

    private async Task DispatchFromStoreAsync(IOutboxStore outbox, IRabbitMqPublisher publisher, CancellationToken ct)
    {
        IReadOnlyList<OutboxMessage> pending = await outbox.GetUnprocessedAsync(BatchSize, ct).ConfigureAwait(false);

        foreach (OutboxMessage message in pending)
        {
            try
            {
                string messageId = message.Id.ToString();

                if (message.Kind == MessageKind.PointToPoint)
                    await publisher.PublishToQueueAsync(message.Destination, message.Type, message.Payload, messageId, message.CorrelationId, ct).ConfigureAwait(false);
                else
                    await publisher.PublishToTopicAsync(_options.PubSubExchange, message.Destination, message.Type, message.Payload, messageId, message.CorrelationId, ct).ConfigureAwait(false);

                await outbox.MarkProcessedAsync(message.Id, ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Also catches a broker publish that actually succeeded but whose MarkProcessedAsync
                // then failed (e.g. a DB blip) — that message gets marked failed and republished next
                // tick, i.e. delivered twice. That's an accepted trade-off: the inbox pattern on the
                // consumer side (see IInboxStore) already dedupes at-least-once delivery, so a rare
                // duplicate send is safe, whereas losing the message would not be.
                logger.LogWarning(ex, "Failed to dispatch outbox message {MessageId} ({Type}) — will retry.", message.Id, message.Type);
                await outbox.MarkFailedAsync(message.Id, ex.Message, ct).ConfigureAwait(false);
            }
        }
    }
}
