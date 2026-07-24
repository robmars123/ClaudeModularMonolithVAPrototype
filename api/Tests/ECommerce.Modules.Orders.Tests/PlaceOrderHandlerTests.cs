using ECommerce.Modules.Orders.Entities;
using ECommerce.Modules.Orders.Features.PlaceOrder;
using ECommerce.Modules.Orders.Persistence;
using ECommerce.Shared.Common;
using ECommerce.Shared.Contracts.Inventory;
using ECommerce.Shared.IntegrationEvents;
using ECommerce.Shared.Mediator;
using ECommerce.Shared.Messaging;
using ECommerce.Shared.Observability;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace ECommerce.Modules.Orders.Tests;

public sealed class PlaceOrderHandlerTests
{
    private const string UserId = "user-1";

    /// <summary>Throws instead of persisting, so the compensating-release path can be exercised deterministically.</summary>
    private sealed class ThrowingOrdersDbContext(DbContextOptions<OrdersDbContext> options) : OrdersDbContext(options)
    {
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            throw new DbUpdateException("simulated save failure");
    }

    private static async Task<Guid> SeedActiveCartAsync(OrdersDbContext db, Guid productId)
    {
        var cart = Cart.Create(UserId);
        cart.AddItem(productId, "Widget", 10m, 2);
        db.Carts.Add(cart);
        await db.SaveChangesAsync();
        return cart.Id;
    }

    private static IMediator MakeMediator(bool stockAvailable = true, bool reservationSucceeds = true)
    {
        var mediator = Substitute.For<IMediator>();

        mediator.Send(Arg.Any<IRequest<Result<int>>>(), Arg.Any<CancellationToken>())
            .Returns(Result<int>.Success(stockAvailable ? 100 : 0));

        mediator.Send(Arg.Any<IRequest<Result>>(), Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.ArgAt<IRequest<Result>>(0) switch
            {
                ReserveStockRequest => reservationSucceeds ? Result.Success() : Result.Failure("out of stock"),
                ReleaseStockRequest => Result.Success(),
                _ => Result.Failure("unexpected request")
            });

        return mediator;
    }

    [Fact]
    public async Task Successful_order_persists_the_order_and_the_outbox_row_atomically()
    {
        using var db = new OrdersSqliteTestDatabase();
        var productId = Guid.NewGuid();

        await using (var setup = db.CreateContext())
            await SeedActiveCartAsync(setup, productId);

        await using var context = db.CreateContext();
        var mediator = MakeMediator();
        var integrationEvents = new EfOutboxIntegrationEventPublisher<OrdersDbContext>(context, Substitute.For<ICorrelationIdAccessor>());
        var handler = new PlaceOrderHandler(context, mediator, integrationEvents, Substitute.For<ILogger<PlaceOrderHandler>>());

        var result = await handler.Handle(new PlaceOrderCommand(UserId, "123 Main St", null), CancellationToken.None);

        Assert.True(result.IsSuccess);

        await using var verify = db.CreateContext();
        Assert.Single(await verify.Orders.ToListAsync());
        Assert.Single(await verify.OutboxMessages.ToListAsync());

        var cart = await verify.Carts.Include(c => c.Items).SingleAsync();
        Assert.Empty(cart.Items);

        await mediator.Received(1).Send(Arg.Any<ReserveStockRequest>(), Arg.Any<CancellationToken>());
        await mediator.Received(1).Publish(Arg.Any<OrderPlacedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Insufficient_stock_fails_without_writing_anything()
    {
        using var db = new OrdersSqliteTestDatabase();
        var productId = Guid.NewGuid();

        await using (var setup = db.CreateContext())
            await SeedActiveCartAsync(setup, productId);

        await using var context = db.CreateContext();
        var mediator = MakeMediator(stockAvailable: false);
        var integrationEvents = new EfOutboxIntegrationEventPublisher<OrdersDbContext>(context, Substitute.For<ICorrelationIdAccessor>());
        var handler = new PlaceOrderHandler(context, mediator, integrationEvents, Substitute.For<ILogger<PlaceOrderHandler>>());

        var result = await handler.Handle(new PlaceOrderCommand(UserId, "123 Main St", null), CancellationToken.None);

        Assert.False(result.IsSuccess);

        await using var verify = db.CreateContext();
        Assert.Empty(await verify.Orders.ToListAsync());
        Assert.Empty(await verify.OutboxMessages.ToListAsync());

        var cart = await verify.Carts.Include(c => c.Items).SingleAsync();
        Assert.Single(cart.Items); // never cleared
    }

    [Fact]
    public async Task Failed_save_triggers_the_compensating_stock_release()
    {
        using var db = new OrdersSqliteTestDatabase();
        var productId = Guid.NewGuid();

        await using (var setup = db.CreateContext())
            await SeedActiveCartAsync(setup, productId);

        // Re-open a context against the same in-memory database, but of the throwing subclass.
        await using var throwingContext = CreateThrowingContext(db);
        var mediator = MakeMediator();
        var integrationEvents = new EfOutboxIntegrationEventPublisher<OrdersDbContext>(throwingContext, Substitute.For<ICorrelationIdAccessor>());
        var handler = new PlaceOrderHandler(throwingContext, mediator, integrationEvents, Substitute.For<ILogger<PlaceOrderHandler>>());

        await Assert.ThrowsAsync<DbUpdateException>(() =>
            handler.Handle(new PlaceOrderCommand(UserId, "123 Main St", null), CancellationToken.None));

        await mediator.Received(1).Send(Arg.Any<ReleaseStockRequest>(), Arg.Any<CancellationToken>());
    }

    private static ThrowingOrdersDbContext CreateThrowingContext(OrdersSqliteTestDatabase db)
    {
        using var probe = db.CreateContext();
        var options = new DbContextOptionsBuilder<OrdersDbContext>().UseSqlite(probe.Database.GetDbConnection()).Options;
        return new ThrowingOrdersDbContext(options);
    }
}
