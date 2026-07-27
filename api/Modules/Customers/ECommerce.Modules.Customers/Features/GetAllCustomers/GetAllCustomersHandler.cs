using ECommerce.Modules.Customers.Entities;
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
        IOrderedQueryable<Customer> query = _db.Customers.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.LastName);

        PagedResult<CustomerListResponse> result = await query.ToPagedResultAsync(
            q.Page, q.PageSize,
            c => new CustomerListResponse(c.Id, c.UserId, c.FirstName, c.LastName, c.FullName, c.Email, c.Phone, null, c.CreatedAt),
            ct);

        return Result<PagedResult<CustomerListResponse>>.Success(result);
    }
}
