using ECommerce.Modules.Customers.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Contracts.Customers;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Customers.Features.GetShippingAddress;

/// <summary>Serves the cross-module <see cref="GetCustomerShippingAddressRequest"/> contract (used by Orders as the default shipping address).</summary>
public class GetCustomerShippingAddressHandler : IRequestHandler<GetCustomerShippingAddressRequest, Result<CustomerShippingAddressDto>>
{
    private readonly CustomersDbContext _db;
    public GetCustomerShippingAddressHandler(CustomersDbContext db) => _db = db;

    public async Task<Result<CustomerShippingAddressDto>> Handle(GetCustomerShippingAddressRequest request, CancellationToken ct)
    {
        var customer = await _db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, ct);

        if (customer?.ShippingAddress is not { } address)
            return Result<CustomerShippingAddressDto>.Failure("No shipping address on file.");

        return Result<CustomerShippingAddressDto>.Success(
            new CustomerShippingAddressDto(address.Street, address.City, address.State, address.ZipCode, address.Country));
    }
}
