using ECommerce.Modules.Orders.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Orders.Features.RemoveFromCart;

public class RemoveFromCartHandler : IRequestHandler<RemoveFromCartCommand, Result>
{
    private readonly OrdersDbContext _db;
    public RemoveFromCartHandler(OrdersDbContext db) => _db = db;

    public async Task<Result> Handle(RemoveFromCartCommand cmd, CancellationToken ct)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == cmd.UserId && c.Status == "Active", ct);

        if (cart is null) return Result.Failure("Cart not found.");

        cart.RemoveItem(cmd.ProductId);
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
