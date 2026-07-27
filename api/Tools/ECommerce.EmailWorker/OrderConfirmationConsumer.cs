using System.Text.Json;
using ECommerce.Shared.IntegrationEvents;
using ECommerce.Shared.Messaging;
using ECommerce.Shared.Observability;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.EmailWorker;

/// <summary>
/// Own pub/sub subscriber for order confirmations — independent of Inventory's point-to-point
/// OrderPlacedQueueConsumer, which commits stock from the same underlying event on a different queue.
/// Mocks sending a confirmation email (console output only), matching EmailNotificationConsumer.
/// </summary>
public sealed class OrderConfirmationConsumer(IRabbitMqConnectionProvider connections, IServiceScopeFactory scopeFactory, ICorrelationIdAccessor correlation, ILogger<OrderConfirmationConsumer> logger)
    : RabbitMqConsumerBackgroundService(connections, scopeFactory, correlation, logger)
{
    protected override string QueueName => "email-worker.order-placed.queue";
    protected override string BindExchange => "ecommerce.events";
    protected override string RoutingKey => "order.placed";

    protected override Task HandleAsync(string type, string payload, CancellationToken ct)
    {
        OrderPlacedEvent? evt = JsonSerializer.Deserialize<OrderPlacedEvent>(payload);
        if (evt is null)
        {
            logger.LogWarning("Received {Type} but could not deserialize payload: {Payload}", type, payload);
            return Task.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(evt.CustomerEmail))
        {
            logger.LogWarning("Order {OrderNumber} placed but no customer contact was resolved; skipping confirmation email.", evt.OrderNumber);
            return Task.CompletedTask;
        }

        logger.LogInformation(
            "[EmailWorker] Sending order confirmation for {OrderNumber} to {Email} ({CustomerName})...",
            evt.OrderNumber, evt.CustomerEmail, evt.CustomerName);
        logger.LogInformation("[EmailWorker] Email sent (mock) — no real SMTP call made.");

        return Task.CompletedTask;
    }
}
