using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using FastEndpoints;

namespace ECommerce.Modules.Customers.Features.GetAllCustomers;

public class Endpoint : Endpoint<GetAllCustomersRequest, PagedResult<CustomerListResponse>>
{
    private readonly IMediator _mediator;
    public Endpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/customers");
        Roles("Admin", "Support");
    }

    public override async Task HandleAsync(GetAllCustomersRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllCustomersQuery(req.Page, req.PageSize), ct);
        await Send.OkAsync(result.Value!, ct);
    }
}

public record GetAllCustomersRequest(int Page = 1, int PageSize = 20);
