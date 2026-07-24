using ECommerce.Modules.Customers.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Customers.Features.GetMyProfile;

public class GetMyProfileHandler : IRequestHandler<GetMyProfileQuery, Result<CustomerResponse>>
{
    private readonly CustomersDbContext _db;
    public GetMyProfileHandler(CustomersDbContext db) => _db = db;

    public async Task<Result<CustomerResponse>> Handle(GetMyProfileQuery q, CancellationToken ct)
    {
        var c = await _db.Customers.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == q.UserId, ct);
        if (c is null) return Result<CustomerResponse>.Failure("Customer profile not found.");

        var addr = c.ShippingAddress is null ? null : new AddressResponse(c.ShippingAddress.Street, c.ShippingAddress.City, c.ShippingAddress.State, c.ShippingAddress.ZipCode, c.ShippingAddress.Country);
        return Result<CustomerResponse>.Success(new CustomerResponse(c.Id, c.UserId, c.FirstName, c.LastName, c.FullName, c.Email, c.Phone, addr, c.CreatedAt));
    }
}
