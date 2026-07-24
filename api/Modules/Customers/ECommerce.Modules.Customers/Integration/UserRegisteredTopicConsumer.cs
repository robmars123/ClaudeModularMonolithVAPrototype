using System.Text.Json;
using ECommerce.Shared.IntegrationEvents;
using ECommerce.Shared.Messaging;
using ECommerce.Shared.Observability;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Customers.Integration;

/// <summary>
/// Pub/sub consumer bound to the "ecommerce.events" topic exchange with routing key
/// "user.registered" — its own durable queue, independent of any other subscriber to the same
/// event. The authoritative customer-profile creation already happens synchronously in-process
/// via <see cref="UserRegisteredHandler"/>; this demonstrates the broadcast broker path.
/// </summary>
public sealed class UserRegisteredTopicConsumer(IRabbitMqConnectionProvider connections, IServiceScopeFactory scopeFactory, ICorrelationIdAccessor correlation, ILogger<UserRegisteredTopicConsumer> logger)
    : RabbitMqConsumerBackgroundService(connections, scopeFactory, correlation, logger)
{
    protected override string QueueName => "customers.user-registered.queue";
    protected override string BindExchange => "ecommerce.events";
    protected override string RoutingKey => "user.registered";
    protected override string InboxKey => CustomersModule.InboxKey;

    protected override Task HandleAsync(string type, string payload, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<UserRegisteredEvent>(payload);
        logger.LogInformation(
            "[RabbitMQ pub/sub] Customers received {Type} for {Email}.",
            type, evt?.Email);

        return Task.CompletedTask;
    }
}
