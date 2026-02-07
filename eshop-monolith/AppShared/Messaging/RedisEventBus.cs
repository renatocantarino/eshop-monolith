using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace AppShared.Messaging;

/// <summary>
/// Redis Streams-based implementation of the Event Bus.
/// </summary>
public class RedisEventBus : IEventBus
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisEventBus> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisEventBus(
        IConnectionMultiplexer redis,
        ILogger<RedisEventBus> logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<string> PublishAsync<T>(
        string streamName, 
        T @event, 
        CancellationToken cancellationToken = default) where T : class
    {
        if (string.IsNullOrWhiteSpace(streamName))
        {
            throw new ArgumentException("Stream name cannot be null or empty.", nameof(streamName));
        }

        if (@event == null)
        {
            throw new ArgumentNullException(nameof(@event));
        }

        try
        {
            var database = _redis.GetDatabase();
            
            // Serialize the event to JSON
            var eventJson = JsonSerializer.Serialize(@event, _jsonOptions);
            var eventType = typeof(T).Name;

            // Create stream entry with metadata
            var streamEntries = new NameValueEntry[]
            {
                new NameValueEntry("eventType", eventType),
                new NameValueEntry("data", eventJson),
                new NameValueEntry("timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds())
            };

            // Add to Redis Stream
            var messageId = await database.StreamAddAsync(
                streamName, 
                streamEntries,
                flags: CommandFlags.None);

            _logger.LogInformation(
                "Published event {EventType} to stream {StreamName} with ID {MessageId}",
                eventType,
                streamName,
                messageId);

            return messageId.ToString();
        }
        catch (RedisException ex)
        {
            _logger.LogError(
                ex,
                "Redis error while publishing event {EventType} to stream {StreamName}",
                typeof(T).Name,
                streamName);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Failed to serialize event {EventType} to JSON",
                typeof(T).Name);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error while publishing event {EventType} to stream {StreamName}",
                typeof(T).Name,
                streamName);
            throw;
        }
    }
}
