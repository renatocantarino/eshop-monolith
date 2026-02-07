using AppShared.Cqrs.Abstractions;

namespace AppShared.Cqrs.Mediator;

/// <summary>
/// Mediator interface for executing commands and queries following the CQRS pattern.
/// Provides a simplified API that automatically infers types from the request objects.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Executes a query and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The type of response from the query.</typeparam>
    /// <param name="query">The query to execute.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The query result.</returns>
    Task<TResponse> ExecuteQueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken ct = default);

    /// <summary>
    /// Executes a command that doesn't return a value.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ExecuteCommandAsync(ICommand command, CancellationToken ct = default);

    /// <summary>
    /// Executes a command and returns the result.
    /// </summary>
    /// <typeparam name="TResponse">The type of response from the command.</typeparam>
    /// <param name="command">The command to execute.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The command result.</returns>
    Task<TResponse> ExecuteCommandAsync<TResponse>(ICommand<TResponse> command, CancellationToken ct = default);

    Task PublishAsync<T>(T @event, CancellationToken ct) where T : IEvent;
}