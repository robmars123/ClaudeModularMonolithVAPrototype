using ECommerce.Modules.Orders.Features.PlaceOrder;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Orders.Features.GetMyOrders;

public record GetMyOrdersQuery(string UserId) : IRequest<Result<IReadOnlyList<OrderResponse>>>;
