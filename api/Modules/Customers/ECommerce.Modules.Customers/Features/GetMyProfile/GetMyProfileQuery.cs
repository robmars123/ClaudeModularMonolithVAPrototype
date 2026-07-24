using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Customers.Features.GetMyProfile;

public record GetMyProfileQuery(string UserId) : IRequest<Result<CustomerResponse>>;

public record CustomerResponse(
    Guid Id, string UserId, string FirstName, string LastName,
    string FullName, string Email, string? Phone,
    AddressResponse? ShippingAddress, DateTime CreatedAt);

public record AddressResponse(string Street, string City, string State, string ZipCode, string Country);
