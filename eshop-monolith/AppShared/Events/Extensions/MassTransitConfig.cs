using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace AppShared.Events.Extensions;

public static class MassTransitConfig
{
    public static IServiceCollection AddMassTransitConfig(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();
            config.SetInMemorySagaRepositoryProvider();
            config.AddConsumers(assemblies);
            config.AddActivities(assemblies);

            config.UsingRabbitMq((context, cfg) =>
            {
                var configuration = context.GetRequiredService<IConfiguration>();
                var connectionString = configuration.GetConnectionString("rabbitmq");
                cfg.Host(connectionString);
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}