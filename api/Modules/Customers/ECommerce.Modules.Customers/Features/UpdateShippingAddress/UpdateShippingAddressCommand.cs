using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Customers.Features.UpdateShippingAddress;

public record UpdateShippingAddressCommand(string UserId, string Street, string City, string State, string ZipCode, string Country)
    : IRequest<Result>;
