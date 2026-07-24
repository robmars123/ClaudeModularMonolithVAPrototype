using ECommerce.Modules.Inventory.Entities;
using ECommerce.Modules.Inventory.Persistence;
using ECommerce.Shared.IntegrationEvents;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Inventory.Integration;

/// <summary>Creates the inventory record for every new product so stock can be tracked from day one.</summary>
public class ProductCreatedHandler : INotificationHandler<ProductCreatedEvent>
{
    private const int DefaultReorderLevel = 10;
    private const int DefaultReorderQuantity = 50;

    private readonly InventoryDbContext _db;
    private readonly ILogger<ProductCreatedHandler> _logger;

    public ProductCreatedHandler(InventoryDbContext db, ILogger<ProductCreatedHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Handle(ProductCreatedEvent notification, CancellationToken ct)
    {
        if (await _db.InventoryItems.AnyAsync(i => i.ProductId == notification.ProductId, ct))
            return;

        var item = InventoryItem.Create(notification.ProductId, 0, DefaultReorderLevel, DefaultReorderQuantity, notification.CreatedBy);
        _db.InventoryItems.Add(item);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created inventory record for product {ProductId} ({Sku})", notification.ProductId, notification.Sku);
    }
}
