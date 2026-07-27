using System.Text;

namespace ECommerce.Shared.Messaging;

/// <summary>
/// Pure attempt-counting logic for <see cref="RabbitMqConsumerBackgroundService"/>'s bounded-retry
/// loop, kept out of the background service itself so it's unit-testable without a real broker.
/// </summary>
public static class RetryPolicy
{
    public const int MaxAttempts = 5;
    private const string AttemptHeaderKey = "x-retry-attempt";

    public static int GetAttempt(IDictionary<string, object?>? headers)
    {
        if (headers is null || !headers.TryGetValue(AttemptHeaderKey, out object? value) || value is null)
            return 0;

        return value switch
        {
            int i => i,
            long l => (int)l,
            byte[] bytes => int.TryParse(Encoding.UTF8.GetString(bytes), out int parsed) ? parsed : 0,
            _ => 0
        };
    }

    public static Dictionary<string, object?> WithIncrementedAttempt(IDictionary<string, object?>? headers)
    {
        Dictionary<string, object?> next = headers is null
            ? new Dictionary<string, object?>()
            : new Dictionary<string, object?>(headers);

        next[AttemptHeaderKey] = GetAttempt(headers) + 1;
        return next;
    }

    /// <summary>Exponential backoff before a retry republish, capped so a poison message can't stall the consumer for long.</summary>
    public static TimeSpan BackoffFor(int attempt) =>
        TimeSpan.FromSeconds(Math.Min(Math.Pow(2, attempt), 30));
}
