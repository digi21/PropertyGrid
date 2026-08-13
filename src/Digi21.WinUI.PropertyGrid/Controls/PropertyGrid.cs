using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Digi21.WinUI.PropertyGrid;

/// <summary>
/// Shows the properties of an object as an editable list, one row per property, with the editor of
/// each row chosen from the type of the property it edits.
/// </summary>
public partial class PropertyGrid : Control
{
    /// <summary>Initializes a new instance of the <see cref="PropertyGrid"/> class.</summary>
    public PropertyGrid()
    {
        DefaultStyleKey = typeof(PropertyGrid);
        DefaultStyleResourceUri = new Uri("ms-appx:///Digi21.WinUI.PropertyGrid/Themes/Generic.xaml");
    }
}
