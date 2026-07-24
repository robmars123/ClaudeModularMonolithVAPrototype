using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Products.Features.GetProducts;

public class Endpoint : Endpoint<GetProductsRequest>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/products");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetProductsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetProductsQuery(req.Page, req.PageSize, req.Search, req.CategoryId, req.IsActive), ct);
        await Send.OkAsync(result.Value!, ct);
    }
}

public record GetProductsRequest(int Page = 1, int PageSize = 20, string? Search = null, Guid? CategoryId = null, bool? IsActive = true);
