using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Orders.Features.ClearCart;

public record ClearCartCommand(string UserId) : IRequest<Result>;
