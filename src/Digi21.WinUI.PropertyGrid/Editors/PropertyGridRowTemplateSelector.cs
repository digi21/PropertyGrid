using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Digi21.WinUI.PropertyGrid;

/// <summary>Chooses the shape of a row: a category header, or a property.</summary>
/// <remarks>
/// <para>
/// Only the two structural templates live here. Which editor goes in the value cell is decided
/// separately, inside the row, by a <see cref="PropertyEditorSelector"/> on a content presenter.
/// </para>
/// <para>
/// That split is what makes the list recycle well: the repeater only ever sees two templates instead
/// of one per property type, so almost every row scrolling into view reuses a pooled presenter and
/// only the contents of its value cell change.
/// </para>
/// </remarks>
public partial class PropertyGridRowTemplateSelector : DataTemplateSelector
{
    /// <summary>Gets or sets the template used for a <see cref="PropertyGridCategoryRow"/>.</summary>
    public DataTemplate? CategoryHeaderTemplate { get; set; }

    /// <summary>Gets or sets the template used for a <see cref="PropertyGridPropertyRow"/>.</summary>
    public DataTemplate? PropertyTemplate { get; set; }

    // The framework calls one overload or the other depending on the host, and what it hands over as
    // the container is the repeater itself rather than a per-item container. Both are overridden so
    // the answer does not depend on which entry point was used, and neither looks at the container.

    /// <inheritdoc />
    protected override DataTemplate? SelectTemplateCore(object item) => Select(item);

    /// <inheritdoc />
    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container) => Select(item);

    private DataTemplate? Select(object item) => item switch
    {
        PropertyGridCategoryRow => CategoryHeaderTemplate,
        PropertyGridPropertyRow => PropertyTemplate,
        _ => null,
    };
}
