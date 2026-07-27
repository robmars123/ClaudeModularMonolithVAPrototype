using ECommerce.Modules.Products.Entities;
using ECommerce.Modules.Products.Features.CreateProduct;
using ECommerce.Modules.Products.Features.GetProducts;
using ECommerce.Modules.Products.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using ECommerce.Shared.Messaging;
using ECommerce.Shared.Observability;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace ECommerce.Modules.Products.Tests;

public sealed class CreateProductHandlerTests
{
    private static async Task<Guid> SeedCategoryAsync(ProductsSqliteTestDatabase db)
    {
        await using ProductsDbContext context = db.CreateContext();
        Category category = Category.Create("Electronics", "Electronic devices", "system");
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return category.Id;
    }

    private static CreateProductHandler MakeHandler(Persistence.ProductsDbContext context, out IPublisher publisher) =>
        new(context, publisher = Substitute.For<IPublisher>(),
            new EfOutboxIntegrationEventPublisher<Persistence.ProductsDbContext>(context, Substitute.For<ICorrelationIdAccessor>()),
            Substitute.For<ILogger<CreateProductHandler>>());

    [Fact]
    public async Task Creates_product_and_enqueues_outbox_row_atomically()
    {
        using ProductsSqliteTestDatabase db = new ProductsSqliteTestDatabase();
        Guid categoryId = await SeedCategoryAsync(db);

        await using ProductsDbContext context = db.CreateContext();
        CreateProductHandler handler = MakeHandler(context, out IPublisher? publisher);

        Result<ProductResponse> result = await handler.Handle(
            new CreateProductCommand("Widget", "A widget", "WID-001", 19.99m, categoryId, null, "admin-1"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Electronics", result.Value!.CategoryName);

        await using ProductsDbContext verify = db.CreateContext();
        Assert.Single(await verify.Products.ToListAsync());
        Assert.Single(await verify.OutboxMessages.ToListAsync());

        await publisher.Received(1).Publish(Arg.Any<ECommerce.Shared.IntegrationEvents.ProductCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Rejects_duplicate_SKU()
    {
        using ProductsSqliteTestDatabase db = new ProductsSqliteTestDatabase();
        Guid categoryId = await SeedCategoryAsync(db);

        await using (ProductsDbContext context = db.CreateContext())
        {
            CreateProductHandler first = MakeHandler(context, out _);
            Result<ProductResponse> firstResult = await first.Handle(
                new CreateProductCommand("Widget", "A widget", "WID-001", 19.99m, categoryId, null, "admin-1"), CancellationToken.None);
            Assert.True(firstResult.IsSuccess);
        }

        await using ProductsDbContext duplicateContext = db.CreateContext();
        CreateProductHandler handler = MakeHandler(duplicateContext, out _);

        Result<ProductResponse> result = await handler.Handle(
            new CreateProductCommand("Widget 2", "Another widget", "WID-001", 24.99m, categoryId, null, "admin-1"), CancellationToken.None);

        Assert.False(result.IsSuccess);

        await using ProductsDbContext verify = db.CreateContext();
        Assert.Single(await verify.Products.ToListAsync());
    }

    [Fact]
    public async Task Rejects_unknown_category()
    {
        using ProductsSqliteTestDatabase db = new ProductsSqliteTestDatabase();

        await using ProductsDbContext context = db.CreateContext();
        CreateProductHandler handler = MakeHandler(context, out _);

        Result<ProductResponse> result = await handler.Handle(
            new CreateProductCommand("Widget", "A widget", "WID-001", 19.99m, Guid.NewGuid(), null, "admin-1"), CancellationToken.None);

        Assert.False(result.IsSuccess);

        await using ProductsDbContext verify = db.CreateContext();
        Assert.Empty(await verify.Products.ToListAsync());
    }
}
