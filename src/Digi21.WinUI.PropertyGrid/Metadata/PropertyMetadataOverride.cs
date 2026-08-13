namespace Digi21.WinUI.PropertyGrid;

// One type's disagreement with what the attributes on a property said. Every field is nullable
// because "not mentioned" and "set to the default" have to stay distinguishable: a store that said
// nothing about the category must not silently move the property to the default one.
internal sealed class PropertyMetadataOverride
{
    internal string? DisplayName { get; set; }

    internal string? HelpText { get; set; }

    internal string? CategoryName { get; set; }

    internal int? Order { get; set; }

    internal bool? IsBrowsable { get; set; }

    internal bool? IsReadOnly { get; set; }

    internal bool? IsMergable { get; set; }

    internal string? EditorKey { get; set; }

    internal bool? IsExpandable { get; set; }

    internal bool HasDefaultValue { get; set; }

    internal object? DefaultValue { get; set; }

    internal PropertyDescription ApplyTo(PropertyDescription description) => description with
    {
        DisplayName = DisplayName ?? description.DisplayName,
        HelpText = HelpText ?? description.HelpText,
        CategoryName = CategoryName ?? description.CategoryName,
        Order = Order ?? description.Order,
        IsBrowsable = IsBrowsable ?? description.IsBrowsable,
        IsReadOnly = IsReadOnly ?? description.IsReadOnly,
        IsMergable = IsMergable ?? description.IsMergable,
        EditorKey = EditorKey ?? description.EditorKey,
        IsExpandable = IsExpandable ?? description.IsExpandable,
        HasDefaultValue = HasDefaultValue || description.HasDefaultValue,
        DefaultValue = HasDefaultValue ? DefaultValue : description.DefaultValue,
    };
}
