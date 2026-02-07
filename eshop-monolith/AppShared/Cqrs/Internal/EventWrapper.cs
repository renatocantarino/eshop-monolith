using AppShared.Cqrs.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace AppShared.Cqrs.Internal;

internal abstract class EventWrapper
{
    public abstract Task HandleAsync(object @event, IServiceProvider serviceProvider, CancellationToken ct);
}

internal class EventWrapperImpl<T> : EventWrapper where T : IEvent
{
    public override async Task HandleAsync(object @event, IServiceProvider serviceProvider, CancellationToken ct)
    {
        // Resolve todos os handlers registrados para este evento específico
        var handlers = serviceProvider.GetServices<IEventHandler<T>>();

        var tasks = handlers.Select(handler => handler.HandleAsync((T)@event, ct));

        // Executa todos os handlers em paralelo
        await Task.WhenAll(tasks);
    }
}