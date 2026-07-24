using ECommerce.Modules.Orders.Features.GetCart;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Orders.Features.AddToCart;

/// <summary>Name and price are resolved from the Products module — never trusted from the client.</summary>
public record AddToCartCommand(string UserId, Guid ProductId, int Quantity)
    : IRequest<Result<CartResponse>>;
