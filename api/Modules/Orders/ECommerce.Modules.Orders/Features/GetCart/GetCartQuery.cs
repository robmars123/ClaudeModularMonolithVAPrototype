using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Orders.Features.GetCart;

public record GetCartQuery(string UserId) : IRequest<Result<CartResponse>>;

public record CartResponse(
    Guid Id, string UserId,
    IReadOnlyList<CartItemResponse> Items,
    decimal TotalAmount, string Status,
    DateTime CreatedAt);

public record CartItemResponse(
    Guid Id, Guid ProductId, string ProductName,
    decimal UnitPrice, int Quantity, decimal TotalPrice);
