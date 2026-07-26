using ECommerce.Shared.Domain;

namespace ECommerce.Modules.Users.Entities;

/// <summary>
/// A rotated refresh token. Only the SHA-256 hash of the raw token is ever persisted. On refresh the
/// presented token is revoked and replaced — presenting an already-revoked token again means the token
/// was stolen/replayed, so the entire family sharing <see cref="UserId"/> is revoked (reuse detection).
/// </summary>
public class RefreshToken : BaseEntity
{
    public string UserId { get; private set; } = string.Empty;
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    private RefreshToken() { }

    public static RefreshToken Create(string userId, string tokenHash, DateTime expiresAt) =>
        new() { UserId = userId, TokenHash = tokenHash, ExpiresAt = expiresAt };

    public bool IsActive => RevokedAt is null && ExpiresAt > DateTime.UtcNow;

    public void Revoke() => RevokedAt ??= DateTime.UtcNow;
}
