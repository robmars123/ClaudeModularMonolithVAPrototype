using ECommerce.Shared.Domain;

namespace ECommerce.Modules.Inventory.Entities;

public class InventoryItem : AuditableEntity
{
    public Guid ProductId { get; private set; }
    public int QuantityOnHand { get; private set; }
    public int ReservedQuantity { get; private set; }
    public int ReorderLevel { get; private set; }
    public int ReorderQuantity { get; private set; }

    public int AvailableQuantity => QuantityOnHand - ReservedQuantity;
    public bool NeedsReorder => AvailableQuantity <= ReorderLevel;

    private InventoryItem() { }

    public static InventoryItem Create(Guid productId, int quantity, int reorderLevel, int reorderQuantity, string createdBy)
    {
        var i = new InventoryItem { ProductId = productId, QuantityOnHand = quantity, ReorderLevel = reorderLevel, ReorderQuantity = reorderQuantity };
        i.SetCreatedBy(createdBy);
        return i;
    }

    public bool TryReserve(int qty) { if (AvailableQuantity < qty) return false; ReservedQuantity += qty; SetUpdatedAt(); return true; }
    public void Release(int qty) { ReservedQuantity = Math.Max(0, ReservedQuantity - qty); SetUpdatedAt(); }
    public void Commit(int qty, string by) { QuantityOnHand -= qty; ReservedQuantity -= qty; SetUpdatedBy(by); }
    public void AddStock(int qty, string by) { QuantityOnHand += qty; SetUpdatedBy(by); }
}
