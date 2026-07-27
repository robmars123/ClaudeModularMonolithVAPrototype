using ECommerce.Modules.Customers.Entities;
using ECommerce.Modules.Customers.Persistence;
using ECommerce.Shared.IntegrationEvents;
using ECommerce.Shared.Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Customers.Integration;

/// <summary>Creates the customer profile as soon as a user registers, so profile endpoints work immediately.</summary>
public class UserRegisteredHandler : INotificationHandler<UserRegisteredEvent>
{
    private readonly CustomersDbContext _db;
    private readonly ILogger<UserRegisteredHandler> _logger;

    public UserRegisteredHandler(CustomersDbContext db, ILogger<UserRegisteredHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Handle(UserRegisteredEvent notification, CancellationToken ct)
    {
        if (await _db.Customers.AnyAsync(c => c.UserId == notification.UserId, ct))
            return;

        Customer customer = Customer.Create(notification.UserId, notification.FirstName, notification.LastName, notification.Email);
        _db.Customers.Add(customer);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation("Created customer profile for user {UserId}", notification.UserId);
    }
}
