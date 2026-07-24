using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;

namespace ECommerce.Shared.Contracts.Products;

/// <summary>Public API of the Products module. Handled inside Products; callable from any module.</summary>
public sealed record GetProductPricingRequest(Guid ProductId) : IRequest<Result<ProductPricingDto>>;

public sealed record ProductPricingDto(Guid ProductId, string Name, string Sku, decimal Price, bool IsActive);
