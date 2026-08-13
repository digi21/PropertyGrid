using System.Reflection;

namespace Digi21.WinUI.PropertyGrid;

/// <summary>Reads and writes the value of one property on the objects a <see cref="PropertyGrid"/> shows.</summary>
/// <remarks>
/// <para>
/// Descriptions are cached per type and shared by every grid, so an accessor is never bound to one
/// instance: the target is passed in on every call.
/// </para>
/// <para>
/// Deriving from this is how an <see cref="IPropertyDescriptionProvider"/> exposes something that is
/// not a CLR property at all — an entry in a dictionary, a column of a database schema, a field of a
/// message parsed at run time.
/// </para>
/// </remarks>
public abstract class PropertyAccessor
{
    /// <summary>Gets a value indicating whether the property can be read.</summary>
    public abstract bool CanRead { get; }

    /// <summary>Gets a value indicating whether the property can be written.</summary>
    public abstract bool CanWrite { get; }

    /// <summary>Reads the value of the property from an object.</summary>
    /// <param name="target">The object to read from.</param>
    /// <returns>The value of the property.</returns>
    public object? GetValue(object target)
    {
        ArgumentNullException.ThrowIfNull(target);

        return GetValueCore(target);
    }

    /// <summary>Reads the value of the property from an object without letting a failure escape.</summary>
    /// <param name="target">The object to read from.</param>
    /// <param name="value">The value read, or <see langword="null"/> if reading failed.</param>
    /// <param name="error">The failure, or <see langword="null"/> if the value was read.</param>
    /// <returns><see langword="true"/> if the value was read.</returns>
    /// <remarks>
    /// Any getter can throw, and a grid shows a whole object at once: one property that fails must
    /// become one row showing an error, not an exception that takes the window down with it.
    /// </remarks>
    public bool TryGetValue(object target, out object? value, out Exception? error)
    {
        ArgumentNullException.ThrowIfNull(target);

        try
        {
            value = GetValueCore(target);
            error = null;
            return true;
        }
        catch (Exception exception)
        {
            value = null;
            error = Unwrap(exception);
            return false;
        }
    }

    /// <summary>Writes the value of the property on an object.</summary>
    /// <param name="target">The object to write to.</param>
    /// <param name="value">The value to write.</param>
    public void SetValue(object target, object? value)
    {
        ArgumentNullException.ThrowIfNull(target);

        SetValueCore(target, value);
    }

    /// <summary>Writes the value of the property on an object without letting a failure escape.</summary>
    /// <param name="target">The object to write to.</param>
    /// <param name="value">The value to write.</param>
    /// <param name="error">The failure, or <see langword="null"/> if the value was written.</param>
    /// <returns><see langword="true"/> if the value was written.</returns>
    /// <remarks>
    /// A setter that rejects a value by throwing is a normal way to express a domain rule, so the
    /// grid treats the exception message as a validation error on the row rather than as a crash.
    /// </remarks>
    public bool TrySetValue(object target, object? value, out Exception? error)
    {
        ArgumentNullException.ThrowIfNull(target);

        try
        {
            SetValueCore(target, value);
            error = null;
            return true;
        }
        catch (Exception exception)
        {
            error = Unwrap(exception);
            return false;
        }
    }

    /// <summary>Reads the value of the property from an object.</summary>
    /// <param name="target">The object to read from, never <see langword="null"/>.</param>
    /// <returns>The value of the property.</returns>
    protected abstract object? GetValueCore(object target);

    /// <summary>Writes the value of the property on an object.</summary>
    /// <param name="target">The object to write to, never <see langword="null"/>.</param>
    /// <param name="value">The value to write.</param>
    protected abstract void SetValueCore(object target, object? value);

    // Reflection wraps whatever the property body threw in a TargetInvocationException, and the
    // wrapper's message is boilerplate. The row shows the message, so it has to be the real one.
    private static Exception Unwrap(Exception exception) =>
        exception is TargetInvocationException { InnerException: { } inner } ? inner : exception;
}
