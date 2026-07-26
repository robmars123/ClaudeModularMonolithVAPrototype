using ECommerce.Modules.Users.Entities;
using ECommerce.Modules.Users.Features.LoginUser;
using ECommerce.Modules.Users.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ECommerce.Modules.Users.Tests;

public sealed class LoginUserHandlerTests
{
    private static async Task SeedUserAsync(IServiceProvider services, string email, string password)
    {
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { FirstName = "Jane", LastName = "Doe", Email = email, UserName = email };
        var result = await users.CreateAsync(user, password);
        Assert.True(result.Succeeded);
        await users.AddToRoleAsync(user, "Customer");
    }

    private static LoginUserHandler MakeHandler(IServiceProvider services) => new(
        services.GetRequiredService<UserManager<ApplicationUser>>(),
        services.GetRequiredService<SignInManager<ApplicationUser>>(),
        services.GetRequiredService<AuthTokenIssuer>());

    [Fact]
    public async Task Succeeds_with_correct_credentials_and_issues_tokens()
    {
        using var host = new UsersTestHost();
        using var scope = host.NewScope();
        await SeedUserAsync(scope.ServiceProvider, "jane@example.com", "Password1");

        var handler = MakeHandler(scope.ServiceProvider);
        var result = await handler.Handle(new LoginUserCommand("jane@example.com", "Password1"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEmpty(result.Value!.Token);
        Assert.NotEmpty(result.Value!.RefreshToken);
    }

    [Fact]
    public async Task Fails_with_wrong_password()
    {
        using var host = new UsersTestHost();
        using var scope = host.NewScope();
        await SeedUserAsync(scope.ServiceProvider, "jane@example.com", "Password1");

        var handler = MakeHandler(scope.ServiceProvider);
        var result = await handler.Handle(new LoginUserCommand("jane@example.com", "WrongPassword"), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Fails_for_unknown_email()
    {
        using var host = new UsersTestHost();
        using var scope = host.NewScope();

        var handler = MakeHandler(scope.ServiceProvider);
        var result = await handler.Handle(new LoginUserCommand("nobody@example.com", "Password1"), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
