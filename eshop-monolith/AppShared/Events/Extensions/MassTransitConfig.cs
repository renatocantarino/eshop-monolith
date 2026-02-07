using AppShared.Events.RedisProcessor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace AppShared.Events.Extensions;

public static class RedisStreamConfig
{
    public static IServiceCollection AddRedisStreamConsumer(this IServiceCollection services, IConfiguration configuration)
    {
        // Obtém a string de conexão do appsettings ou variáveis de ambiente do Aspire
        var connectionString = configuration.GetConnectionString("cache")
            ?? throw new InvalidOperationException("Connection string 'cache' not found.");

        // Registra explicitamente como KeyedSingleton para bater com o [FromKeyedServices("cache")]
        services.AddKeyedSingleton<IConnectionMultiplexer>("cache", (sp, key) =>
        {
            return ConnectionMultiplexer.Connect(connectionString);
        });

        // Registra também como serviço comum (sem chave) para evitar o erro de 'Unable to resolve'
        // em outras partes do sistema que pedem a interface pura.
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            sp.GetRequiredKeyedService<IConnectionMultiplexer>("cache"));

        // Adiciona o Worker que agora encontrará o IConnectionMultiplexer (chaveado ou não)
        services.AddHostedService<RedisStreamWorker>();

        return services;
    }
}