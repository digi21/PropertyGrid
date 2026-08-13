namespace Digi21.WinUI.PropertyGrid;

/// <summary>The header of a group of properties in a <see cref="PropertyGrid"/>.</summary>
public sealed class PropertyGridCategoryRow : PropertyGridRow
{
    private readonly List<PropertyGridPropertyRow> properties = [];

    internal PropertyGridCategoryRow(PropertyGridSource source, string name)
        : base(source, "[" + name + "]", 0)
    {
        Name = name;
        SetExpandedQuietly(true);
    }

    /// <summary>Gets the name of the category.</summary>
    public string Name { get; }

    /// <inheritdoc />
    public override string DisplayName => Name;

    /// <inheritdoc />
    public override bool IsExpandable => true;

    /// <summary>Gets the properties in the category, whether or not the header is open.</summary>
    public IReadOnlyList<PropertyGridPropertyRow> Properties => properties;

    /// <summary>Gets how many of the properties in the category are currently shown.</summary>
    public int VisibleCount { get; private set; }

    internal void Add(PropertyGridPropertyRow property) => properties.Add(property);

    internal void SetVisibleCount(int count)
    {
        if (VisibleCount == count)
        {
            return;
        }

        VisibleCount = count;
        RaisePropertyChanged(nameof(VisibleCount));
    }
}
