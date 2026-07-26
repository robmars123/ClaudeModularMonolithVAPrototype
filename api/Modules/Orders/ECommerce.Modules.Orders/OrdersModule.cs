using ECommerce.Modules.Orders.Features.PlaceOrder;
using ECommerce.Modules.Orders.Persistence;
using ECommerce.Shared.Contracts;
using ECommerce.Shared.Mediator;
using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Modules.Orders;

public class OrdersModule : IModuleRegistration
{
    public IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OrdersDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddMediator(typeof(OrdersModule).Assembly);
        services.AddSingleton<PlaceOrderIdempotencyGuard>();

        services.AddScoped<IIntegrationEventPublisher, EfOutboxIntegrationEventPublisher<OrdersDbContext>>();
        services.AddScoped<IOutboxStore, EfOutboxStore<OrdersDbContext>>();

        return services;
    }
}
