using ECommerce.Modules.Users.Entities;
using ECommerce.Modules.Users.Persistence;
using ECommerce.Modules.Users.Services;
using ECommerce.Shared.Common;
using ECommerce.Shared.IntegrationEvents;
using ECommerce.Shared.Mediator;
using ECommerce.Shared.Messaging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Users.Features.RegisterUser;

/// <summary>Public self-registration — always provisions a Customer. Staff accounts are provisioned separately via CreateStaffUser (Admin-only).</summary>
public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, Result<AuthResponse>>
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly UsersDbContext _db;
    private readonly AuthTokenIssuer _issuer;
    private readonly IPublisher _publisher;
    private readonly IIntegrationEventPublisher _integrationEvents;
    private readonly ILogger<RegisterUserHandler> _logger;

    public RegisterUserHandler(UserManager<ApplicationUser> users, UsersDbContext db, AuthTokenIssuer issuer, IPublisher publisher, [FromKeyedServices(UsersModule.PublisherKey)] IIntegrationEventPublisher integrationEvents, ILogger<RegisterUserHandler> logger)
    {
        _users = users; _db = db; _issuer = issuer; _publisher = publisher; _integrationEvents = integrationEvents; _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(RegisterUserCommand cmd, CancellationToken ct)
    {
        Result<(ApplicationUser User, UserRegisteredEvent Event)> creation = await CreateIdentityUserAsync(cmd, ct);
        if (!creation.IsSuccess)
            return Result<AuthResponse>.Failure(creation.Errors);

        (ApplicationUser? user, UserRegisteredEvent? registeredEvent) = creation.Value!;

        // Notify other modules (Customers creates the profile for this user). The user is already
        // saved, so a failure here is logged for reconciliation instead of failing the request.
        await _publisher.PublishBestEffort(registeredEvent, _logger, $"User {user.Email} registered but notifying Customers failed", ct);

        IList<string> roles = await _users.GetRolesAsync(user);
        AuthResponse response = await _issuer.IssueAsync(user, roles, ct);
        return Result<AuthResponse>.Success(response);
    }

    private async Task<Result<(ApplicationUser User, UserRegisteredEvent Event)>> CreateIdentityUserAsync(RegisterUserCommand cmd, CancellationToken ct)
    {
        if (await _users.FindByEmailAsync(cmd.Email) is not null)
            return Result<(ApplicationUser, UserRegisteredEvent)>.Failure("Email already registered.");

        ApplicationUser user = new ApplicationUser { FirstName = cmd.FirstName, LastName = cmd.LastName, Email = cmd.Email, UserName = cmd.Email };
        UserRegisteredEvent registeredEvent = new UserRegisteredEvent(user.Id, user.Email!, user.FirstName, user.LastName);

        // UserManager.CreateAsync/AddToRoleAsync each call SaveChangesAsync internally against this
        // same UsersDbContext instance — wrapping them in an explicit transaction makes those saves
        // and the outbox row below commit (or roll back) together instead of independently.
        await using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(ct);

        IdentityResult identityResult = await _users.CreateAsync(user, cmd.Password);
        if (!identityResult.Succeeded)
        {
            await transaction.RollbackAsync(ct);
            return Result<(ApplicationUser, UserRegisteredEvent)>.Failure(identityResult.Errors.Select(e => e.Description).ToList());
        }

        await _users.AddToRoleAsync(user, Roles.Customer);

        // Pub/sub: broadcast on the topic exchange — every bound subscriber queue gets its own copy.
        // Enqueued into the same change tracker/transaction as the user, so it commits atomically with it.
        // Destination here is the topic routing key ("user.registered"), not a queue name — each
        // subscriber (Customers, EmailWorker) binds its own queue to this routing key independently.
        _integrationEvents.Enqueue(registeredEvent, MessageKind.PubSub, "user.registered");
        await _db.SaveChangesAsync(ct);

        await transaction.CommitAsync(ct);
        return Result<(ApplicationUser, UserRegisteredEvent)>.Success((user, registeredEvent));
    }
}