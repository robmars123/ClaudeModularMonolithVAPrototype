using System.Security.Claims;
using ECommerce.Modules.Products.Features.GetProducts;
using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Products.Features.UpdateProduct;

public class Endpoint : Endpoint<UpdateProductRequest, ProductResponse>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/products/{id}");
        Roles("Admin");
    }

    public override async Task HandleAsync(UpdateProductRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _mediator.Send(new UpdateProductCommand(req.Id, req.Name, req.Description, req.Price, req.CategoryId, req.ImageUrl, userId), ct);
        if (!result.IsSuccess)
        {
            await HttpContext.Response.SendAsync(new { errors = result.Errors }, result.Error!.Contains("not found") ? 404 : 400, cancellation: ct);
            return;
        }
        await Send.OkAsync(result.Value!, ct);
    }
}

public record UpdateProductRequest(Guid Id, string Name, string Description, decimal Price, Guid CategoryId, string? ImageUrl);
