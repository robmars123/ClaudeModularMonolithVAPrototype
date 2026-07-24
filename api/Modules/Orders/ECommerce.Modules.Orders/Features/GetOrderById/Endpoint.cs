using System.Security.Claims;
using ECommerce.Modules.Orders.Features.PlaceOrder;
using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Orders.Features.GetOrderById;

public class Endpoint : Endpoint<GetOrderByIdRequest, OrderResponse>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure() => Get("/api/orders/{orderId}");

    public override async Task HandleAsync(GetOrderByIdRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var isAdmin = User.IsInRole("Admin");
        var result = await _mediator.Send(new GetOrderByIdQuery(req.OrderId, userId, isAdmin), ct);
        if (!result.IsSuccess) { await HttpContext.Response.SendAsync(new { error = result.Error }, 404, cancellation: ct); return; }
        await Send.OkAsync(result.Value!, ct);
    }
}

public record GetOrderByIdRequest(Guid OrderId);
