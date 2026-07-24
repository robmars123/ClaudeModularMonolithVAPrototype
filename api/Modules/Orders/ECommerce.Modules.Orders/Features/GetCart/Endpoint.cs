using System.Security.Claims;
using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Orders.Features.GetCart;

public class Endpoint : EndpointWithoutRequest<CartResponse>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure() => Get("/api/cart");

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new GetCartQuery(userId), ct);
        await Send.OkAsync(result.Value!, ct);
    }
}
