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
    public IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ProductsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddMediator(typeof(ProductsModule).Assembly);

        services.AddScoped<IIntegrationEventPublisher, EfOutboxIntegrationEventPublisher<ProductsDbContext>>();
        services.AddScoped<IOutboxStore, EfOutboxStore<ProductsDbContext>>();

        return services;
    }
}
