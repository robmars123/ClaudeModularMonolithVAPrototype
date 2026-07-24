using System.Text.Json;
using ECommerce.Shared.IntegrationEvents;
using ECommerce.Shared.Messaging;
using ECommerce.Shared.Observability;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Inventory.Integration;

/// <summary>
/// Pub/sub consumer bound to the "ecommerce.events" topic exchange with routing key
/// "product.created" — its own durable queue, independent of any other subscriber to the same
/// event. The authoritative inventory-record creation already happens synchronously in-process
/// via <see cref="ProductCreatedHandler"/>; this demonstrates the broadcast broker path.
/// </summary>
public sealed class ProductCreatedTopicConsumer(IRabbitMqConnectionProvider connections, IServiceScopeFactory scopeFactory, ICorrelationIdAccessor correlation, ILogger<ProductCreatedTopicConsumer> logger)
    : RabbitMqConsumerBackgroundService(connections, scopeFactory, correlation, logger)
{
    protected override string QueueName => "inventory.product-created.queue";
    protected override string BindExchange => "ecommerce.events";
    protected override string RoutingKey => "product.created";
    protected override string InboxKey => InventoryModule.InboxKey;

    protected override Task HandleAsync(string type, string payload, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<ProductCreatedEvent>(payload);
        logger.LogInformation(
            "[RabbitMQ pub/sub] Inventory received {Type} for product {Sku} ({Name}).",
            type, evt?.Sku, evt?.Name);

        return Task.CompletedTask;
    }
}
