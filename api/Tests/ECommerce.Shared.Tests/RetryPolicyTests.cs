using ECommerce.Shared.Messaging;
using Xunit;

namespace ECommerce.Shared.Tests;

public sealed class RetryPolicyTests
{
    [Fact]
    public void GetAttempt_defaults_to_zero_when_no_headers()
    {
        Assert.Equal(0, RetryPolicy.GetAttempt(null));
        Assert.Equal(0, RetryPolicy.GetAttempt(new Dictionary<string, object?>()));
    }

    [Fact]
    public void WithIncrementedAttempt_increments_from_the_current_value()
    {
        var headers = RetryPolicy.WithIncrementedAttempt(null);
        Assert.Equal(1, RetryPolicy.GetAttempt(headers));

        var next = RetryPolicy.WithIncrementedAttempt(headers);
        Assert.Equal(2, RetryPolicy.GetAttempt(next));
    }

    [Fact]
    public void WithIncrementedAttempt_preserves_other_headers()
    {
        var original = new Dictionary<string, object?> { ["x-custom"] = "value" };
        var next = RetryPolicy.WithIncrementedAttempt(original);

        Assert.Equal("value", next["x-custom"]);
        Assert.Equal(1, RetryPolicy.GetAttempt(next));
    }

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 4)]
    [InlineData(10, 30)] // capped
    public void BackoffFor_grows_exponentially_up_to_the_cap(int attempt, int expectedSeconds)
    {
        Assert.Equal(TimeSpan.FromSeconds(expectedSeconds), RetryPolicy.BackoffFor(attempt));
    }
}
