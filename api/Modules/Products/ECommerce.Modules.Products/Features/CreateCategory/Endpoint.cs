using System.Security.Claims;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using FastEndpoints;
using GetCategoriesEndpoint = ECommerce.Modules.Products.Features.GetCategories.Endpoint;

namespace ECommerce.Modules.Products.Features.CreateCategory;

public class Endpoint : Endpoint<CreateCategoryRequest, CategoryResponse>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/categories");
        Roles("Admin", "ProductManager");
    }

    public override async Task HandleAsync(CreateCategoryRequest req, CancellationToken ct)
    {
        string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        Result<CategoryResponse> result = await _mediator.Send(new CreateCategoryCommand(req.Name, req.Description, userId), ct);
        if (!result.IsSuccess) { await HttpContext.Response.SendAsync(new { errors = result.Errors }, 400, cancellation: ct); return; }
        await Send.CreatedAtAsync<GetCategoriesEndpoint>(new { }, result.Value, cancellation: ct);
    }
}

public record CreateCategoryRequest(string Name, string? Description);
