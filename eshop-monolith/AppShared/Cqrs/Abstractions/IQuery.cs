namespace AppShared.Cqrs.Abstractions;

/// <summary>
/// Marker interface for queries that return a value of type <typeparamref name="TResponse"/>.
/// Queries are read-only operations that don't modify state.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface IQuery<out TResponse>
{
}
