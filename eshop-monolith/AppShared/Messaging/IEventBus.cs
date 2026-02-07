namespace AppShared.Messaging;

/// <summary>
/// Defines the contract for publishing integration events to a message bus.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Publishes an event to the specified stream.
    /// </summary>
    /// <typeparam name="T">The type of event to publish</typeparam>
    /// <param name="streamName">The name of the stream to publish to</param>
    /// <param name="event">The event data to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The message ID assigned by Redis</returns>
    Task<string> PublishAsync<T>(string streamName, T @event, CancellationToken cancellationToken = default) where T : class;
}
