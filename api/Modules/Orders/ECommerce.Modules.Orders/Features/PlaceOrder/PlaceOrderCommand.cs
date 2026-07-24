using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Orders.Features.PlaceOrder;

public record PlaceOrderCommand(string UserId, string? ShippingAddress, string? Notes) : IRequest<Result<OrderResponse>>;

public record OrderResponse(
    Guid Id, string OrderNumber, string UserId,
    string Status, decimal TotalAmount,
    string? ShippingAddress, string? Notes,
    IReadOnlyList<OrderItemResponse> Items,
    DateTime CreatedAt, DateTime? UpdatedAt);

public record OrderItemResponse(
    Guid Id, Guid ProductId, string ProductName,
    decimal UnitPrice, int Quantity, decimal TotalPrice);
