using ECommerce.Modules.Products.Features.CreateCategory;
using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Products.Features.GetCategories;

public class Endpoint : EndpointWithoutRequest<IReadOnlyList<CategoryResponse>>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/categories");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(), ct);
        await Send.OkAsync(result.Value!, ct);
    }
}
