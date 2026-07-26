using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Inventory.Features.GetInventory;

public class Endpoint : Endpoint<GetInventoryRequest, PagedResult<InventoryResponse>>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/inventory");
        Roles("Admin", "ProductManager");
    }

    public override async Task HandleAsync(GetInventoryRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetInventoryQuery(req.Page, req.PageSize, req.LowStockOnly), ct);
        await Send.OkAsync(result.Value!, ct);
    }
}

public record GetInventoryRequest(int Page = 1, int PageSize = 20, bool LowStockOnly = false);
