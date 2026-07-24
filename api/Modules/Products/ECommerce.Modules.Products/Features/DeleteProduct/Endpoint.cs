using System.Security.Claims;
using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Products.Features.DeleteProduct;

public class Endpoint : Endpoint<DeleteProductRequest>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Delete("/api/products/{id}");
        Roles("Admin");
    }

    public override async Task HandleAsync(DeleteProductRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new DeleteProductCommand(req.Id, userId), ct);
        if (!result.IsSuccess) { await HttpContext.Response.SendAsync(new { error = result.Error }, 404, cancellation: ct); return; }
        await Send.NoContentAsync(ct);
    }
}

public record DeleteProductRequest(Guid Id);
