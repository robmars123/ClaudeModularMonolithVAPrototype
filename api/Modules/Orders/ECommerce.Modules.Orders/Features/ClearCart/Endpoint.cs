using System.Security.Claims;
using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Orders.Features.ClearCart;

public class Endpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure() => Delete("/api/cart");

    public override async Task HandleAsync(CancellationToken ct)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _mediator.Send(new ClearCartCommand(userId), ct);
        await Send.NoContentAsync(ct);
    }
}
