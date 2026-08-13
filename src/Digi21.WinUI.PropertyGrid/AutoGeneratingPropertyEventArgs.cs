namespace Digi21.WinUI.PropertyGrid;

/// <summary>Carries one property as the grid is building its rows, so it can be changed or dropped.</summary>
/// <remarks>
/// The imperative counterpart of <see cref="PropertyGridMetadata"/>, and the shortest way to adjust
/// a type you do not own. Every field starts at what the attributes on the property said.
/// </remarks>
public sealed class AutoGeneratingPropertyEventArgs : EventArgs
{
    internal AutoGeneratingPropertyEventArgs(PropertyDescription description)
    {
        Description = description;
        DisplayName = description.DisplayName;
        HelpText = description.HelpText;
        CategoryName = description.CategoryName;
        Order = description.Order;
        IsReadOnly = description.IsReadOnly;
        EditorKey = description.EditorKey;
        IsExpandable = description.IsExpandable;
    }

    /// <summary>Gets what the grid found out about the property by itself.</summary>
    public PropertyDescription Description { get; }

    /// <summary>Gets the name of the property as it is declared in code.</summary>
    public string Name => Description.Name;

    /// <summary>Gets the declared type of the property.</summary>
    public Type PropertyType => Description.PropertyType;

    /// <summary>Gets or sets the label to show.</summary>
    public string DisplayName { get; set; }

    /// <summary>Gets or sets the sentence explaining the property.</summary>
    public string? HelpText { get; set; }

    /// <summary>Gets or sets the category to put the property in.</summary>
    public string? CategoryName { get; set; }

    /// <summary>Gets or sets where the property sits, lowest first.</summary>
    public int Order { get; set; }

    /// <summary>Gets or sets a value indicating whether the grid refuses to write the property.</summary>
    public bool IsReadOnly { get; set; }

    /// <summary>Gets or sets the name of the editor to use instead of the one the type would resolve to.</summary>
    public string? EditorKey { get; set; }

    /// <summary>Gets or sets whether the property can be opened into child rows, or <see langword="null"/> to let the policy decide.</summary>
    public bool? IsExpandable { get; set; }

    /// <summary>Gets or sets a value indicating whether the property is left out of the grid altogether.</summary>
    public bool Cancel { get; set; }

    internal PropertyDescription ToDescription() => Description with
    {
        DisplayName = DisplayName,
        HelpText = HelpText,
        CategoryName = CategoryName,
        Order = Order,
        IsReadOnly = IsReadOnly,
        EditorKey = EditorKey,
        IsExpandable = IsExpandable,
    };
}
