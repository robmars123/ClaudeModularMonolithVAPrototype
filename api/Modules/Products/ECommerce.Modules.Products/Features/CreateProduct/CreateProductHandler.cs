using ECommerce.Modules.Products.Entities;
using ECommerce.Modules.Products.Features.GetProducts;
using ECommerce.Modules.Products.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.IntegrationEvents;
using ECommerce.Shared.Mediator;
using ECommerce.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ECommerce.Modules.Products.Features.CreateProduct;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, Result<ProductResponse>>
{
    private readonly ProductsDbContext _db;
    private readonly IPublisher _publisher;
    private readonly IIntegrationEventPublisher _integrationEvents;
    private readonly ILogger<CreateProductHandler> _logger;

    public CreateProductHandler(ProductsDbContext db, IPublisher publisher, [FromKeyedServices(ProductsModule.PublisherKey)] IIntegrationEventPublisher integrationEvents, ILogger<CreateProductHandler> logger)
    {
        _db = db; _publisher = publisher; _integrationEvents = integrationEvents; _logger = logger;
    }

    public async Task<Result<ProductResponse>> Handle(CreateProductCommand cmd, CancellationToken ct)
    {
        if (await _db.Products.AnyAsync(p => p.SKU == cmd.SKU, ct))
            return Result<ProductResponse>.Failure($"SKU '{cmd.SKU}' already exists.");

        if (!await _db.Categories.AnyAsync(c => c.Id == cmd.CategoryId && c.IsActive, ct))
            return Result<ProductResponse>.Failure("Category not found.");

        Product product = Product.Create(cmd.Name, cmd.Description, cmd.SKU, cmd.Price, cmd.CategoryId, cmd.ImageUrl, cmd.CreatedBy);
        _db.Products.Add(product);

        ProductCreatedEvent createdEvent = new ProductCreatedEvent(product.Id, product.SKU, product.Name, cmd.CreatedBy);

        // Pub/sub: broadcast on the topic exchange — every bound subscriber queue gets its own copy.
        // Enqueued into the same change tracker as the product, so it commits atomically with it below.
        _integrationEvents.Enqueue(createdEvent, MessageKind.PubSub, "product.created");

        await _db.SaveChangesAsync(ct);

        // Notify other modules (Inventory creates the stock record for this product). The product is
        // already saved, so a failure here is logged for reconciliation instead of failing the request.
        await _publisher.PublishBestEffort(createdEvent, _logger, $"Product {product.SKU} created but notifying Inventory failed", ct);

        Category? cat = await _db.Categories.FindAsync([cmd.CategoryId], ct);
        return Result<ProductResponse>.Success(new ProductResponse(product.Id, product.Name, product.Description, product.SKU, product.Price, product.ImageUrl, product.IsActive, product.CategoryId, cat!.Name, product.CreatedAt, product.UpdatedAt));
    }
}
