using ECommerce.Modules.Orders.Enums;
using ECommerce.Shared.Domain;

namespace ECommerce.Modules.Orders.Entities;

public class Order : AuditableEntity
{
    public string UserId { get; private set; } = default!;
    public string OrderNumber { get; private set; } = default!;
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? ShippingAddress { get; private set; }
    public string? Notes { get; private set; }
    public DateTime? ShippedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    private readonly List<OrderItem> _items = [];
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public static Order Create(string userId, IEnumerable<CartItem> cartItems, string? shippingAddress, string? notes)
    {
        var items = cartItems.ToList();
        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            OrderNumber = GenerateOrderNumber(),
            Status = OrderStatus.Pending,
            TotalAmount = items.Sum(i => i.TotalPrice),
            ShippingAddress = shippingAddress,
            Notes = notes,
        };
        order.SetCreatedBy(userId);

        foreach (var cartItem in items)
            order._items.Add(OrderItem.Create(order.Id, cartItem.ProductId, cartItem.ProductName, cartItem.UnitPrice, cartItem.Quantity));

        return order;
    }

    public void Confirm(string by)
    {
        Status = OrderStatus.Confirmed;
        SetUpdatedBy(by);
    }

    public void Ship(string by)
    {
        Status = OrderStatus.Shipped;
        ShippedAt = DateTime.UtcNow;
        SetUpdatedBy(by);
    }

    public void Deliver(string by)
    {
        Status = OrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        SetUpdatedBy(by);
    }

    public void Cancel(string by)
    {
        Status = OrderStatus.Cancelled;
        SetUpdatedBy(by);
    }

    private static string GenerateOrderNumber()
        => $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
}
