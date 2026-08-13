using Microsoft.UI.Xaml;

namespace Digi21.WinUI.PropertyGrid;

/// <summary>Carries a row about to pick its editor, with a chance to supply a different one.</summary>
/// <remarks>
/// The last-resort hook, raised before anything registered is consulted. It is for decisions the
/// type system does not carry — this one <c>int</c> is a percentage, that one is a port number —
/// which are otherwise expressed by putting <see cref="PropertyEditorAttribute"/> on the property.
/// </remarks>
public sealed class PropertyEditorSelectingEventArgs : EventArgs
{
    internal PropertyEditorSelectingEventArgs(PropertyGridPropertyRow row) => Row = row;

    /// <summary>Gets the row about to be given an editor.</summary>
    public PropertyGridPropertyRow Row { get; }

    /// <summary>Gets or sets the editor to use, or <see langword="null"/> to let the grid decide.</summary>
    public DataTemplate? Template { get; set; }
}
