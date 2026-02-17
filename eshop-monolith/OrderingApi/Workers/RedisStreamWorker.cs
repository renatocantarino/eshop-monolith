using AppShared.IntegrationEvents;
using AppShared.Messaging;
using Microsoft.Extensions.Options;
using OrderingApi.Application;
using StackExchange.Redis;
using System.Text.Json;

namespace OrderingApi.Workers;

/// <summary>
/// Background service that consumes order events from Redis Streams.
/// </summary>
public class RedisStreamWorker : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly RedisMessagingOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RedisStreamWorker> _logger;
    private const int MaxRetryAttempts = 3;
    private const int RetryDelayMs = 1000;

    public RedisStreamWorker(
        IConnectionMultiplexer redis,
        IOptions<RedisMessagingOptions> options,
        IServiceProvider serviceProvider,
        ILogger<RedisStreamWorker> logger)
    {
        _redis = redis ?? throw new ArgumentNullException(nameof(redis));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Redis Stream Worker starting...");

        var database = _redis.GetDatabase();
        var streamName = _options.OrdersStreamName;
        var groupName = _options.ConsumerGroupName;
        var consumerName = _options.ConsumerName;

        // Ensure the consumer group exists
        await EnsureConsumerGroupExistsAsync(database, streamName, groupName, stoppingToken);

        _logger.LogInformation(
            "Listening for messages on stream {StreamName} with consumer group {GroupName}",
            streamName,
            groupName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Read messages from the stream
                var entries = await database.StreamReadGroupAsync(
                    key: streamName,
                    groupName: groupName,
                    consumerName: consumerName,
                    position: ">", // Read only new messages
                    count: 10, // Process up to 10 messages at a time
                    noAck: false);

                if (entries.Length == 0)
                {
                    // No new messages, wait a bit before polling again
                    await Task.Delay(100, stoppingToken);
                    continue;
                }

                foreach (var entry in entries)
                {
                    await ProcessMessageAsync(database, streamName, groupName, entry, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Redis Stream Worker is stopping due to cancellation");
                break;
            }
            catch (RedisException ex)
            {
                _logger.LogError(ex, "Redis error occurred while reading stream");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in Redis Stream Worker");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }

        _logger.LogInformation("Redis Stream Worker stopped");
    }

    private async Task EnsureConsumerGroupExistsAsync(
        IDatabase database,
        string streamName,
        string groupName,
        CancellationToken cancellationToken)
    {
        try
        {
            // Try to create the consumer group
            // StreamPosition.NewMessages ("$") means start from new messages
            await database.StreamCreateConsumerGroupAsync(
                streamName,
                groupName,
                StreamPosition.NewMessages,
                createStream: true);

            _logger.LogInformation(
                "Created consumer group {GroupName} on stream {StreamName}",
                groupName,
                streamName);
        }
        catch (RedisServerException ex) when (ex.Message.Contains("BUSYGROUP"))
        {
            // Consumer group already exists, which is fine
            _logger.LogInformation(
                "Consumer group {GroupName} already exists on stream {StreamName}",
                groupName,
                streamName);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create consumer group {GroupName} on stream {StreamName}",
                groupName,
                streamName);
            throw;
        }
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private async Task ProcessMessageAsync(
        IDatabase database,
        string streamName,
        string groupName,
        StreamEntry entry,
        CancellationToken cancellationToken)
    {
        var messageId = entry.Id;

        try
        {
            _logger.LogInformation("Processing message {MessageId} from stream {StreamName}", messageId, streamName);

            // Extract event data from the stream entry
            var eventTypeValue = entry.Values.FirstOrDefault(v => v.Name == "eventType").Value;
            var dataValue = entry.Values.FirstOrDefault(v => v.Name == "data").Value;

            if (dataValue.IsNullOrEmpty)
            {
                _logger.LogWarning("Message {MessageId} has no data, skipping", messageId);
                await AcknowledgeMessageAsync(database, streamName, groupName, messageId);
                return;
            }

            var eventType = eventTypeValue.ToString();
            var eventData = dataValue.ToString();

            switch (eventType)
            {
                case nameof(OrderCreatedEvent):
                    if (JsonSerializer.Deserialize<OrderCreatedEvent>(eventData!, _jsonOptions) is { } orderEvent)
                    {
                        await ProcessOrderCreatedEventAsync(orderEvent, cancellationToken);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize OrderCreatedEvent. MessageId: {MessageId}", messageId);
                    }
                    break;

                default:
                    _logger.LogInformation("Event type {EventType} ignored.", eventType);
                    break;
            }

            // Acknowledge the message after successful processing
            await AcknowledgeMessageAsync(database, streamName, groupName, messageId);

            _logger.LogInformation("Successfully processed and acknowledged message {MessageId}", messageId);
        }
        catch (JsonException ex)
        {
            _logger.LogError(
                ex,
                "Failed to deserialize message {MessageId}. Message will be acknowledged to prevent reprocessing",
                messageId);

            // Acknowledge malformed messages to prevent infinite retries
            await AcknowledgeMessageAsync(database, streamName, groupName, messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing message {MessageId}. Message will remain in pending list for retry",
                messageId);

            // Don't acknowledge - message will be retried
            // In production, you might want to implement a dead letter queue after max retries
        }
    }

    private async Task ProcessOrderCreatedEventAsync(OrderCreatedEvent orderEvent, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing order for customer {CustomerId}, basket {BasketId}, total {TotalPrice}",
            orderEvent.CustomerId,
            orderEvent.BasketId,
            orderEvent.TotalPrice);

        // Use a scope to get scoped services like DbContext
        using var scope = _serviceProvider.CreateScope();
        var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();

        try
        {
            await orderService.CreateOrderFromEventAsync(orderEvent, cancellationToken);

            _logger.LogInformation(
                "Successfully created order for customer {CustomerId}",
                orderEvent.CustomerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to create order for customer {CustomerId}",
                orderEvent.CustomerId);
            throw; // Re-throw to prevent acknowledgment
        }
    }

    private async Task AcknowledgeMessageAsync(
        IDatabase database,
        string streamName,
        string groupName,
        RedisValue messageId)
    {
        try
        {
            await database.StreamAcknowledgeAsync(streamName, groupName, messageId);
            _logger.LogDebug("Acknowledged message {MessageId}", messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to acknowledge message {MessageId}", messageId);
            // Don't throw - we don't want to fail the entire processing loop
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Redis Stream Worker is stopping gracefully");
        await base.StopAsync(cancellationToken);
    }
}