using AppShared.Cqrs.Mediator;
using AppShared.Cqrs.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace AppShared.Cqrs.Extensions;

/// <summary>
/// Extension methods for configuring CQRS services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Raptor Mediator and its default pipeline behaviors to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRaptorMediator(this IServiceCollection services)
    {
        services.AddScoped<IMediator, RaptorMediator>();
        services.AddRaptorPipelineBehaviors();
        return services;
    }

    /// <summary>
    /// Adds the default pipeline behaviors (Logging and Metrics) to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRaptorPipelineBehaviors(this IServiceCollection services)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
