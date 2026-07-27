using ECommerce.Shared.Messaging;
using Xunit;

namespace ECommerce.Shared.Tests;

public sealed class EfInboxStoreTests
{
    [Fact]
    public async Task Message_is_not_processed_until_marked()
    {
        using SqliteTestDatabase db = new SqliteTestDatabase();
        const string messageId = "msg-1";
        const string consumer = "some.queue";

        await using (TestDbContext context = db.CreateContext())
        {
            EfInboxStore<TestDbContext> inbox = new EfInboxStore<TestDbContext>(context);
            Assert.False(await inbox.IsProcessedAsync(messageId, consumer));

            await inbox.MarkProcessedAsync(messageId, consumer);
        }

        await using TestDbContext verify = db.CreateContext();
        Assert.True(await new EfInboxStore<TestDbContext>(verify).IsProcessedAsync(messageId, consumer));
    }

    [Fact]
    public async Task Same_message_id_is_tracked_independently_per_consumer()
    {
        using SqliteTestDatabase db = new SqliteTestDatabase();
        const string messageId = "msg-shared";

        await using (TestDbContext context = db.CreateContext())
            await new EfInboxStore<TestDbContext>(context).MarkProcessedAsync(messageId, "consumer.a");

        await using TestDbContext verify = db.CreateContext();
        EfInboxStore<TestDbContext> inbox = new EfInboxStore<TestDbContext>(verify);
        Assert.True(await inbox.IsProcessedAsync(messageId, "consumer.a"));
        Assert.False(await inbox.IsProcessedAsync(messageId, "consumer.b"));
    }
}
