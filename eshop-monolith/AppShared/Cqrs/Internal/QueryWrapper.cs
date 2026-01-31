using AppShared.Cqrs.Abstractions;
using AppShared.Cqrs.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace AppShared.Cqrs.Internal;

/// <summary>
/// Base class for query wrappers that handle dynamic query execution.
/// This is an internal implementation detail of the mediator pattern.
/// </summary>
/// <typeparam name="TResponse">The type of response from the query.</typeparam>
internal abstract class QueryWrapper<TResponse>
{
    /// <summary>
    /// Handles the query execution with pipeline behaviors.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <param name="provider">Service provider for resolving dependencies.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The query result.</returns>
    public abstract Task<TResponse> HandleAsync(object query, IServiceProvider provider, CancellationToken ct);
}

/// <summary>
/// Concrete implementation of query wrapper for a specific query type.
/// </summary>
/// <typeparam name="TQuery">The type of query.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
internal class QueryWrapperImpl<TQuery, TResponse> : QueryWrapper<TResponse>
    where TQuery : IQuery<TResponse>
{
    public override Task<TResponse> HandleAsync(object query, IServiceProvider provider, CancellationToken ct)
    {
        var handler = provider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
        var behaviors = provider.GetServices<IPipelineBehavior<TQuery, TResponse>>();

        Func<Task<TResponse>> handlerDelegate = () => handler.HandleAsync((TQuery)query, ct);

        return behaviors.Reverse().Aggregate(handlerDelegate, (next, pipeline) =>
            () => pipeline.HandleAsync((TQuery)query, next, ct))();
    }
}
