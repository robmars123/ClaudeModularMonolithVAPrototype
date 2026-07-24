using ECommerce.Modules.Orders.Features.PlaceOrder;
using ECommerce.Modules.Orders.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Orders.Features.GetOrderById;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderResponse>>
{
    private readonly OrdersDbContext _db;
    public GetOrderByIdHandler(OrdersDbContext db) => _db = db;

    public async Task<Result<OrderResponse>> Handle(GetOrderByIdQuery q, CancellationToken ct)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == q.OrderId, ct);

        if (order is null) return Result<OrderResponse>.Failure("Order not found.");
        if (!q.IsAdmin && order.UserId != q.UserId) return Result<OrderResponse>.Failure("Order not found.");

        return Result<OrderResponse>.Success(new OrderResponse(
            order.Id, order.OrderNumber, order.UserId,
            order.Status.ToString(), order.TotalAmount,
            order.ShippingAddress, order.Notes,
            order.Items.Select(i => new OrderItemResponse(i.Id, i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.TotalPrice)).ToList(),
            order.CreatedAt, order.UpdatedAt));
    }
}
