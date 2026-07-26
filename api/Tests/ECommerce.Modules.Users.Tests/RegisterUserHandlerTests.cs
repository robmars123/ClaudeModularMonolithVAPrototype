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
        var db = services.GetRequiredService<UsersDbContext>();
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
        using var host = new UsersTestHost();
        using var scope = host.NewScope();
        var handler = MakeHandler(scope.ServiceProvider, out var publisher);

        var result = await handler.Handle(
            new RegisterUserCommand("Jane", "Doe", "jane@example.com", "Password1"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value!.Token);
        Assert.NotEmpty(result.Value!.RefreshToken);
        Assert.Equal(["Customer"], result.Value!.Roles);

        var db = scope.ServiceProvider.GetRequiredService<UsersDbContext>();
        Assert.Single(await db.OutboxMessages.ToListAsync());
        Assert.Single(await db.RefreshTokens.ToListAsync());

        await publisher.Received(1).Publish(Arg.Any<ECommerce.Shared.IntegrationEvents.UserRegisteredEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Rejects_duplicate_email()
    {
        using var host = new UsersTestHost();

        using (var scope = host.NewScope())
        {
            var handler = MakeHandler(scope.ServiceProvider, out _);
            var first = await handler.Handle(new RegisterUserCommand("Jane", "Doe", "jane@example.com", "Password1"), CancellationToken.None);
            Assert.True(first.IsSuccess);
        }

        using var secondScope = host.NewScope();
        var secondHandler = MakeHandler(secondScope.ServiceProvider, out _);
        var result = await secondHandler.Handle(new RegisterUserCommand("Jane", "Other", "jane@example.com", "Password2"), CancellationToken.None);

        Assert.False(result.IsSuccess);

        var db = secondScope.ServiceProvider.GetRequiredService<UsersDbContext>();
        Assert.Single(await db.Users.ToListAsync());
    }
}
