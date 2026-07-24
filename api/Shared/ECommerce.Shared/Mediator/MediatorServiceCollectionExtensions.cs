using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ECommerce.Shared.Mediator;

public static class MediatorServiceCollectionExtensions
{
    /// <summary>
    /// Registers the mediator and scans the given assemblies for IRequestHandler / INotificationHandler
    /// implementations. Safe to call once per module — the mediator itself and already-registered
    /// handlers are not duplicated.
    /// </summary>
    public static IServiceCollection AddMediator(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.TryAddScoped<IMediator, Mediator>();
        services.TryAddScoped<ISender>(sp => sp.GetRequiredService<IMediator>());
        services.TryAddScoped<IPublisher>(sp => sp.GetRequiredService<IMediator>());

        foreach (var assembly in assemblies.Distinct())
        {
            foreach (var implementation in assembly.GetTypes())
            {
                if (implementation is not { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false })
                    continue;

                foreach (var service in implementation.GetInterfaces())
                {
                    if (!service.IsGenericType)
                        continue;

                    var definition = service.GetGenericTypeDefinition();
                    if (definition != typeof(IRequestHandler<,>) && definition != typeof(INotificationHandler<>))
                        continue;

                    if (!services.Any(d => d.ServiceType == service && d.ImplementationType == implementation))
                        services.AddTransient(service, implementation);
                }
            }
        }

        return services;
    }

    /// <summary>Registers an open-generic pipeline behavior (e.g. typeof(RequestLoggingBehavior&lt;,&gt;)) exactly once.</summary>
    public static IServiceCollection AddPipelineBehavior(this IServiceCollection services, Type openBehaviorType)
    {
        if (!services.Any(d => d.ServiceType == typeof(IPipelineBehavior<,>) && d.ImplementationType == openBehaviorType))
            services.AddTransient(typeof(IPipelineBehavior<,>), openBehaviorType);

        return services;
    }
}
