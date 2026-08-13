namespace Digi21.WinUI.PropertyGrid;

/// <summary>How a <see cref="PropertyGrid"/> orders and groups the properties it shows.</summary>
/// <remarks>
/// An explicit order, from <see cref="PropertyOrderAttribute"/> or from
/// <c>[Display(Order = …)]</c>, is honoured in every mode: it says where the author wanted the
/// property, and no view mode should silently discard that. The mode decides what happens to
/// everything that did not ask for a position.
/// </remarks>
public enum PropertySort
{
    /// <summary>
    /// Declaration order: the properties of a base type first, then the order they appear in the
    /// source. Categories are not shown.
    /// </summary>
    NoSort,

    /// <summary>By display name, with no categories.</summary>
    Alphabetical,

    /// <summary>
    /// Grouped into categories, each category placed where it first appears in declaration order,
    /// and the properties inside it left in declaration order.
    /// </summary>
    Categorized,

    /// <summary>
    /// Grouped into categories sorted by name, with the properties inside each sorted by display
    /// name. This is the arrangement Visual Studio and Actipro's grid show by default.
    /// </summary>
    CategorizedAlphabetical,
}
