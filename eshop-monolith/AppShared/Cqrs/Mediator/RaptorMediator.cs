using AppShared.Cqrs.Abstractions;
using AppShared.Cqrs.Internal;
using System.Collections.Concurrent;

namespace AppShared.Cqrs.Mediator;

public class RaptorMediator(IServiceProvider serviceProvider) : IMediator
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private static readonly ConcurrentDictionary<Type, object> _cache = new();

    public async Task<TResponse> ExecuteQueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken ct = default)
    {
        var queryType = query.GetType();
        var wrapper = (QueryWrapper<TResponse>)_cache.GetOrAdd(queryType, t =>
        {
            var wrapperType = typeof(QueryWrapperImpl<,>).MakeGenericType(t, typeof(TResponse));
            return Activator.CreateInstance(wrapperType)!;
        });

        return await wrapper.HandleAsync(query, _serviceProvider, ct);
    }

    public async Task ExecuteCommandAsync(ICommand command, CancellationToken ct = default)
    {
        var commandType = command.GetType();
        var wrapper = (CommandWrapper)_cache.GetOrAdd(commandType, t =>
        {
            var wrapperType = typeof(CommandWrapperImpl<>).MakeGenericType(t);
            return Activator.CreateInstance(wrapperType)!;
        });

        await wrapper.HandleAsync(command, _serviceProvider, ct);
    }

    public async Task<TResponse> ExecuteCommandAsync<TResponse>(ICommand<TResponse> command, CancellationToken ct = default)
    {
        var commandType = command.GetType();
        var wrapper = (CommandWrapperWithResponse<TResponse>)_cache.GetOrAdd(commandType, t =>
        {
            var wrapperType = typeof(CommandWrapperWithResponseImpl<,>).MakeGenericType(t, typeof(TResponse));
            return Activator.CreateInstance(wrapperType)!;
        });

        return await wrapper.HandleAsync(command, _serviceProvider, ct);
    }

    public async Task PublishAsync<T>(T @event, CancellationToken ct) where T : IEvent
    {
        var eventType = @event.GetType();
        var wrapper = (EventWrapper)_cache.GetOrAdd(eventType, t =>
        {
            var wrapperType = typeof(EventWrapperImpl<>).MakeGenericType(t);
            return Activator.CreateInstance(wrapperType)!;
        });

        await wrapper.HandleAsync(@event, _serviceProvider, ct);
    }
}