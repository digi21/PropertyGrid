using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Digi21.WinUI.PropertyGrid;

/// <summary>Chooses the editor that renders the value cell of a property.</summary>
/// <remarks>
/// Resolution goes: whatever the grid's <c>EditorSelecting</c> event supplies, then the grid's own
/// registered editors, then the ones registered for the whole application, then the built-in table —
/// whose templates are looked up by name in the application's resources, so redeclaring one of the
/// <see cref="PropertyEditorKeys"/> names replaces that editor everywhere.
/// </remarks>
public partial class PropertyEditorSelector : DataTemplateSelector
{
    /// <summary>Gets or sets the editors registered on the grid this selector belongs to.</summary>
    public PropertyEditorTemplateMap? EditorTemplates { get; set; }

    internal PropertyGrid? Owner { get; set; }

    /// <summary>Does nothing, and is kept so that calling it stays harmless.</summary>
    /// <remarks>
    /// Resolution used to be memoized and is not any more. The key was the declared type, the
    /// runtime type and the requested name, and that stopped identifying an editor the moment a
    /// single property could carry its own list of values or its own <c>[FilePath]</c>: two strings
    /// in the same object would resolve once and share the answer, so whichever came first decided
    /// for the other. Resolving is a handful of type comparisons and a dictionary lookup, run once
    /// per row realized, so there was nothing worth the hazard.
    /// </remarks>
    public void Invalidate()
    {
    }

    /// <inheritdoc />
    protected override DataTemplate? SelectTemplateCore(object item) => Select(item);

    /// <inheritdoc />
    protected override DataTemplate? SelectTemplateCore(object item, DependencyObject container) => Select(item);

    private DataTemplate? Select(object item)
    {
        if (item is not PropertyGridPropertyRow row)
        {
            return null;
        }

        // The event comes first: it exists so a consumer can decide per row, using knowledge the
        // type system does not carry.
        return Owner?.RaiseEditorSelecting(row) ?? Resolve(row);
    }

    private DataTemplate? Resolve(PropertyGridPropertyRow row)
    {
        string? explicitKey = row.Description.EditorKey;

        if (EditorTemplates?.Resolve(row.PropertyType, row.RuntimeType, explicitKey) is { } registered)
        {
            return registered;
        }

        if (PropertyEditorTemplateMap.Default.Resolve(row.PropertyType, row.RuntimeType, explicitKey) is { } shared)
        {
            return shared;
        }

        // A property is free to name a template declared in the application's resources without
        // registering anything at all, which is the least ceremonious way to use a custom editor.
        if (!string.IsNullOrEmpty(explicitKey) && PropertyGridThemeResources.Template(explicitKey) is { } named)
        {
            return named;
        }

        string builtIn = BuiltInEditors.KeyFor(row.Description, row.RuntimeType);

        return PropertyGridThemeResources.Template(builtIn)
            ?? PropertyGridThemeResources.Template(PropertyEditorKeys.ReadOnly);
    }
}
