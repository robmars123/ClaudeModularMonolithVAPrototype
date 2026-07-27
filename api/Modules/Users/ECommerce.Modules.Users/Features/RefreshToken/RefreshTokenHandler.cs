using ECommerce.Modules.Users.Entities;
using ECommerce.Modules.Users.Features.RegisterUser;
using ECommerce.Modules.Users.Persistence;
using ECommerce.Modules.Users.Services;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Users.Features.RefreshToken;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    private readonly UsersDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly AuthTokenIssuer _issuer;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(UsersDbContext db, UserManager<ApplicationUser> users, AuthTokenIssuer issuer, ILogger<RefreshTokenHandler> logger)
    {
        _db = db; _users = users; _issuer = issuer; _logger = logger;
    }

    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand cmd, CancellationToken ct)
    {
        string hash = TokenService.Hash(cmd.RefreshTokenValue);
        Entities.RefreshToken? token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        if (token is null)
            return Result<AuthResponse>.Failure("Refresh token is invalid or expired. Please log in again.");

        // Checked before expiry: a stolen token replayed after its natural expiry is still a signal
        // worth revoking the family over, not just a generic "expired" reply.
        if (token.RevokedAt is not null)
        {
            _logger.LogWarning("Revoked refresh token replayed for user {UserId} — revoking all active sessions.", token.UserId);
            await RevokeFamilyAsync(token.UserId, ct);
            return Result<AuthResponse>.Failure("Session revoked. Please log in again.");
        }

        if (token.ExpiresAt <= DateTime.UtcNow)
            return Result<AuthResponse>.Failure("Refresh token is invalid or expired. Please log in again.");

        // Atomically claim this token for rotation. A plain read-then-write would let two concurrent
        // requests presenting the same still-valid token both "win" — the guarded update means only
        // one can; the loser's write affects zero rows, which is exactly the reuse-detection signal.
        int claimed = await _db.RefreshTokens
            .Where(t => t.Id == token.Id && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, DateTime.UtcNow), ct);

        if (claimed == 0)
        {
            _logger.LogWarning("Concurrent refresh detected for user {UserId} — revoking all active sessions.", token.UserId);
            await RevokeFamilyAsync(token.UserId, ct);
            return Result<AuthResponse>.Failure("Session revoked. Please log in again.");
        }

        ApplicationUser? user = await _users.FindByIdAsync(token.UserId);
        if (user is null || !user.IsActive)
            return Result<AuthResponse>.Failure("Refresh token is invalid or expired. Please log in again.");

        IList<string> roles = await _users.GetRolesAsync(user);
        AuthResponse response = await _issuer.IssueAsync(user, roles, ct);

        return Result<AuthResponse>.Success(response);
    }

    private Task RevokeFamilyAsync(string userId, CancellationToken ct) =>
        _db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, DateTime.UtcNow), ct);
}
