using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Modules.Customers.Features.GetAllCustomers;

public record GetAllCustomersQuery(int Page, int PageSize) : IRequest<Result<PagedResult<CustomerListResponse>>>;

public record CustomerListResponse(
    Guid Id, string UserId, string FirstName, string LastName,
    string FullName, string Email, string? Phone, object? ShippingAddress, DateTime CreatedAt);
