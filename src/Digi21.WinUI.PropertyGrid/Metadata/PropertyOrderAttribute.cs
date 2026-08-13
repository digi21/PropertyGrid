namespace Digi21.WinUI.PropertyGrid;

/// <summary>Places a property at a given position in a <see cref="PropertyGrid"/>.</summary>
/// <remarks>
/// Properties without an order come after every property that has one, in whatever arrangement the
/// grid's <see cref="PropertySort"/> asks for. Ties between equal orders are broken the same way.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class PropertyOrderAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="PropertyOrderAttribute"/> class.</summary>
    /// <param name="order">The position, lowest first.</param>
    public PropertyOrderAttribute(int order) => Order = order;

    /// <summary>Gets the position of the property, lowest first.</summary>
    public int Order { get; }
}
