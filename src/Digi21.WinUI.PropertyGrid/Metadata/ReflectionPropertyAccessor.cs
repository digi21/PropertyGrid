using System.Reflection;

namespace Digi21.WinUI.PropertyGrid;

/// <summary>A <see cref="PropertyAccessor"/> backed by a CLR property.</summary>
public sealed class ReflectionPropertyAccessor : PropertyAccessor
{
    private readonly PropertyInfo property;

    /// <summary>Initializes a new instance of the <see cref="ReflectionPropertyAccessor"/> class.</summary>
    /// <param name="property">The property to read and write.</param>
    public ReflectionPropertyAccessor(PropertyInfo property)
    {
        ArgumentNullException.ThrowIfNull(property);

        this.property = property;
        CanRead = property.GetMethod is { IsPublic: true };
        CanWrite = property.SetMethod is { IsPublic: true } setter && !IsInitOnly(setter);
    }

    /// <inheritdoc />
    public override bool CanRead { get; }

    /// <inheritdoc />
    public override bool CanWrite { get; }

    /// <summary>Gets the property this accessor reads and writes.</summary>
    public PropertyInfo Property => property;

    /// <inheritdoc />
    protected override object? GetValueCore(object target) => property.GetValue(target);

    /// <inheritdoc />
    protected override void SetValueCore(object target, object? value) => property.SetValue(target, value);

    // An init-only setter is a perfectly ordinary public setter as far as reflection is concerned,
    // and calling it after construction succeeds. The compiler marks it with a modreq on the return
    // parameter, and that modreq is the only thing separating `{ get; init; }` from `{ get; set; }`.
    // Without this check every record property would look editable and silently accept edits that
    // the language forbids.
    private static bool IsInitOnly(MethodInfo setter) =>
        Array.Exists(
            setter.ReturnParameter.GetRequiredCustomModifiers(),
            modifier => modifier.FullName == "System.Runtime.CompilerServices.IsExternalInit");
}
