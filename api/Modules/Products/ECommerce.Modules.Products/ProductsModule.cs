using ECommerce.Modules.Products.Persistence;
using ECommerce.Shared.Contracts;
using ECommerce.Shared.Mediator;
using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Modules.Products;

public class ProductsModule : IModuleRegistration
{
    /// <summary>Keyed-DI key for this module's <see cref="IIntegrationEventPublisher"/> — see <see cref="ECommerce.Modules.Users.UsersModule.PublisherKey"/> for why this must be keyed.</summary>
    public const string PublisherKey = "products";

    public IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ProductsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddMediator(typeof(ProductsModule).Assembly);

        services.AddKeyedScoped<IIntegrationEventPublisher, EfOutboxIntegrationEventPublisher<ProductsDbContext>>(PublisherKey);
        services.AddScoped<IOutboxStore, EfOutboxStore<ProductsDbContext>>();

        return services;
    }
}
