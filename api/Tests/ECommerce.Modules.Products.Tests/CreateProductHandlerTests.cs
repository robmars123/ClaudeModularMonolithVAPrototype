using ECommerce.Modules.Products.Entities;
using ECommerce.Modules.Products.Features.CreateProduct;
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
        await using var context = db.CreateContext();
        var category = Category.Create("Electronics", "Electronic devices", "system");
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
        using var db = new ProductsSqliteTestDatabase();
        var categoryId = await SeedCategoryAsync(db);

        await using var context = db.CreateContext();
        var handler = MakeHandler(context, out var publisher);

        var result = await handler.Handle(
            new CreateProductCommand("Widget", "A widget", "WID-001", 19.99m, categoryId, null, "admin-1"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Electronics", result.Value!.CategoryName);

        await using var verify = db.CreateContext();
        Assert.Single(await verify.Products.ToListAsync());
        Assert.Single(await verify.OutboxMessages.ToListAsync());

        await publisher.Received(1).Publish(Arg.Any<ECommerce.Shared.IntegrationEvents.ProductCreatedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Rejects_duplicate_SKU()
    {
        using var db = new ProductsSqliteTestDatabase();
        var categoryId = await SeedCategoryAsync(db);

        await using (var context = db.CreateContext())
        {
            var first = MakeHandler(context, out _);
            var firstResult = await first.Handle(
                new CreateProductCommand("Widget", "A widget", "WID-001", 19.99m, categoryId, null, "admin-1"), CancellationToken.None);
            Assert.True(firstResult.IsSuccess);
        }

        await using var duplicateContext = db.CreateContext();
        var handler = MakeHandler(duplicateContext, out _);

        var result = await handler.Handle(
            new CreateProductCommand("Widget 2", "Another widget", "WID-001", 24.99m, categoryId, null, "admin-1"), CancellationToken.None);

        Assert.False(result.IsSuccess);

        await using var verify = db.CreateContext();
        Assert.Single(await verify.Products.ToListAsync());
    }

    [Fact]
    public async Task Rejects_unknown_category()
    {
        using var db = new ProductsSqliteTestDatabase();

        await using var context = db.CreateContext();
        var handler = MakeHandler(context, out _);

        var result = await handler.Handle(
            new CreateProductCommand("Widget", "A widget", "WID-001", 19.99m, Guid.NewGuid(), null, "admin-1"), CancellationToken.None);

        Assert.False(result.IsSuccess);

        await using var verify = db.CreateContext();
        Assert.Empty(await verify.Products.ToListAsync());
    }
}
