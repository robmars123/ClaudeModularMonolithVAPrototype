using ECommerce.Modules.Customers.Entities;
using ECommerce.Modules.Customers.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Contracts.Customers;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Customers.Features.GetCustomerContact;

/// <summary>Serves the cross-module <see cref="GetCustomerContactRequest"/> contract (used by Orders to address the order-confirmation notification).</summary>
public class GetCustomerContactHandler : IRequestHandler<GetCustomerContactRequest, Result<CustomerContactDto>>
{
    private readonly CustomersDbContext _db;
    public GetCustomerContactHandler(CustomersDbContext db) => _db = db;

    public async Task<Result<CustomerContactDto>> Handle(GetCustomerContactRequest request, CancellationToken ct)
    {
        Customer? customer = await _db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, ct);

        return customer is null
            ? Result<CustomerContactDto>.Failure("Customer not found.")
            : Result<CustomerContactDto>.Success(new CustomerContactDto(customer.Email, customer.FullName));
    }
}
