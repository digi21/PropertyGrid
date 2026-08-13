namespace Digi21.WinUI.PropertyGrid;

/// <summary>
/// Says whether a property, or every property of a type, can be opened in the grid to show the
/// properties of its value as indented child rows.
/// </summary>
/// <remarks>
/// Applied to a type it means "instances of this are worth exploring", which is the useful form for
/// a model type you own. Applied to a property it overrides that for the one property, which is how
/// a single field of an otherwise interesting type is kept closed.
/// </remarks>
[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct,
    AllowMultiple = false,
    Inherited = true)]
public sealed class ExpandableAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="ExpandableAttribute"/> class.</summary>
    /// <param name="isExpandable">Whether the value can be opened. Defaults to <see langword="true"/>.</param>
    public ExpandableAttribute(bool isExpandable = true) => IsExpandable = isExpandable;

    /// <summary>Gets a value indicating whether the value can be opened into child rows.</summary>
    public bool IsExpandable { get; }
}
