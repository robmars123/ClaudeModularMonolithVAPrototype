using ECommerce.Modules.Orders.Entities;
using ECommerce.Modules.Orders.Features.GetCart;
using ECommerce.Modules.Orders.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Contracts.Inventory;
using ECommerce.Shared.Contracts.Products;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Orders.Features.AddToCart;

public class AddToCartHandler : IRequestHandler<AddToCartCommand, Result<CartResponse>>
{
    private readonly OrdersDbContext _db;
    private readonly ISender _sender;

    public AddToCartHandler(OrdersDbContext db, ISender sender)
    {
        _db = db; _sender = sender;
    }

    public async Task<Result<CartResponse>> Handle(AddToCartCommand cmd, CancellationToken ct)
    {
        if (cmd.Quantity <= 0)
            return Result<CartResponse>.Failure("Quantity must be greater than zero.");

        // Resolve name/price from the Products module — the client only sends an id.
        var productResult = await _sender.Send(new GetProductPricingRequest(cmd.ProductId), ct);
        if (!productResult.IsSuccess)
            return Result<CartResponse>.Failure("Product not found.");

        var product = productResult.Value!;
        if (!product.IsActive)
            return Result<CartResponse>.Failure($"Product '{product.Name}' is no longer available.");

        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == cmd.UserId && c.Status == "Active", ct);

        if (cart is null)
        {
            cart = Cart.Create(cmd.UserId);
            _db.Carts.Add(cart);
        }

        // Check availability from the Inventory module against the total quantity in the cart.
        var alreadyInCart = cart.Items.FirstOrDefault(i => i.ProductId == cmd.ProductId)?.Quantity ?? 0;
        var availability = await _sender.Send(new GetStockAvailabilityRequest(cmd.ProductId), ct);
        var available = availability.IsSuccess ? availability.Value : 0;

        if (alreadyInCart + cmd.Quantity > available)
            return Result<CartResponse>.Failure($"Only {available} unit(s) of '{product.Name}' available.");

        cart.AddItem(cmd.ProductId, product.Name, product.Price, cmd.Quantity);

        // New items carry a pre-set Guid key, so change detection on a tracked cart would
        // mistake them for existing rows (UPDATE instead of INSERT) — mark them Added explicitly.
        var item = cart.Items.First(i => i.ProductId == cmd.ProductId);
        if (_db.Entry(item).State == EntityState.Detached)
            _db.CartItems.Add(item);

        await _db.SaveChangesAsync(ct);

        var response = new CartResponse(
            cart.Id, cart.UserId,
            cart.Items.Select(i => new CartItemResponse(i.Id, i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.UnitPrice * i.Quantity)).ToList(),
            cart.TotalAmount, cart.Status, cart.CreatedAt);

        return Result<CartResponse>.Success(response);
    }
}
