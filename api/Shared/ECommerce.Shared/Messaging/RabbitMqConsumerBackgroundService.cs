using System.Text;
using ECommerce.Shared.Observability;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ECommerce.Shared.Messaging;

/// <summary>
/// Shared consumer loop for both patterns:
/// - Point-to-point: leave <see cref="BindExchange"/> null — the service consumes directly off
///   its own named queue (default exchange), so only one instance processes each message.
/// - Pub/sub: set <see cref="BindExchange"/> and <see cref="RoutingKey"/> — the service declares
///   its own durable queue and binds it to the topic exchange, so it gets its own copy of every
///   matching message independent of any other subscriber.
/// Tolerant of the broker being unreachable: retries the connect/consume loop instead of crashing.
///
/// Delivery is deduplicated via <see cref="IInboxStore"/> (at-least-once broker delivery must not
/// mean at-least-once side effects) and failures are retried a bounded number of times with
/// backoff before landing in "{QueueName}.dlq" for manual inspection — see <see cref="RetryPolicy"/>.
/// </summary>
public abstract class RabbitMqConsumerBackgroundService(
    IRabbitMqConnectionProvider connections,
    IServiceScopeFactory scopeFactory,
    ICorrelationIdAccessor correlation,
    ILogger logger) : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

    protected abstract string QueueName { get; }
    protected virtual string? BindExchange => null;
    protected virtual string? RoutingKey => null;

    /// <summary>
    /// Key used to resolve this consumer's <see cref="IInboxStore"/> via keyed DI when more than one
    /// module (each with its own inbox table) shares the same process — e.g. the API host. Leave
    /// null when the process only ever registers one <see cref="IInboxStore"/> (e.g. a standalone worker).
    /// </summary>
    protected virtual string? InboxKey => null;

    protected abstract Task HandleAsync(string type, string payload, CancellationToken ct);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await RunAsync(stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Expected during host shutdown — not a fault.
        }
    }

    private string DlqName => $"{QueueName}.dlq";

    private async Task RunAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var channel = await connections.GetChannelAsync(stoppingToken).ConfigureAwait(false);
            if (channel is null)
            {
                await Task.Delay(RetryDelay, stoppingToken).ConfigureAwait(false);
                continue;
            }

            try
            {
                await using (channel.ConfigureAwait(false))
                {
                    await channel.QueueDeclareAsync(QueueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken).ConfigureAwait(false);
                    await channel.QueueDeclareAsync(DlqName, durable: true, exclusive: false, autoDelete: false, cancellationToken: stoppingToken).ConfigureAwait(false);

                    if (BindExchange is not null)
                    {
                        await channel.ExchangeDeclareAsync(BindExchange, ExchangeType.Topic, durable: true, cancellationToken: stoppingToken).ConfigureAwait(false);
                        await channel.QueueBindAsync(QueueName, BindExchange, RoutingKey ?? "#", cancellationToken: stoppingToken).ConfigureAwait(false);
                    }

                    var consumer = new AsyncEventingBasicConsumer(channel);
                    consumer.ReceivedAsync += (_, ea) => OnReceivedAsync(channel, ea, stoppingToken);

                    await channel.BasicConsumeAsync(QueueName, autoAck: false, consumer, cancellationToken: stoppingToken).ConfigureAwait(false);

                    while (!stoppingToken.IsCancellationRequested && channel.IsOpen)
                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken).ConfigureAwait(false);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogWarning(ex, "Consumer loop for queue '{Queue}' failed — retrying in {Delay}s.", QueueName, RetryDelay.TotalSeconds);
                await Task.Delay(RetryDelay, stoppingToken).ConfigureAwait(false);
            }
        }
    }

    private async Task OnReceivedAsync(IChannel channel, BasicDeliverEventArgs ea, CancellationToken stoppingToken)
    {
        var type = ea.BasicProperties.Type ?? "unknown";
        var payload = Encoding.UTF8.GetString(ea.Body.ToArray());
        var messageId = string.IsNullOrWhiteSpace(ea.BasicProperties.MessageId) ? Guid.NewGuid().ToString() : ea.BasicProperties.MessageId;
        var correlationId = ea.BasicProperties.CorrelationId;

        correlation.CorrelationId = correlationId;

        using var _ = logger.BeginScope(new Dictionary<string, object?>
        {
            ["MessageId"] = messageId,
            ["CorrelationId"] = correlationId,
            ["Queue"] = QueueName
        });

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var inbox = InboxKey is null
                ? scope.ServiceProvider.GetRequiredService<IInboxStore>()
                : scope.ServiceProvider.GetRequiredKeyedService<IInboxStore>(InboxKey);

            if (await inbox.IsProcessedAsync(messageId, QueueName, stoppingToken).ConfigureAwait(false))
            {
                logger.LogInformation("Message {MessageId} already processed by '{Queue}' — skipping (duplicate delivery).", messageId, QueueName);
                await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, stoppingToken).ConfigureAwait(false);
                return;
            }

            await HandleAsync(type, payload, stoppingToken).ConfigureAwait(false);
            await inbox.MarkProcessedAsync(messageId, QueueName, stoppingToken).ConfigureAwait(false);
            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, stoppingToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await HandleFailureAsync(channel, ea, type, payload, messageId, correlationId, ex, stoppingToken).ConfigureAwait(false);
        }
    }

    private async Task HandleFailureAsync(IChannel channel, BasicDeliverEventArgs ea, string type, string payload, string messageId, string? correlationId, Exception ex, CancellationToken stoppingToken)
    {
        var headers = ea.BasicProperties.Headers;
        var attempt = RetryPolicy.GetAttempt(headers) + 1;
        var routeToDlq = attempt > RetryPolicy.MaxAttempts;

        string destination;
        Dictionary<string, object?> nextHeaders;

        if (routeToDlq)
        {
            logger.LogError(ex, "Message {MessageId} on '{Queue}' failed after {Attempts} attempts — routing to {Dlq}.", messageId, QueueName, attempt - 1, DlqName);
            destination = DlqName;
            nextHeaders = new Dictionary<string, object?>(headers ?? new Dictionary<string, object?>())
            {
                ["x-failed-queue"] = QueueName,
                ["x-error"] = ex.Message
            };
        }
        else
        {
            // In-process backoff before republishing — simple and fully under our control, at the cost
            // of blocking this queue's single consumer loop behind the delay (acceptable for this
            // workload; a per-message-TTL delay queue via the RabbitMQ delayed-message-exchange plugin
            // would avoid the head-of-line blocking at the cost of an extra broker dependency).
            var delay = RetryPolicy.BackoffFor(attempt);
            logger.LogWarning(ex, "Message {MessageId} on '{Queue}' failed (attempt {Attempt}/{MaxAttempts}) — retrying in {DelaySeconds}s.", messageId, QueueName, attempt, RetryPolicy.MaxAttempts, delay.TotalSeconds);
            await Task.Delay(delay, stoppingToken).ConfigureAwait(false);

            destination = QueueName;
            nextHeaders = RetryPolicy.WithIncrementedAttempt(headers);
        }

        var props = new BasicProperties { Persistent = true, Type = type, MessageId = messageId, CorrelationId = correlationId, Headers = nextHeaders };

        try
        {
            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: destination, mandatory: false, basicProperties: props, body: Encoding.UTF8.GetBytes(payload), cancellationToken: stoppingToken).ConfigureAwait(false);

            // Only remove the original delivery once its retry/DLQ replacement is safely published —
            // acking first and publishing second would silently drop the message if the publish failed.
            await channel.BasicAckAsync(ea.DeliveryTag, multiple: false, stoppingToken).ConfigureAwait(false);
        }
        catch (Exception publishEx)
        {
            // Couldn't hand off the replacement — leave the original unacked so the broker redelivers
            // it once the connection recovers, instead of losing it between two queues.
            logger.LogError(publishEx, "Failed to publish retry/DLQ copy for message {MessageId} on '{Queue}' — leaving original unacked for redelivery.", messageId, QueueName);
        }
    }
}
