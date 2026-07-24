using ECommerce.Modules.Products.Features.GetProducts;
using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Products.Features.GetProductById;

public class Endpoint : Endpoint<GetProductByIdRequest, ProductResponse>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/products/{id}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetProductByIdRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(req.Id), ct);
        if (!result.IsSuccess) { await HttpContext.Response.SendAsync(new { error = result.Error }, 404, cancellation: ct); return; }
        await Send.OkAsync(result.Value!, ct);
    }
}

public record GetProductByIdRequest(Guid Id);
