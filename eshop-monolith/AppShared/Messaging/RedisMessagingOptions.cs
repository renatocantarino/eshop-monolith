namespace AppShared.Messaging;

/// <summary>
/// Configuration options for Redis messaging.
/// </summary>
public class RedisMessagingOptions
{
    public const string SectionName = "RedisMessaging";

    /// <summary>
    /// The name of the Redis stream for order events.
    /// </summary>
    public string OrdersStreamName { get; set; } = "orders-stream";

    /// <summary>
    /// The name of the consumer group for order processing.
    /// </summary>
    public string ConsumerGroupName { get; set; } = "ordering-service";

    /// <summary>
    /// The consumer name for this instance.
    /// </summary>
    public string ConsumerName { get; set; } = Environment.MachineName;
}
