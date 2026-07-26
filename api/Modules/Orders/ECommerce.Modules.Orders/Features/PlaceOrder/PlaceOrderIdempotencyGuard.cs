using System.Collections.Concurrent;

namespace ECommerce.Modules.Orders.Features.PlaceOrder;

/// <summary>
/// Closes the narrow race where a client retries PlaceOrder (double-click, flaky network) while the
/// first request is still mid-flight. Once the first commits, the cart is empty and a retry naturally
/// no-ops via the "Cart is empty" check — this only guards the window *before* that commit. A per-user
/// lease with a short TTL, not a distributed idempotency key: sufficient for the agreed single-instance
/// deployment, and cheap enough not to need a table or Redis.
/// </summary>
public sealed class PlaceOrderIdempotencyGuard
{
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromSeconds(10);
    private readonly ConcurrentDictionary<string, (Guid Token, DateTime ExpiresAt)> _leases = new();

    /// <summary>
    /// Atomically claims the lease for <paramref name="userId"/>, returning a token that must be
    /// passed back to <see cref="Release"/>. Null if another PlaceOrder for the same user is already
    /// in flight and its lease hasn't expired.
    /// </summary>
    public Guid? TryAcquire(string userId)
    {
        var now = DateTime.UtcNow;
        var claim = (Token: Guid.NewGuid(), ExpiresAt: now.Add(LeaseDuration));

        if (_leases.TryAdd(userId, claim))
            return claim.Token;

        // A lease exists — only take over if it's stale (e.g. a prior request crashed without releasing).
        if (_leases.TryGetValue(userId, out var existing) && existing.ExpiresAt <= now
            && _leases.TryUpdate(userId, claim, existing))
        {
            return claim.Token;
        }

        return null;
    }

    /// <summary>
    /// Releases the lease only if <paramref name="token"/> still owns it — a request whose lease already
    /// expired and was taken over by a newer request must not delete that newer request's lease out from
    /// under it.
    /// </summary>
    public void Release(string userId, Guid token)
    {
        if (_leases.TryGetValue(userId, out var current) && current.Token == token)
            _leases.TryRemove(new KeyValuePair<string, (Guid, DateTime)>(userId, current));
    }
}
