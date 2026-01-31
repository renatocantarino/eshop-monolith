using AppShared.Cqrs.Abstractions;
using AppShared.Cqrs.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace AppShared.Cqrs.Internal;

/// <summary>
/// Base class for command wrappers that handle dynamic command execution with return value.
/// This is an internal implementation detail of the mediator pattern.
/// </summary>
/// <typeparam name="TResponse">The type of response from the command.</typeparam>
internal abstract class CommandWrapperWithResponse<TResponse>
{
    /// <summary>
    /// Handles the command execution with pipeline behaviors.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="provider">Service provider for resolving dependencies.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The command result.</returns>
    public abstract Task<TResponse> HandleAsync(object command, IServiceProvider provider, CancellationToken ct);
}

/// <summary>
/// Concrete implementation of command wrapper for a specific command type that returns a value.
/// </summary>
/// <typeparam name="TCommand">The type of command.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
internal class CommandWrapperWithResponseImpl<TCommand, TResponse> : CommandWrapperWithResponse<TResponse>
    where TCommand : ICommand<TResponse>
{
    public override Task<TResponse> HandleAsync(object command, IServiceProvider provider, CancellationToken ct)
    {
        var handler = provider.GetRequiredService<ICommandHandler<TCommand, TResponse>>();
        var behaviors = provider.GetServices<IPipelineBehavior<TCommand, TResponse>>();

        Func<Task<TResponse>> handlerDelegate = () => handler.HandleAsync((TCommand)command, ct);

        return behaviors.Reverse().Aggregate(handlerDelegate, (next, pipeline) =>
            () => pipeline.HandleAsync((TCommand)command, next, ct))();
    }
}
