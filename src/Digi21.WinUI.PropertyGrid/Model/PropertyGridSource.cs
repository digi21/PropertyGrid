using System.Globalization;

namespace Digi21.WinUI.PropertyGrid;

/// <summary>
/// Builds and keeps up to date the rows a <see cref="PropertyGrid"/> shows: the tree of categories
/// and properties behind them, and the flat list the grid actually renders.
/// </summary>
/// <remarks>
/// This is the whole model of the control, and it deliberately knows nothing about XAML. Everything
/// it does — discovery, arrangement, filtering, expansion, validation, reacting to the object
/// changing underneath it — is reachable from a test with no interface thread in sight.
/// </remarks>
public sealed class PropertyGridSource : ITargetObserver
{
    private readonly PropertyRowCollection visibleRows = [];
    private readonly List<PropertyGridCategoryRow> categories = [];
    private readonly HashSet<string> expandedKeys = new(StringComparer.Ordinal);
    private readonly Dictionary<object, WeakTargetListener> listeners = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<object, List<PropertyGridPropertyRow>> rowsByTarget =
        new(ReferenceEqualityComparer.Instance);

    private IPropertyDescriptionProvider provider = ReflectionPropertyDescriptionProvider.Default;
    private PropertySort sort = PropertySort.CategorizedAlphabetical;
    private PropertyExpansionPolicy expansionPolicy = PropertyExpansionPolicy.Attributed;
    private CultureInfo culture = CultureInfo.CurrentCulture;
    private string defaultCategoryName = PropertyGridStrings.DefaultCategoryName;
    private object? target;
    private Type? targetType;
    private string? filterText;
    private Predicate<PropertyGridPropertyRow>? filter;
    private int maximumExpansionDepth = 10;
    private bool isReadOnly;
    private bool rebuilding;

    /// <summary>Gets the rows to render, headers and properties together, in the order they appear.</summary>
    public IReadOnlyList<PropertyGridRow> Rows => visibleRows;

    /// <summary>Gets the categories the properties were grouped into, whether or not they are open.</summary>
    public IReadOnlyList<PropertyGridCategoryRow> Categories => categories;

    /// <summary>Gets or sets how the properties of a type are discovered and described.</summary>
    public IPropertyDescriptionProvider Provider
    {
        get => provider;
        set => Reconfigure(ref provider, value ?? ReflectionPropertyDescriptionProvider.Default, rebuild: true);
    }

    /// <summary>Gets or sets how the rows are ordered and grouped.</summary>
    public PropertySort Sort
    {
        get => sort;
        set => Reconfigure(ref sort, value, rebuild: true);
    }

    /// <summary>Gets or sets which properties can be opened into child rows.</summary>
    public PropertyExpansionPolicy ExpansionPolicy
    {
        get => expansionPolicy;
        set => Reconfigure(ref expansionPolicy, value, rebuild: true);
    }

    /// <summary>Gets or sets how deeply the grid follows an object graph before it stops offering to go further.</summary>
    public int MaximumExpansionDepth
    {
        get => maximumExpansionDepth;
        set => Reconfigure(ref maximumExpansionDepth, Math.Max(0, value), rebuild: true);
    }

    /// <summary>Gets or sets the culture values are shown and parsed in.</summary>
    public CultureInfo Culture
    {
        get => culture;
        set => Reconfigure(ref culture, value ?? CultureInfo.CurrentCulture, rebuild: true);
    }

    /// <summary>Gets or sets the category properties land in when they do not name one.</summary>
    public string DefaultCategoryName
    {
        get => defaultCategoryName;
        set => Reconfigure(ref defaultCategoryName, string.IsNullOrWhiteSpace(value) ? PropertyGridStrings.DefaultCategoryName : value, rebuild: true);
    }

    /// <summary>Gets or sets a value indicating whether every row refuses to be edited.</summary>
    public bool IsReadOnly
    {
        get => isReadOnly;
        set => Reconfigure(ref isReadOnly, value, rebuild: true);
    }

    /// <summary>Gets or sets which layers of validation run when a value is edited.</summary>
    public PropertyGridValidationMode ValidationMode { get; set; } = PropertyGridValidationMode.All;

    /// <summary>Gets the validators consulted before a value is written.</summary>
    public IList<IPropertyValidator> Validators { get; } = [];

    /// <summary>Gets or sets text every shown property has to contain in its name or description.</summary>
    public string? FilterText
    {
        get => filterText;
        set => Reconfigure(ref filterText, value, rebuild: false, refilter: true);
    }

    /// <summary>Gets or sets an arbitrary test every shown property has to pass, on top of <see cref="FilterText"/>.</summary>
    public Predicate<PropertyGridPropertyRow>? Filter
    {
        get => filter;
        set => Reconfigure(ref filter, value, rebuild: false, refilter: true);
    }

    /// <summary>Gets or sets a chance to change or drop each property as the rows are built.</summary>
    /// <remarks>
    /// Returning <see langword="null"/> leaves the property out. This is the hook behind the grid's
    /// auto-generating event, and the least ceremonious way to adjust a type you do not own.
    /// </remarks>
    public Func<PropertyDescription, PropertyDescription?>? DescriptionFilter { get; set; }

    /// <summary>Gets or sets a chance to veto a value before it is written.</summary>
    /// <remarks>The string returned is the reason shown on the row; returning <see langword="null"/> allows the write.</remarks>
    public Func<PropertyGridPropertyRow, object?, object?, string?>? ValueChanging { get; set; }

    /// <summary>Gets or sets a callback raised after a value has been written.</summary>
    public Action<PropertyGridPropertyRow, object?, object?>? ValueChanged { get; set; }

    /// <summary>Gets or sets a callback raised whenever the list of visible rows has been rebuilt.</summary>
    public Action? RowsChanged { get; set; }

    /// <summary>Shows the properties of an object.</summary>
    /// <param name="value">The object to show, or <see langword="null"/> to show nothing.</param>
    public void SetTarget(object? value)
    {
        target = value;
        targetType = null;
        Rebuild();
    }

    /// <summary>Shows the properties a type declares, with no object behind them.</summary>
    /// <param name="type">The type to show, or <see langword="null"/> to show nothing.</param>
    /// <remarks>
    /// Every row is read-only: there is nothing to read a value from and nothing to write one to.
    /// This is for looking at the shape of a type rather than at an instance of it.
    /// </remarks>
    public void SetTargetType(Type? type)
    {
        target = null;
        targetType = type;
        Rebuild();
    }

    /// <summary>Reads every value again and rebuilds the rows from scratch.</summary>
    public void Refresh() => Rebuild();

    /// <summary>Finds a row by the name of the property it edits.</summary>
    /// <param name="path">The name of the property, or a path such as <c>Address.City</c>.</param>
    /// <returns>The row, or <see langword="null"/> if the grid is not showing it.</returns>
    public PropertyGridPropertyRow? FindRow(string path)
    {
        foreach (PropertyGridCategoryRow category in categories)
        {
            foreach (PropertyGridPropertyRow row in category.Properties)
            {
                PropertyGridPropertyRow? found = FindIn(row, path);
                if (found is not null)
                {
                    return found;
                }
            }
        }

        return null;
    }

    /// <summary>Opens every category header.</summary>
    public void ExpandAllCategories() => SetAllCategories(true);

    /// <summary>Closes every category header.</summary>
    public void CollapseAllCategories() => SetAllCategories(false);

    /// <summary>Closes every property that was opened into child rows.</summary>
    public void CollapseAll()
    {
        foreach (PropertyGridCategoryRow category in categories)
        {
            foreach (PropertyGridPropertyRow row in category.Properties)
            {
                CollapseTree(row);
            }
        }

        RebuildVisibleRows();
    }

    void ITargetObserver.OnTargetPropertyChanged(object changed, string? propertyName)
    {
        if (!rowsByTarget.TryGetValue(changed, out List<PropertyGridPropertyRow>? affected))
        {
            return;
        }

        bool structureChanged = false;

        // An empty name is the convention for "everything about me changed", and refusing to honour
        // it is a classic source of a grid that quietly stops matching its object.
        foreach (PropertyGridPropertyRow row in affected.ToArray())
        {
            if (!string.IsNullOrEmpty(propertyName) && !string.Equals(row.Name, propertyName, StringComparison.Ordinal))
            {
                continue;
            }

            bool wasExpandable = row.IsExpandable;
            row.Refresh();

            // A property whose value was replaced is showing the children of the old value.
            if (row.Children.Count > 0 || wasExpandable != row.IsExpandable)
            {
                row.InvalidateChildren();
                structureChanged = true;
            }
        }

        if (structureChanged)
        {
            RebuildVisibleRows();
        }
    }

    void ITargetObserver.OnTargetErrorsChanged(object changed, string? propertyName)
    {
        if (!rowsByTarget.TryGetValue(changed, out List<PropertyGridPropertyRow>? affected))
        {
            return;
        }

        foreach (PropertyGridPropertyRow row in affected)
        {
            if (string.IsNullOrEmpty(propertyName) || string.Equals(row.Name, propertyName, StringComparison.Ordinal))
            {
                row.CollectTargetErrors();
            }
        }
    }

    internal bool CanExpand(PropertyGridPropertyRow row, object? value)
    {
        if (expansionPolicy == PropertyExpansionPolicy.None
            || row.Description.IsExpandable == false
            || value is null
            || row.Depth >= maximumExpansionDepth)
        {
            return false;
        }

        // Editing a field of a struct means writing the whole struct back through its parent, and
        // this version does not do that. Offering the chevron anyway would produce edits that look
        // accepted and are silently lost, which is worse than not offering it.
        if (value.GetType().IsValueType || value is string)
        {
            return false;
        }

        // A cycle in the object graph is the fastest way to hang the control: without this the
        // chevron on a child that points back at its own ancestor never stops offering more.
        for (PropertyGridPropertyRow? ancestor = row.Parent; ancestor is not null; ancestor = ancestor.Parent)
        {
            if (ReferenceEquals(ancestor.Value, value))
            {
                return false;
            }
        }

        if (target is not null && ReferenceEquals(target, value))
        {
            return false;
        }

        if (row.Description.IsExpandable != true && expansionPolicy != PropertyExpansionPolicy.Automatic)
        {
            return false;
        }

        if (KnownTypes.IsSimple(row.Description.PropertyType) || KnownTypes.IsCollection(value.GetType()))
        {
            return false;
        }

        return provider.GetProperties(value.GetType()).Count > 0;
    }

    internal IReadOnlyList<PropertyGridRow> BuildChildren(PropertyGridPropertyRow parent, object value)
    {
        List<PropertyGridRow> built = [];
        foreach (PropertyDescription description in Arrange(value.GetType()))
        {
            built.Add(CreateRow(description, value, parent.Key + "." + description.Name, parent.Depth + 1, parent.Category, parent));
        }

        return built;
    }

    internal void OnExpansionChanged(PropertyGridRow row)
    {
        if (rebuilding)
        {
            return;
        }

        if (row is PropertyGridPropertyRow property && property.IsExpanded)
        {
            // Children are built the first time a row is opened and never before: walking the graph
            // to draw the top level would follow lazy properties nobody asked to see.
            property.EnsureChildren();
        }

        RememberExpansion(row);
        RebuildVisibleRows();
    }

    internal bool OnValueChanging(PropertyGridPropertyRow row, object? oldValue, object? newValue, out string? reason)
    {
        reason = ValueChanging?.Invoke(row, oldValue, newValue);
        return reason is null;
    }

    internal void OnValueChanged(PropertyGridPropertyRow row, object? oldValue, object? newValue) =>
        ValueChanged?.Invoke(row, oldValue, newValue);

    private static PropertyGridPropertyRow? FindIn(PropertyGridPropertyRow row, string path)
    {
        if (string.Equals(row.Key, path, StringComparison.Ordinal) || string.Equals(row.Name, path, StringComparison.Ordinal))
        {
            return row;
        }

        foreach (PropertyGridRow child in row.Children)
        {
            if (child is PropertyGridPropertyRow property && FindIn(property, path) is { } found)
            {
                return found;
            }
        }

        return null;
    }

    private static void CollapseTree(PropertyGridPropertyRow row)
    {
        foreach (PropertyGridRow child in row.Children)
        {
            if (child is PropertyGridPropertyRow property)
            {
                CollapseTree(property);
            }
        }

        row.SetExpandedQuietly(false);
    }

    private void Reconfigure<T>(ref T field, T value, bool rebuild, bool refilter = false)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;

        if (rebuild)
        {
            Rebuild();
        }
        else if (refilter)
        {
            RebuildVisibleRows();
        }
    }

    private void Rebuild()
    {
        rebuilding = true;
        try
        {
            DetachListeners();
            categories.Clear();
            rowsByTarget.Clear();

            Type? shown = target?.GetType() ?? targetType;
            if (shown is not null)
            {
                Dictionary<string, PropertyGridCategoryRow> byName = new(StringComparer.Ordinal);

                foreach (PropertyDescription description in Arrange(shown))
                {
                    string categoryName = PropertyDescriptionSorter.CategoryOf(description, defaultCategoryName);
                    if (!byName.TryGetValue(categoryName, out PropertyGridCategoryRow? category))
                    {
                        category = new PropertyGridCategoryRow(this, categoryName);
                        byName[categoryName] = category;
                        categories.Add(category);
                    }

                    category.Add(CreateRow(description, target, description.Name, 0, category, parent: null));
                }

                // A category that was closed before the rebuild stays closed: the arrangement
                // changing is not a reason to undo what the user collapsed.
                foreach (PropertyGridCategoryRow category in categories)
                {
                    category.SetExpandedQuietly(!expandedKeys.Contains(ClosedKey(category.Key)));
                }
            }
        }
        finally
        {
            rebuilding = false;
        }

        RebuildVisibleRows();
    }

    private IReadOnlyList<PropertyDescription> Arrange(Type type)
    {
        IReadOnlyList<PropertyDescription> discovered = provider.GetProperties(type);

        if (DescriptionFilter is { } adjust)
        {
            List<PropertyDescription> kept = [];
            foreach (PropertyDescription description in discovered)
            {
                if (adjust(description) is { } adjusted)
                {
                    kept.Add(adjusted);
                }
            }

            discovered = kept;
        }

        return PropertyDescriptionSorter.Sort(discovered, sort, defaultCategoryName);
    }

    private PropertyGridPropertyRow CreateRow(
        PropertyDescription description,
        object? owner,
        string key,
        int depth,
        PropertyGridCategoryRow? category,
        PropertyGridPropertyRow? parent)
    {
        // With no object behind it there is nothing to read or write, so the row shows the shape of
        // the property and refuses every edit. The placeholder keeps the accessor from being handed
        // a null it would only throw on.
        object rowTarget = owner ?? SchemaPlaceholder.Instance;

        PropertyDescription effective = owner is null
            ? description with { IsReadOnly = true, Accessor = SchemaPlaceholder.Accessor }
            : description;

        PropertyGridPropertyRow row = new(this, effective, rowTarget, key, depth, category, parent);

        if (owner is not null)
        {
            Observe(owner, row);
        }

        if (expandedKeys.Contains(key) && row.IsExpandable)
        {
            row.EnsureChildren();
            row.SetExpandedQuietly(true);
        }

        return row;
    }

    private void Observe(object owner, PropertyGridPropertyRow row)
    {
        if (!rowsByTarget.TryGetValue(owner, out List<PropertyGridPropertyRow>? forTarget))
        {
            forTarget = [];
            rowsByTarget[owner] = forTarget;
        }

        forTarget.Add(row);

        if (!listeners.ContainsKey(owner))
        {
            listeners[owner] = new WeakTargetListener(owner, this);
        }
    }

    private void DetachListeners()
    {
        foreach (WeakTargetListener listener in listeners.Values)
        {
            listener.Detach();
        }

        listeners.Clear();
    }

    private void RebuildVisibleRows()
    {
        List<PropertyGridRow> flattened = [];
        bool showHeaders = sort is PropertySort.Categorized or PropertySort.CategorizedAlphabetical;
        bool filtering = !string.IsNullOrWhiteSpace(filterText) || filter is not null;

        foreach (PropertyGridCategoryRow category in categories)
        {
            List<PropertyGridPropertyRow> matching = [];
            foreach (PropertyGridPropertyRow row in category.Properties)
            {
                if (Matches(row))
                {
                    matching.Add(row);
                }
            }

            category.SetVisibleCount(matching.Count);

            if (matching.Count == 0)
            {
                continue;
            }

            if (showHeaders)
            {
                flattened.Add(category);

                // While a filter is on, a closed category would hide the very matches the user is
                // looking for, so the filter wins over the collapsed state without discarding it.
                if (!category.IsExpanded && !filtering)
                {
                    continue;
                }
            }

            foreach (PropertyGridPropertyRow row in matching)
            {
                Append(row, flattened);
            }
        }

        visibleRows.Replace(flattened);
        RowsChanged?.Invoke();
    }

    private void Append(PropertyGridPropertyRow row, List<PropertyGridRow> flattened)
    {
        flattened.Add(row);

        if (!row.IsExpanded)
        {
            return;
        }

        foreach (PropertyGridRow child in row.Children)
        {
            if (child is PropertyGridPropertyRow property)
            {
                Append(property, flattened);
            }
        }
    }

    private bool Matches(PropertyGridPropertyRow row)
    {
        if (filter is { } test && !test(row))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(filterText))
        {
            return true;
        }

        return row.DisplayName.Contains(filterText, StringComparison.CurrentCultureIgnoreCase)
            || row.Name.Contains(filterText, StringComparison.CurrentCultureIgnoreCase)
            || (row.HelpText?.Contains(filterText, StringComparison.CurrentCultureIgnoreCase) ?? false);
    }

    private void SetAllCategories(bool expanded)
    {
        foreach (PropertyGridCategoryRow category in categories)
        {
            category.SetExpandedQuietly(expanded);
            RememberExpansion(category);
        }

        RebuildVisibleRows();
    }

    // Categories start open and properties start closed, so what has to survive a rebuild is the
    // departure from the default in each case, and the two are kept apart in the one set by
    // prefixing the category keys.
    private void RememberExpansion(PropertyGridRow row)
    {
        bool isCategory = row is PropertyGridCategoryRow;
        string key = isCategory ? ClosedKey(row.Key) : row.Key;

        if (isCategory ? !row.IsExpanded : row.IsExpanded)
        {
            expandedKeys.Add(key);
        }
        else
        {
            expandedKeys.Remove(key);
        }
    }

    private static string ClosedKey(string categoryKey) => "!" + categoryKey;

    private sealed class SchemaPlaceholder
    {
        internal static SchemaPlaceholder Instance { get; } = new();

        internal static PropertyAccessor Accessor { get; } = new NullAccessor();

        private sealed class NullAccessor : PropertyAccessor
        {
            public override bool CanRead => true;

            public override bool CanWrite => false;

            protected override object? GetValueCore(object target) => null;

            protected override void SetValueCore(object target, object? value) =>
                throw new NotSupportedException("The grid is showing a type, not an object.");
        }
    }
}
