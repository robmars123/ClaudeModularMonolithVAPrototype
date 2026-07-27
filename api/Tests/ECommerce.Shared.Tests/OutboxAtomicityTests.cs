using ECommerce.Shared.Mediator;
using ECommerce.Shared.Messaging;
using ECommerce.Shared.Observability;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace ECommerce.Shared.Tests;

public sealed record TestEvent(string Message) : INotification;

public sealed class OutboxAtomicityTests
{
    [Fact]
    public async Task Enqueue_and_aggregate_commit_together_on_success()
    {
        using SqliteTestDatabase db = new SqliteTestDatabase();
        ICorrelationIdAccessor correlation = Substitute.For<ICorrelationIdAccessor>();
        correlation.CorrelationId.Returns("corr-1");

        await using (TestDbContext context = db.CreateContext())
        {
            EfOutboxIntegrationEventPublisher<TestDbContext> publisher = new EfOutboxIntegrationEventPublisher<TestDbContext>(context, correlation);
            context.Aggregates.Add(new TestAggregate { Id = Guid.NewGuid(), Name = "widget" });
            publisher.Enqueue(new TestEvent("hello"), MessageKind.PointToPoint, "some.queue");

            await context.SaveChangesAsync();
        }

        await using TestDbContext verify = db.CreateContext();
        Assert.Equal(1, await verify.Aggregates.CountAsync());
        OutboxMessage outboxMessage = Assert.Single(await verify.OutboxMessages.ToListAsync());
        Assert.Equal("TestEvent", outboxMessage.Type);
        Assert.Equal("corr-1", outboxMessage.CorrelationId);
    }

    [Fact]
    public async Task Failed_SaveChanges_rolls_back_both_aggregate_and_outbox_row()
    {
        using SqliteTestDatabase db = new SqliteTestDatabase();
        ICorrelationIdAccessor correlation = Substitute.For<ICorrelationIdAccessor>();

        // Seed one row so the second insert below violates the unique index on Name.
        await using (TestDbContext seed = db.CreateContext())
        {
            seed.Aggregates.Add(new TestAggregate { Id = Guid.NewGuid(), Name = "duplicate" });
            await seed.SaveChangesAsync();
        }

        await using (TestDbContext context = db.CreateContext())
        {
            EfOutboxIntegrationEventPublisher<TestDbContext> publisher = new EfOutboxIntegrationEventPublisher<TestDbContext>(context, correlation);
            context.Aggregates.Add(new TestAggregate { Id = Guid.NewGuid(), Name = "duplicate" });
            publisher.Enqueue(new TestEvent("should not persist"), MessageKind.PointToPoint, "some.queue");

            await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());
        }

        await using TestDbContext verify = db.CreateContext();
        Assert.Equal(1, await verify.Aggregates.CountAsync()); // only the seeded row
        Assert.Empty(await verify.OutboxMessages.ToListAsync()); // the outbox row never committed either
    }
}
