using ECommerce.Modules.Users.Entities;
using ECommerce.Modules.Users.Persistence;
using ECommerce.Modules.Users.Services;
using ECommerce.Shared.Common;
using ECommerce.Shared.IntegrationEvents;
using ECommerce.Shared.Mediator;
using ECommerce.Shared.Messaging;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Users.Features.RegisterUser;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<AuthResponse>>
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly UsersDbContext _db;
    private readonly TokenService _tokens;
    private readonly IConfiguration _config;
    private readonly IPublisher _publisher;
    private readonly IIntegrationEventPublisher _integrationEvents;
    private readonly ILogger<RegisterUserHandler> _logger;

    public RegisterUserHandler(UserManager<ApplicationUser> users, UsersDbContext db, TokenService tokens, IConfiguration config, IPublisher publisher, IIntegrationEventPublisher integrationEvents, ILogger<RegisterUserHandler> logger)
    {
        _users = users; _db = db; _tokens = tokens; _config = config; _publisher = publisher; _integrationEvents = integrationEvents; _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterUserCommand cmd, CancellationToken ct)
    {
        if (await _users.FindByEmailAsync(cmd.Email) is not null)
            return Result<AuthResponse>.Failure("Email already registered.");

        var role = cmd.Role is "Admin" or "Customer" ? cmd.Role : "Customer";
        var user = new ApplicationUser { FirstName = cmd.FirstName, LastName = cmd.LastName, Email = cmd.Email, UserName = cmd.Email };

        // UserManager.CreateAsync/AddToRoleAsync each call SaveChangesAsync internally against this
        // same UsersDbContext instance — wrapping them in an explicit transaction makes those saves
        // and the outbox row below commit (or roll back) together instead of independently.
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        var result = await _users.CreateAsync(user, cmd.Password);
        if (!result.Succeeded)
        {
            await transaction.RollbackAsync(ct);
            return Result<AuthResponse>.Failure(result.Errors.Select(e => e.Description).ToList());
        }

        await _users.AddToRoleAsync(user, role);

        var registeredEvent = new UserRegisteredEvent(user.Id, user.Email!, user.FirstName, user.LastName);

        // Pub/sub: broadcast on the topic exchange — every bound subscriber queue gets its own copy.
        // Enqueued into the same change tracker/transaction as the user, so it commits atomically with it.
        _integrationEvents.Enqueue(registeredEvent, MessageKind.PubSub, "user.registered");
        await _db.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);

        // Notify other modules (Customers creates the profile for this user). The user is already
        // saved, so a failure here is logged for reconciliation instead of failing the request.
        await _publisher.PublishBestEffort(registeredEvent, _logger, $"User {user.Email} registered but notifying Customers failed", ct);

        var roles = await _users.GetRolesAsync(user);
        var expiry = double.Parse(_config["JwtSettings:ExpiryMinutes"] ?? "60");

        return Result<AuthResponse>.Success(new AuthResponse(_tokens.Generate(user, roles), user.Email!, user.FullName, roles, DateTime.UtcNow.AddMinutes(expiry)));
    }
}
