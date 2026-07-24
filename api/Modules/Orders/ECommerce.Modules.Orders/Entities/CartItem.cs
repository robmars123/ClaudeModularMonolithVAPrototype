using ECommerce.Shared.Domain;

namespace ECommerce.Modules.Orders.Entities;

public class CartItem : BaseEntity
{
    public Guid CartId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal TotalPrice => UnitPrice * Quantity;

    private CartItem() { }

    public static CartItem Create(Guid cartId, Guid productId, string productName, decimal unitPrice, int quantity)
        => new()
        {
            Id = Guid.NewGuid(),
            CartId = cartId,
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity,
            CreatedAt = DateTime.UtcNow
        };

    public void UpdateQuantity(int quantity) => Quantity = quantity;
}
