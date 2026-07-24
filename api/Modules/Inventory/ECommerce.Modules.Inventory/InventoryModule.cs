using ECommerce.Modules.Inventory.Integration;
using ECommerce.Modules.Inventory.Persistence;
using ECommerce.Shared.Contracts;
using ECommerce.Shared.Mediator;
using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Modules.Inventory;

public class InventoryModule : IModuleRegistration
{
    /// <summary>Keyed-DI key for this module's <see cref="IInboxStore"/> — shared with its consumers so the key can't drift out of sync.</summary>
    public const string InboxKey = "inventory";

    public IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InventoryDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddMediator(typeof(InventoryModule).Assembly);

        services.AddKeyedScoped<IInboxStore, EfInboxStore<InventoryDbContext>>(InboxKey);

        // RabbitMQ consumers (see AddRabbitMqMessaging in Program.cs for the broker wiring):
        // point-to-point for OrderPlacedEvent, pub/sub for ProductCreatedEvent.
        services.AddHostedService<OrderPlacedQueueConsumer>();
        services.AddHostedService<ProductCreatedTopicConsumer>();

        return services;
    }
}
