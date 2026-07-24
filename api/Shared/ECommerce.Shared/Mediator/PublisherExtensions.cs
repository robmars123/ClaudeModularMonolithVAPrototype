using Microsoft.Extensions.Logging;

namespace ECommerce.Shared.Mediator;

public static class PublisherExtensions
{
    /// <summary>
    /// Publishes a notification whose failure must not undo (or fail the request for) a write that
    /// already committed. Used right after <c>SaveChangesAsync</c> for the in-process side effect a
    /// notification triggers in another module (e.g. Inventory creating a stock record when Products
    /// publishes <c>ProductCreatedEvent</c>): the aggregate is already saved, so a failure here is
    /// logged for reconciliation instead of turning into a 500 for an operation that actually succeeded.
    /// </summary>
    public static async Task PublishBestEffort(this IPublisher publisher, INotification notification, ILogger logger, string failureMessage, CancellationToken ct)
    {
        try
        {
            await publisher.Publish(notification, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{FailureMessage}", failureMessage);
        }
    }
}
