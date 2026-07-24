using ECommerce.Modules.Customers.Integration;
using ECommerce.Modules.Customers.Persistence;
using ECommerce.Shared.Contracts;
using ECommerce.Shared.Mediator;
using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Modules.Customers;

public class CustomersModule : IModuleRegistration
{
    /// <summary>Keyed-DI key for this module's <see cref="IInboxStore"/> — shared with its consumer so the key can't drift out of sync.</summary>
    public const string InboxKey = "customers";

    public IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CustomersDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddMediator(typeof(CustomersModule).Assembly);

        services.AddKeyedScoped<IInboxStore, EfInboxStore<CustomersDbContext>>(InboxKey);

        // RabbitMQ pub/sub subscriber for UserRegisteredEvent (see AddRabbitMqMessaging in Program.cs for the broker wiring).
        services.AddHostedService<UserRegisteredTopicConsumer>();

        return services;
    }
}
