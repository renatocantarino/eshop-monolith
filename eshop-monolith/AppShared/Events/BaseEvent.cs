using AppShared.Cqrs.Abstractions;

namespace AppShared.Events;

public record BaseEvent : IEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime CreationDate { get; init; } = DateTime.UtcNow;
}