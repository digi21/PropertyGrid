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
    private readonly Dictionary<CacheKey, DataTemplate?> cache = [];

    /// <summary>Gets or sets the editors registered on the grid this selector belongs to.</summary>
    public PropertyEditorTemplateMap? EditorTemplates { get; set; }

    internal PropertyGrid? Owner { get; set; }

    /// <summary>Forgets the editors it resolved, so the next row asks again.</summary>
    /// <remarks>
    /// Resolution depends on what is registered and on what the application's resources hold, and
    /// neither of those is watched. Anything changing them has to say so.
    /// </remarks>
    public void Invalidate() => cache.Clear();

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

        // The event is raised before anything else and is never cached: it exists so a consumer can
        // decide per row, using knowledge the type system does not carry.
        if (Owner?.RaiseEditorSelecting(row) is { } chosen)
        {
            return chosen;
        }

        CacheKey key = new(row.Description.EditorKey, row.PropertyType, row.RuntimeType);
        if (cache.TryGetValue(key, out DataTemplate? cached))
        {
            return cached;
        }

        DataTemplate? resolved = Resolve(row);
        cache[key] = resolved;
        return resolved;
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

    private readonly record struct CacheKey(string? EditorKey, Type PropertyType, Type? RuntimeType);
}
