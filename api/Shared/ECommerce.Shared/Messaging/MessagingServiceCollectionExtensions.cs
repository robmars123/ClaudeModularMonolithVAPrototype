using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ECommerce.Shared.Messaging;

public static class MessagingServiceCollectionExtensions
{
    /// <summary>
    /// Registers the RabbitMQ connection/publisher and the outbox dispatcher. Does NOT register
    /// <see cref="IIntegrationEventPublisher"/> or <see cref="IOutboxStore"/> — each module registers
    /// its own, generic over its own <c>DbContext</c>, so the outbox write is atomic with that
    /// module's business save (see <see cref="EfOutboxIntegrationEventPublisher{TDbContext}"/>).
    /// </summary>
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection(RabbitMqOptions.SectionName));

        services.TryAddSingleton<IRabbitMqConnectionProvider, RabbitMqConnectionProvider>();
        services.TryAddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
        services.AddHostedService<OutboxDispatcher>();

        return services;
    }
}
