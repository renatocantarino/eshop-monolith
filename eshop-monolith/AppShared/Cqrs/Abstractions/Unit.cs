namespace AppShared.Cqrs.Abstractions;

/// <summary>
/// Represents a void type, since <see cref="System.Void"/> is not a valid return type in C#.
/// Use this type when you need to represent a "no value" return type in generic contexts.
/// </summary>
public readonly struct Unit
{
    private static readonly Unit _value = new();

    /// <summary>
    /// Gets the singleton instance of <see cref="Unit"/>.
    /// </summary>
    public static Unit Value => _value;

    /// <summary>
    /// Gets a completed task with a <see cref="Unit"/> result.
    /// </summary>
    public static Task<Unit> Task => System.Threading.Tasks.Task.FromResult(_value);

    /// <summary>
    /// Compares two <see cref="Unit"/> values for equality.
    /// </summary>
    public override bool Equals(object? obj) => obj is Unit;

    /// <summary>
    /// Returns the hash code for this instance.
    /// </summary>
    public override int GetHashCode() => 0;

    /// <summary>
    /// Returns a string representation of this instance.
    /// </summary>
    public override string ToString() => "()";

    /// <summary>
    /// Equality operator for <see cref="Unit"/>.
    /// </summary>
    public static bool operator ==(Unit left, Unit right) => true;

    /// <summary>
    /// Inequality operator for <see cref="Unit"/>.
    /// </summary>
    public static bool operator !=(Unit left, Unit right) => false;
}
