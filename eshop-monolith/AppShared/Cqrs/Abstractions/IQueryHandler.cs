namespace AppShared.Cqrs.Abstractions;

/// <summary>
/// Handler for queries that return a value.
/// </summary>
/// <typeparam name="TQuery">The type of query to handle.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Handles the query asynchronously and returns a response.
    /// </summary>
    /// <param name="query">The query to handle.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The response from handling the query.</returns>
    Task<TResponse> HandleAsync(TQuery query, CancellationToken ct = default);
}
