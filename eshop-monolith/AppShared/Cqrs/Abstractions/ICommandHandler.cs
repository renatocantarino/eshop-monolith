namespace AppShared.Cqrs.Abstractions;

/// <summary>
/// Handler for commands that don't return a value.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Handles the command asynchronously.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="ct">Cancellation token.</param>
    Task HandleAsync(TCommand command, CancellationToken ct = default);
}

/// <summary>
/// Handler for commands that return a value.
/// </summary>
/// <typeparam name="TCommand">The type of command to handle.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Handles the command asynchronously and returns a response.
    /// </summary>
    /// <param name="command">The command to handle.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The response from handling the command.</returns>
    Task<TResponse> HandleAsync(TCommand command, CancellationToken ct = default);
}
