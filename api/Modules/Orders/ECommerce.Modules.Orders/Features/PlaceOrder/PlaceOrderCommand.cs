using ECommerce.Modules.Orders.Entities;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Orders.Features.PlaceOrder;

public record PlaceOrderCommand(string UserId, string? ShippingAddress, string? Notes) : IRequest<Result<OrderResponse>>;

public record OrderResponse(
    Guid Id, string OrderNumber, string UserId,
    string Status, decimal TotalAmount,
    string? ShippingAddress, string? Notes,
    IReadOnlyList<OrderItemResponse> Items,
    DateTime CreatedAt, DateTime? UpdatedAt)
{
    public static OrderResponse FromEntity(Order o) => new(
        o.Id, o.OrderNumber, o.UserId,
        o.Status.ToString(), o.TotalAmount,
        o.ShippingAddress, o.Notes,
        o.Items.Select(i => new OrderItemResponse(i.Id, i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.TotalPrice)).ToList(),
        o.CreatedAt, o.UpdatedAt);
}

public record OrderItemResponse(
    Guid Id, Guid ProductId, string ProductName,
    decimal UnitPrice, int Quantity, decimal TotalPrice);
