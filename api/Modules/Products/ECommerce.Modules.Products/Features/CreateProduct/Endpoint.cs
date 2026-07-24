using System.Security.Claims;
using ECommerce.Modules.Products.Features.GetProducts;
using ECommerce.Shared.Mediator;
using FastEndpoints;
using GetProductByIdEndpoint = ECommerce.Modules.Products.Features.GetProductById.Endpoint;

namespace ECommerce.Modules.Products.Features.CreateProduct;

public class Endpoint : Endpoint<CreateProductRequest, ProductResponse>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/products");
        Roles("Admin");
    }

    public override async Task HandleAsync(CreateProductRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new CreateProductCommand(req.Name, req.Description, req.SKU, req.Price, req.CategoryId, req.ImageUrl, userId), ct);
        if (!result.IsSuccess) { await HttpContext.Response.SendAsync(new { errors = result.Errors }, 400, cancellation: ct); return; }
        await Send.CreatedAtAsync<GetProductByIdEndpoint>(new { id = result.Value!.Id }, result.Value, cancellation: ct);
    }
}

public record CreateProductRequest(string Name, string Description, string SKU, decimal Price, Guid CategoryId, string? ImageUrl);
