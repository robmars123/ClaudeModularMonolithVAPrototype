using ECommerce.Modules.Orders.Entities;
using ECommerce.Modules.Orders.Features.PlaceOrder;
using ECommerce.Modules.Orders.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Orders.Features.GetMyOrders;

public class GetMyOrdersHandler : IRequestHandler<GetMyOrdersQuery, Result<IReadOnlyList<OrderResponse>>>
{
    private readonly OrdersDbContext _db;
    public GetMyOrdersHandler(OrdersDbContext db) => _db = db;

    public async Task<Result<IReadOnlyList<OrderResponse>>> Handle(GetMyOrdersQuery q, CancellationToken ct)
    {
        List<Order> orders = await _db.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .Where(o => o.UserId == q.UserId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

        List<OrderResponse> result = orders.Select(OrderResponse.FromEntity).ToList();

        return Result<IReadOnlyList<OrderResponse>>.Success(result);
    }
}
