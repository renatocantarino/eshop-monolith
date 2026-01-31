using AppShared.Cqrs.Abstractions;
using AppShared.Cqrs.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace AppShared.Cqrs.Internal;

/// <summary>
/// Base class for command wrappers that handle dynamic command execution without return value.
/// This is an internal implementation detail of the mediator pattern.
/// </summary>
internal abstract class CommandWrapper
{
    /// <summary>
    /// Handles the command execution with pipeline behaviors.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="provider">Service provider for resolving dependencies.</param>
    /// <param name="ct">Cancellation token.</param>
    public abstract Task HandleAsync(object command, IServiceProvider provider, CancellationToken ct);
}

/// <summary>
/// Concrete implementation of command wrapper for a specific command type that doesn't return a value.
/// </summary>
/// <typeparam name="TCommand">The type of command.</typeparam>
internal class CommandWrapperImpl<TCommand> : CommandWrapper
    where TCommand : ICommand
{
    public override async Task HandleAsync(object command, IServiceProvider provider, CancellationToken ct)
    {
        var handler = provider.GetRequiredService<ICommandHandler<TCommand>>();
        // Use 'Unit' to satisfy the IPipelineBehavior<TRequest, TResponse> interface
        var behaviors = provider.GetServices<IPipelineBehavior<TCommand, Unit>>();

        Func<Task<Unit>> handlerDelegate = async () =>
        {
            await handler.HandleAsync((TCommand)command, ct);
            return Unit.Value;
        };

        await behaviors.Reverse().Aggregate(handlerDelegate, (next, pipeline) =>
            () => pipeline.HandleAsync((TCommand)command, next, ct))();
    }
}
