using ECommerce.Modules.Inventory.Persistence;
using ECommerce.Shared.IntegrationEvents;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Inventory.Integration;

/// <summary>Converts the reservation made during checkout into a real stock deduction once the order is persisted.</summary>
public class OrderPlacedHandler : INotificationHandler<OrderPlacedEvent>
{
    private readonly InventoryDbContext _db;
    private readonly ILogger<OrderPlacedHandler> _logger;

    public OrderPlacedHandler(InventoryDbContext db, ILogger<OrderPlacedHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Handle(OrderPlacedEvent notification, CancellationToken ct)
    {
        foreach (var line in notification.Lines)
        {
            var item = await _db.InventoryItems
                .FirstOrDefaultAsync(i => i.ProductId == line.ProductId, ct);

            if (item is null)
            {
                _logger.LogWarning(
                    "Order {OrderNumber} references product {ProductId} with no inventory record; commit skipped",
                    notification.OrderNumber, line.ProductId);
                continue;
            }

            item.Commit(line.Quantity, notification.UserId);
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Committed stock for order {OrderNumber}", notification.OrderNumber);
    }
}
