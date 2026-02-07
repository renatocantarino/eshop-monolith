namespace AppShared.Cqrs.Abstractions;

/// <summary>
/// Marker interface for commands that don't return a value.
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Marker interface for commands that return a value of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
public interface ICommand<out TResponse>
{
}

