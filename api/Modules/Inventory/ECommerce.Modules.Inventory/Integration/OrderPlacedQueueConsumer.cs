using System.Text.Json;
using ECommerce.Shared.IntegrationEvents;
using ECommerce.Shared.Messaging;
using ECommerce.Shared.Observability;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Inventory.Integration;

/// <summary>
/// Point-to-point consumer for "orders.placed.queue" (default exchange, one named queue —
/// competing consumers, so scaling this service out still processes each order exactly once).
/// The authoritative stock commit already happens synchronously in-process via
/// <see cref="OrderPlacedHandler"/>; this is the async broker path other out-of-process
/// consumers (reporting, notifications, a future microservice) would subscribe to instead.
/// </summary>
public sealed class OrderPlacedQueueConsumer(IRabbitMqConnectionProvider connections, IServiceScopeFactory scopeFactory, ICorrelationIdAccessor correlation, ILogger<OrderPlacedQueueConsumer> logger)
    : RabbitMqConsumerBackgroundService(connections, scopeFactory, correlation, logger)
{
    protected override string QueueName => "orders.placed.queue";
    protected override string InboxKey => InventoryModule.InboxKey;

    protected override Task HandleAsync(string type, string payload, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<OrderPlacedEvent>(payload);
        logger.LogInformation(
            "[RabbitMQ point-to-point] Received {Type} for order {OrderNumber} ({LineCount} line(s)).",
            type, evt?.OrderNumber, evt?.Lines.Count ?? 0);

        return Task.CompletedTask;
    }
}
