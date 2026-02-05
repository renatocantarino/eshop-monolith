using MassTransit.SagaStateMachine;

namespace AppShared.Events;

public record BaseEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public string EventTypeName => GetType().AssemblyQualifiedName;
    public DateTime OcurredOn => DateTime.UtcNow;
}