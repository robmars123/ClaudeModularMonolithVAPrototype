using ECommerce.Modules.Users.Entities;
using ECommerce.Modules.Users.Persistence;
using ECommerce.Modules.Users.Services;
using ECommerce.Shared.Contracts;
using ECommerce.Shared.Mediator;
using ECommerce.Shared.Messaging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Modules.Users;

public class UsersModule : IModuleRegistration
{
    public IServiceCollection RegisterModule(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<UsersDbContext>(o =>
            o.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddIdentity<ApplicationUser, IdentityRole>(o =>
        {
            o.Password.RequiredLength = 8;
            o.Password.RequireDigit = true;
            o.Password.RequireUppercase = true;
            o.Password.RequireNonAlphanumeric = false;
            o.User.RequireUniqueEmail = true;
            o.Lockout.MaxFailedAccessAttempts = 5;
            o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
        .AddEntityFrameworkStores<UsersDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<TokenService>();
        services.AddScoped<AuthTokenIssuer>();
        services.AddMediator(typeof(UsersModule).Assembly);

        services.AddScoped<IIntegrationEventPublisher, EfOutboxIntegrationEventPublisher<UsersDbContext>>();
        services.AddScoped<IOutboxStore, EfOutboxStore<UsersDbContext>>();

        return services;
    }
}
