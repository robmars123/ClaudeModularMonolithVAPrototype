using ECommerce.Modules.Orders.Features.PlaceOrder;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Orders.Features.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId, string UserId, bool IsAdmin = false) : IRequest<Result<OrderResponse>>;
