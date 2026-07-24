using ECommerce.Modules.Customers.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Customers.Features.GetAllCustomers;

public class GetAllCustomersHandler : IRequestHandler<GetAllCustomersQuery, Result<PagedResult<CustomerListResponse>>>
{
    private readonly CustomersDbContext _db;
    public GetAllCustomersHandler(CustomersDbContext db) => _db = db;

    public async Task<Result<PagedResult<CustomerListResponse>>> Handle(GetAllCustomersQuery q, CancellationToken ct)
    {
        var query = _db.Customers.AsNoTracking().Where(c => c.IsActive);

        var total = await query.CountAsync(ct);
        var customers = await query
            .OrderBy(c => c.LastName).Skip((q.Page - 1) * q.PageSize).Take(q.PageSize)
            .Select(c => new CustomerListResponse(c.Id, c.UserId, c.FirstName, c.LastName, c.FullName, c.Email, c.Phone, null, c.CreatedAt))
            .ToListAsync(ct);

        return Result<PagedResult<CustomerListResponse>>.Success(new PagedResult<CustomerListResponse>(customers, total, q.Page, q.PageSize));
    }
}
