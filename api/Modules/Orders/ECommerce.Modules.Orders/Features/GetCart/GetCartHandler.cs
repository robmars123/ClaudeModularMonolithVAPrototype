using ECommerce.Modules.Orders.Entities;
using ECommerce.Modules.Orders.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Orders.Features.GetCart;

public class GetCartHandler : IRequestHandler<GetCartQuery, Result<CartResponse>>
{
    private readonly OrdersDbContext _db;
    public GetCartHandler(OrdersDbContext db) => _db = db;

    public async Task<Result<CartResponse>> Handle(GetCartQuery q, CancellationToken ct)
    {
        Cart? cart = await _db.Carts
            .Include(c => c.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == q.UserId && c.Status == "Active", ct);

        if (cart is null)
            return Result<CartResponse>.Success(new CartResponse(Guid.Empty, q.UserId, [], 0, "Empty", DateTime.UtcNow));

        CartResponse response = new CartResponse(
            cart.Id, cart.UserId,
            cart.Items.Select(i => new CartItemResponse(i.Id, i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.UnitPrice * i.Quantity)).ToList(),
            cart.TotalAmount, cart.Status, cart.CreatedAt);

        return Result<CartResponse>.Success(response);
    }
}
