using System.Security.Claims;
using ECommerce.Shared.Mediator;
using FastEndpoints;
using GetOrderByIdEndpoint = ECommerce.Modules.Orders.Features.GetOrderById.Endpoint;

namespace ECommerce.Modules.Orders.Features.PlaceOrder;

public class Endpoint : Endpoint<PlaceOrderRequest, OrderResponse>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure() => Post("/api/orders/place");

    public override async Task HandleAsync(PlaceOrderRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new PlaceOrderCommand(userId, req.ShippingAddress, req.Notes), ct);
        if (!result.IsSuccess) { await HttpContext.Response.SendAsync(new { error = result.Error }, 400, cancellation: ct); return; }
        await Send.CreatedAtAsync<GetOrderByIdEndpoint>(new { orderId = result.Value!.Id }, result.Value, cancellation: ct);
    }
}

public record PlaceOrderRequest(string? ShippingAddress, string? Notes);
