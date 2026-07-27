using ECommerce.Modules.Users.Entities;
using ECommerce.Modules.Users.Features.RegisterUser;
using ECommerce.Modules.Users.Persistence;
using ECommerce.Modules.Users.Services;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using ECommerce.Shared.Messaging;
using ECommerce.Shared.Observability;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace ECommerce.Modules.Users.Tests;

public sealed class RegisterUserHandlerTests
{
    private static RegisterUserHandler MakeHandler(IServiceProvider services, out IPublisher publisher)
    {
        UsersDbContext db = services.GetRequiredService<UsersDbContext>();
        publisher = Substitute.For<IPublisher>();

        return new RegisterUserHandler(
            services.GetRequiredService<UserManager<ApplicationUser>>(),
            db,
            services.GetRequiredService<AuthTokenIssuer>(),
            publisher,
            new EfOutboxIntegrationEventPublisher<UsersDbContext>(db, Substitute.For<ICorrelationIdAccessor>()),
            Substitute.For<ILogger<RegisterUserHandler>>());
    }

    [Fact]
    public async Task Registers_as_customer_and_issues_tokens()
    {
        using UsersTestHost host = new UsersTestHost();
        using IServiceScope scope = host.NewScope();
        RegisterUserHandler handler = MakeHandler(scope.ServiceProvider, out IPublisher? publisher);

        Result<AuthResponse> result = await handler.Handle(
            new RegisterUserCommand("Jane", "Doe", "jane@example.com", "Password1"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value!.Token);
        Assert.NotEmpty(result.Value!.RefreshToken);
        Assert.Equal(["Customer"], result.Value!.Roles);

        UsersDbContext db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        Assert.Single(await db.OutboxMessages.ToListAsync());
        Assert.Single(await db.RefreshTokens.ToListAsync());

        await publisher.Received(1).Publish(Arg.Any<ECommerce.Shared.IntegrationEvents.UserRegisteredEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Rejects_duplicate_email()
    {
        using UsersTestHost host = new UsersTestHost();

        using (IServiceScope scope = host.NewScope())
        {
            RegisterUserHandler handler = MakeHandler(scope.ServiceProvider, out _);
            Result<AuthResponse> first = await handler.Handle(new RegisterUserCommand("Jane", "Doe", "jane@example.com", "Password1"), CancellationToken.None);
            Assert.True(first.IsSuccess);
        }

        using IServiceScope secondScope = host.NewScope();
        RegisterUserHandler secondHandler = MakeHandler(secondScope.ServiceProvider, out _);
        Result<AuthResponse> result = await secondHandler.Handle(new RegisterUserCommand("Jane", "Other", "jane@example.com", "Password2"), CancellationToken.None);

        Assert.False(result.IsSuccess);

        UsersDbContext db = secondScope.ServiceProvider.GetRequiredService<UsersDbContext>();
        Assert.Single(await db.Users.ToListAsync());
    }
}
