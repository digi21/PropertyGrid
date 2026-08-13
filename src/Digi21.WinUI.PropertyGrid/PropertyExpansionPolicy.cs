namespace Digi21.WinUI.PropertyGrid;

/// <summary>Decides which properties a <see cref="PropertyGrid"/> offers to open into child rows.</summary>
public enum PropertyExpansionPolicy
{
    /// <summary>Nothing opens. Every property is a single row showing a summary of its value.</summary>
    None,

    /// <summary>
    /// Only what asked to. A property opens when it, or its type, carries
    /// <see cref="ExpandableAttribute"/>, or when the grid's metadata says so.
    /// </summary>
    Attributed,

    /// <summary>
    /// Anything worth opening does. Every property whose value is an object with properties of its
    /// own gets an expander, which is the right default for an inspector and the wrong one for a
    /// settings dialog: a rich object graph turns into a lot of chevrons.
    /// </summary>
    Automatic,
}
