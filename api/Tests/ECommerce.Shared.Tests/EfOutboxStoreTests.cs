using ECommerce.Shared.Messaging;
using Xunit;

namespace ECommerce.Shared.Tests;

public sealed class EfOutboxStoreTests
{
    [Fact]
    public async Task GetUnprocessed_then_MarkProcessed_removes_it_from_the_pending_batch()
    {
        using SqliteTestDatabase db = new SqliteTestDatabase();
        Guid messageId = Guid.NewGuid();

        await using (TestDbContext context = db.CreateContext())
        {
            context.OutboxMessages.Add(new OutboxMessage { Id = messageId, Type = "X", Kind = MessageKind.PointToPoint, Destination = "q", Payload = "{}" });
            await context.SaveChangesAsync();
        }

        await using (TestDbContext context = db.CreateContext())
        {
            EfOutboxStore<TestDbContext> store = new EfOutboxStore<TestDbContext>(context);
            IReadOnlyList<OutboxMessage> pending = await store.GetUnprocessedAsync(10);
            Assert.Single(pending);

            await store.MarkProcessedAsync(messageId);
        }

        await using (TestDbContext context = db.CreateContext())
        {
            EfOutboxStore<TestDbContext> store = new EfOutboxStore<TestDbContext>(context);
            Assert.Empty(await store.GetUnprocessedAsync(10));
        }
    }

    [Fact]
    public async Task MarkFailed_increments_attempts_and_records_the_error()
    {
        using SqliteTestDatabase db = new SqliteTestDatabase();
        Guid messageId = Guid.NewGuid();

        await using (TestDbContext context = db.CreateContext())
        {
            context.OutboxMessages.Add(new OutboxMessage { Id = messageId, Type = "X", Kind = MessageKind.PointToPoint, Destination = "q", Payload = "{}" });
            await context.SaveChangesAsync();
        }

        await using (TestDbContext context = db.CreateContext())
            await new EfOutboxStore<TestDbContext>(context).MarkFailedAsync(messageId, "broker unreachable");

        await using TestDbContext verify = db.CreateContext();
        OutboxMessage? message = await verify.OutboxMessages.FindAsync(messageId);
        Assert.NotNull(message);
        Assert.Equal(1, message!.Attempts);
        Assert.Equal("broker unreachable", message.Error);
        Assert.Null(message.ProcessedAtUtc);
    }
}
