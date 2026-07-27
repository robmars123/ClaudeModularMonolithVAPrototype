using System.Security.Claims;
using ECommerce.Modules.Orders.Features.PlaceOrder;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Orders.Features.GetMyOrders;

public class Endpoint : EndpointWithoutRequest<IReadOnlyList<OrderResponse>>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure() => Get("/api/orders");

    public override async Task HandleAsync(CancellationToken ct)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Result<IReadOnlyList<OrderResponse>> result = await _mediator.Send(new GetMyOrdersQuery(userId), ct);
        await Send.OkAsync(result.Value!, ct);
    }
}
