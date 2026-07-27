using ECommerce.Modules.Orders.Entities;
using ECommerce.Modules.Orders.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Orders.Features.ClearCart;

public class ClearCartHandler : IRequestHandler<ClearCartCommand, Result>
{
    private readonly OrdersDbContext _db;
    public ClearCartHandler(OrdersDbContext db) => _db = db;

    public async Task<Result> Handle(ClearCartCommand cmd, CancellationToken ct)
    {
        Cart? cart = await _db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == cmd.UserId && c.Status == "Active", ct);

        if (cart is null) return Result.Success();

        cart.Clear();
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
