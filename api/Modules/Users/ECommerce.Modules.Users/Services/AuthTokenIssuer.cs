using ECommerce.Modules.Users.Entities;
using ECommerce.Modules.Users.Features.RegisterUser;
using ECommerce.Modules.Users.Persistence;

namespace ECommerce.Modules.Users.Services;

/// <summary>
/// Single place that mints an access token + rotated refresh token pair. Every login-shaped flow
/// (register, login, refresh) goes through this instead of re-implementing token issuance.
/// </summary>
public sealed class AuthTokenIssuer(TokenService tokens, UsersDbContext db)
{
    public async Task<AuthResponse> IssueAsync(ApplicationUser user, IList<string> roles, CancellationToken ct)
    {
        var accessToken = tokens.GenerateAccessToken(user, roles);
        var (rawRefreshToken, refreshTokenHash) = tokens.GenerateRefreshToken();

        db.RefreshTokens.Add(RefreshToken.Create(user.Id, refreshTokenHash, DateTime.UtcNow.AddDays(tokens.RefreshTokenExpiryDays)));
        await db.SaveChangesAsync(ct);

        return new AuthResponse(
            accessToken, rawRefreshToken, user.Email!, user.FullName, roles,
            DateTime.UtcNow.AddMinutes(tokens.AccessTokenExpiryMinutes));
    }
}
