using ECommerce.Shared.Domain;

namespace ECommerce.Modules.Orders.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = default!;
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal TotalPrice { get; private set; }

    private OrderItem() { }

    public static OrderItem Create(Guid orderId, Guid productId, string productName, decimal unitPrice, int quantity)
        => new()
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity,
            TotalPrice = unitPrice * quantity,
            CreatedAt = DateTime.UtcNow
        };
}
