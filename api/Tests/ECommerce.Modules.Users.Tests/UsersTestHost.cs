using ECommerce.Modules.Users.Entities;
using ECommerce.Modules.Users.Persistence;
using ECommerce.Modules.Users.Services;
using ECommerce.Shared.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ECommerce.Modules.Users.Tests;

/// <summary>
/// A real (not mocked) Identity stack — UserManager/SignInManager/RoleManager backed by an EF Core
/// UserStore over an in-memory SQLite connection — so handler tests exercise the actual password
/// hashing, uniqueness, and role-assignment behavior Identity provides, not a hand-rolled substitute.
/// </summary>
public sealed class UsersTestHost : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _services;

    public UsersTestHost()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        ServiceCollection collection = new ServiceCollection();
        collection.AddLogging();
        collection.AddDataProtection();
        collection.AddHttpContextAccessor();
        collection.AddAuthentication();
        collection.AddDbContext<UsersDbContext>(o => o.UseSqlite(_connection));
        collection.AddIdentityCore<ApplicationUser>(o =>
            {
                o.Password.RequiredLength = 6;
                o.Password.RequireDigit = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<UsersDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        collection.AddSingleton<IOptions<JwtOptions>>(Options.Create(new JwtOptions
        {
            Secret = "test-secret-key-at-least-32-characters-long!",
            Issuer = "Tests",
            Audience = "Tests",
            AccessTokenExpiryMinutes = 15,
            RefreshTokenExpiryDays = 7,
        }));
        collection.AddScoped<TokenService>();
        collection.AddScoped<AuthTokenIssuer>();

        _services = collection.BuildServiceProvider();

        using IServiceScope scope = _services.CreateScope();
        scope.ServiceProvider.GetRequiredService<UsersDbContext>().Database.EnsureCreated();

        RoleManager<IdentityRole> roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (string role in Roles.All)
            roles.CreateAsync(new IdentityRole(role)).GetAwaiter().GetResult();
    }

    /// <summary>Every call opens a fresh DI scope (fresh UserManager/DbContext) against the same underlying database — mirrors one request's scoped lifetime.</summary>
    public IServiceScope NewScope() => _services.CreateScope();

    public void Dispose()
    {
        _services.Dispose();
        _connection.Dispose();
    }
}
