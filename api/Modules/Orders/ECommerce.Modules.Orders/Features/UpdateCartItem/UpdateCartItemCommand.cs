using ECommerce.Modules.Orders.Features.GetCart;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Orders.Features.UpdateCartItem;

public record UpdateCartItemCommand(string UserId, Guid ProductId, int Quantity) : IRequest<Result<CartResponse>>;
