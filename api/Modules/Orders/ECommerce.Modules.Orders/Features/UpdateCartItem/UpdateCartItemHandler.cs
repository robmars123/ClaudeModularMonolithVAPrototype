using ECommerce.Modules.Orders.Features.GetCart;
using ECommerce.Modules.Orders.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Orders.Features.UpdateCartItem;

public class UpdateCartItemHandler : IRequestHandler<UpdateCartItemCommand, Result<CartResponse>>
{
    private readonly OrdersDbContext _db;
    public UpdateCartItemHandler(OrdersDbContext db) => _db = db;

    public async Task<Result<CartResponse>> Handle(UpdateCartItemCommand cmd, CancellationToken ct)
    {
        if (cmd.Quantity <= 0)
            return Result<CartResponse>.Failure("Quantity must be greater than zero.");

        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == cmd.UserId && c.Status == "Active", ct);

        if (cart is null) return Result<CartResponse>.Failure("Cart not found.");

        cart.UpdateItemQuantity(cmd.ProductId, cmd.Quantity);
        await _db.SaveChangesAsync(ct);

        var response = new CartResponse(
            cart.Id, cart.UserId,
            cart.Items.Select(i => new CartItemResponse(i.Id, i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.UnitPrice * i.Quantity)).ToList(),
            cart.TotalAmount, cart.Status, cart.CreatedAt);

        return Result<CartResponse>.Success(response);
    }
}
