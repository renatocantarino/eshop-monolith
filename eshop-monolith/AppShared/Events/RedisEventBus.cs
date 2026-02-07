using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System.Text.Json;

namespace AppShared.Events;

public interface IEventBus
{
    Task PublishAsync<T>(string streamName, T message);
}

public class RedisEventBus : IEventBus
{
    private readonly IDatabase _db;

    public RedisEventBus([FromKeyedServices("cache")] IConnectionMultiplexer redis)
    {
        _db = redis.GetDatabase();
    }

    public async Task PublishAsync<T>(string streamName, T message)
    {
        var json = JsonSerializer.Serialize(message);
        await _db.StreamAddAsync(streamName, "data", json);
    }
}