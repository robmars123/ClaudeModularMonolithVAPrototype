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
        var orders = await _db.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .Where(o => o.UserId == q.UserId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

        var result = orders.Select(o => new OrderResponse(
            o.Id, o.OrderNumber, o.UserId,
            o.Status.ToString(), o.TotalAmount,
            o.ShippingAddress, o.Notes,
            o.Items.Select(i => new OrderItemResponse(i.Id, i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.TotalPrice)).ToList(),
            o.CreatedAt, o.UpdatedAt)).ToList();

        return Result<IReadOnlyList<OrderResponse>>.Success(result);
    }
}
