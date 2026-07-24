using ECommerce.Modules.Customers.Entities;
using ECommerce.Modules.Customers.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Modules.Customers.Features.UpdateShippingAddress;

public class UpdateShippingAddressHandler : IRequestHandler<UpdateShippingAddressCommand, Result>
{
    private readonly CustomersDbContext _db;
    public UpdateShippingAddressHandler(CustomersDbContext db) => _db = db;

    public async Task<Result> Handle(UpdateShippingAddressCommand cmd, CancellationToken ct)
    {
        var customer = await _db.Customers.FirstOrDefaultAsync(c => c.UserId == cmd.UserId, ct);
        if (customer is null) return Result.Failure("Customer not found.");

        customer.UpdateShippingAddress(new Address(cmd.Street, cmd.City, cmd.State, cmd.ZipCode, cmd.Country), cmd.UserId);
        await _db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
