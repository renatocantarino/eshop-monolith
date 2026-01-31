namespace AppShared.Cqrs.Pipeline;

/// <summary>
/// Intercepts the execution of a request to add cross-cutting concerns such as logging, validation, authentication, etc.
/// Behaviors are executed in the order they are registered, forming a pipeline around the actual handler.
/// </summary>
/// <typeparam name="TRequest">The type of request being handled.</typeparam>
/// <typeparam name="TResponse">The type of response from the handler.</typeparam>
public interface IPipelineBehavior<in TRequest, TResponse>
{
    /// <summary>
    /// Handles the request by performing operations before and/or after calling the next behavior or handler.
    /// </summary>
    /// <param name="request">The request being handled.</param>
    /// <param name="next">The next behavior or handler in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response from the pipeline.</returns>
    Task<TResponse> HandleAsync(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken = default);
}
