using ECommerce.Modules.Users.Persistence;
using ECommerce.Modules.Users.Services;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Users.Features.Logout;

public class LogoutHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly UsersDbContext _db;
    public LogoutHandler(UsersDbContext db) => _db = db;

    public async Task<Result> Handle(LogoutCommand cmd, CancellationToken ct)
    {
        var hash = TokenService.Hash(cmd.RefreshTokenValue);
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash, ct);

        // Idempotent: an already-revoked/unknown token still "succeeds" from the caller's perspective —
        // the outcome they want (session dead) already holds.
        if (token is not null && token.RevokedAt is null)
        {
            token.Revoke();
            await _db.SaveChangesAsync(ct);
        }

        return Result.Success();
    }
}
