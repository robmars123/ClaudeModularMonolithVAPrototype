using ECommerce.Shared.Behaviors;
using ECommerce.Shared.Common;
using ECommerce.Shared.Mediator;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ECommerce.Shared.Tests;

public sealed class MediatorTests
{
    private sealed record UnhandledPing : IRequest<Result>;

    private sealed record ValidatedCommand(string? Name) : IRequest<Result<string>>, IValidatableRequest
    {
        public IEnumerable<string> Validate()
        {
            if (string.IsNullOrWhiteSpace(Name)) yield return "Name is required.";
        }
    }

    private sealed class ValidatedCommandHandler : IRequestHandler<ValidatedCommand, Result<string>>
    {
        public bool WasCalled { get; private set; }
        public Task<Result<string>> Handle(ValidatedCommand request, CancellationToken ct)
        {
            WasCalled = true;
            return Task.FromResult(Result<string>.Success(request.Name!));
        }
    }

    private sealed record PingNotification : INotification;

    private sealed class ThrowingHandler : INotificationHandler<PingNotification>
    {
        public Task Handle(PingNotification notification, CancellationToken ct) => throw new InvalidOperationException("handler A failed");
    }

    private sealed class TrackingHandler : INotificationHandler<PingNotification>
    {
        public bool WasCalled { get; private set; }
        public Task Handle(PingNotification notification, CancellationToken ct)
        {
            WasCalled = true;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task Send_throws_a_descriptive_exception_when_no_handler_is_registered()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddMediator();
        IMediator mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        InvalidOperationException ex = await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(new UnhandledPing()));

        Assert.Contains(nameof(UnhandledPing), ex.Message);
    }

    [Fact]
    public async Task Publish_runs_every_handler_and_aggregates_failures_instead_of_short_circuiting()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddMediator();
        services.AddTransient<INotificationHandler<PingNotification>, ThrowingHandler>();
        TrackingHandler tracking = new TrackingHandler();
        services.AddSingleton<INotificationHandler<PingNotification>>(tracking);

        IMediator mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        await Assert.ThrowsAnyAsync<Exception>(() => mediator.Publish(new PingNotification()));

        // The failure of one handler must not have prevented the other from running.
        Assert.True(tracking.WasCalled);
    }

    [Fact]
    public async Task ValidationBehavior_short_circuits_before_the_handler_runs()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddMediator();
        services.AddPipelineBehavior(typeof(ValidationBehavior<,>));
        ValidatedCommandHandler handler = new ValidatedCommandHandler();
        services.AddSingleton<IRequestHandler<ValidatedCommand, Result<string>>>(handler);

        IMediator mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
        Result<string> result = await mediator.Send(new ValidatedCommand(null));

        Assert.False(result.IsSuccess);
        Assert.False(handler.WasCalled);
    }

    [Fact]
    public async Task ValidationBehavior_lets_valid_requests_reach_the_handler()
    {
        ServiceCollection services = new ServiceCollection();
        services.AddMediator();
        services.AddPipelineBehavior(typeof(ValidationBehavior<,>));
        ValidatedCommandHandler handler = new ValidatedCommandHandler();
        services.AddSingleton<IRequestHandler<ValidatedCommand, Result<string>>>(handler);

        IMediator mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
        Result<string> result = await mediator.Send(new ValidatedCommand("Jane"));

        Assert.True(result.IsSuccess);
        Assert.True(handler.WasCalled);
    }
}
