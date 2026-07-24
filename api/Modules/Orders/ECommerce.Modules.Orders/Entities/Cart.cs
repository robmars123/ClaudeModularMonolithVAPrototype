using ECommerce.Shared.Domain;

namespace ECommerce.Modules.Orders.Entities;

public class Cart : BaseEntity
{
    public string UserId { get; private set; } = default!;
    public string Status { get; private set; } = "Active";
    public new DateTime UpdatedAt { get; private set; }

    private readonly List<CartItem> _items = [];
    public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

    public decimal TotalAmount => _items.Sum(i => i.TotalPrice);

    private Cart() { }

    public static Cart Create(string userId)
    {
        var cart = new Cart
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        return cart;
    }

    public void AddItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        var existing = _items.FirstOrDefault(i => i.ProductId == productId);
        if (existing is not null)
        {
            existing.UpdateQuantity(existing.Quantity + quantity);
        }
        else
        {
            _items.Add(CartItem.Create(Id, productId, productName, unitPrice, quantity));
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(Guid productId)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is not null)
        {
            _items.Remove(item);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item is not null)
        {
            item.UpdateQuantity(quantity);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Clear()
    {
        _items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }
}
