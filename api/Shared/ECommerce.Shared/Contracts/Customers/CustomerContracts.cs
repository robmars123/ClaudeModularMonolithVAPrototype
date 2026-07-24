using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Shared.Contracts.Customers;

/// <summary>Public API of the Customers module. Handled inside Customers; callable from any module.</summary>
public sealed record GetCustomerShippingAddressRequest(string UserId) : IRequest<Result<CustomerShippingAddressDto>>;

public sealed record CustomerShippingAddressDto(string Street, string City, string State, string ZipCode, string Country)
{
    public override string ToString() => $"{Street}, {City}, {State} {ZipCode}, {Country}";
}
