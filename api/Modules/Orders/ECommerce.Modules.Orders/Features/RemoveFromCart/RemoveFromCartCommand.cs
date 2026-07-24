using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Orders.Features.RemoveFromCart;

public record RemoveFromCartCommand(string UserId, Guid ProductId) : IRequest<Result>;
