using System.Text.Json;
using ECommerce.Shared.IntegrationEvents;
using ECommerce.Shared.Messaging;
using ECommerce.Shared.Observability;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.EmailWorker;

/// <summary>
/// Standalone out-of-process pub/sub subscriber — its own queue bound to the shared
/// "ecommerce.events" topic exchange, so it gets its own copy of every UserRegisteredEvent
/// independent of the in-process Customers handler and the ProductCreated/OrderPlaced consumers
/// living inside the API process. Mocks sending a welcome email (console output only).
/// </summary>
public sealed class EmailNotificationConsumer(IRabbitMqConnectionProvider connections, IServiceScopeFactory scopeFactory, ICorrelationIdAccessor correlation, ILogger<EmailNotificationConsumer> logger)
    : RabbitMqConsumerBackgroundService(connections, scopeFactory, correlation, logger)
{
    // Only one IInboxStore is registered in this process, so the default (unkeyed) resolution is fine.
    protected override string QueueName => "email-worker.user-registered.queue";
    protected override string BindExchange => "ecommerce.events";
    protected override string RoutingKey => "user.registered";

    protected override Task HandleAsync(string type, string payload, CancellationToken ct)
    {
        UserRegisteredEvent? evt = JsonSerializer.Deserialize<UserRegisteredEvent>(payload);
        if (evt is null)
        {
            logger.LogWarning("Received {Type} but could not deserialize payload: {Payload}", type, payload);
            return Task.CompletedTask;
        }

        logger.LogInformation(
            "[EmailWorker] Sending welcome email to {Email} ({FirstName} {LastName})...",
            evt.Email, evt.FirstName, evt.LastName);
        logger.LogInformation("[EmailWorker] Email sent (mock) — no real SMTP call made.");

        return Task.CompletedTask;
    }
}
