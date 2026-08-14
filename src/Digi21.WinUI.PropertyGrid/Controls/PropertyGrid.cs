using System.Globalization;
using Digi21.WinUI.PropertyGrid.Primitives;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Digi21.WinUI.PropertyGrid;

/// <summary>
/// Shows the properties of an object as an editable list, one row per property, with the editor of
/// each row chosen from the type of the property it edits.
/// </summary>
/// <remarks>
/// <para>
/// Point it at an object with <see cref="SelectedObject"/> and it does the rest: it reflects over
/// the type, reads the attributes on each property to label and group it, picks an editor per type,
/// and keeps both sides in step if the object reports its own changes.
/// </para>
/// <para>
/// Everything it draws comes from a <c>{ThemeResource PropertyGrid…}</c> key, and every editor from
/// a template named in <see cref="PropertyEditorKeys"/>, so both the look and the editors can be
/// replaced from the application's resources without retemplating the control.
/// </para>
/// </remarks>
public partial class PropertyGrid : Control
{
    /// <summary>Identifies the <see cref="SelectedObject"/> dependency property.</summary>
    public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
        nameof(SelectedObject),
        typeof(object),
        typeof(PropertyGrid),
        new PropertyMetadata(null, (d, e) => ((PropertyGrid)d).OnSelectedObjectChanged(e.NewValue)));

    /// <summary>Identifies the <see cref="SelectedType"/> dependency property.</summary>
    public static readonly DependencyProperty SelectedTypeProperty = DependencyProperty.Register(
        nameof(SelectedType),
        typeof(Type),
        typeof(PropertyGrid),
        new PropertyMetadata(null, (d, e) => ((PropertyGrid)d).OnSelectedTypeChanged(e.NewValue as Type)));

    /// <summary>Identifies the <see cref="PropertySort"/> dependency property.</summary>
    public static readonly DependencyProperty PropertySortProperty = DependencyProperty.Register(
        nameof(PropertySort),
        typeof(PropertySort),
        typeof(PropertyGrid),
        new PropertyMetadata(
            PropertySort.CategorizedAlphabetical,
            (d, e) => ((PropertyGrid)d).source.Sort = (PropertySort)e.NewValue));

    /// <summary>Identifies the <see cref="NameColumnWidth"/> dependency property.</summary>
    public static readonly DependencyProperty NameColumnWidthProperty = DependencyProperty.Register(
        nameof(NameColumnWidth),
        typeof(double),
        typeof(PropertyGrid),
        new PropertyMetadata(160.0, (d, e) => ((PropertyGrid)d).OnNameColumnWidthChanged((double)e.NewValue)));

    /// <summary>Identifies the <see cref="MinimumNameColumnWidth"/> dependency property.</summary>
    public static readonly DependencyProperty MinimumNameColumnWidthProperty = DependencyProperty.Register(
        nameof(MinimumNameColumnWidth),
        typeof(double),
        typeof(PropertyGrid),
        new PropertyMetadata(48.0, (d, _) => ((PropertyGrid)d).ClampNameColumnWidth()));

    /// <summary>Identifies the <see cref="MinimumValueColumnWidth"/> dependency property.</summary>
    public static readonly DependencyProperty MinimumValueColumnWidthProperty = DependencyProperty.Register(
        nameof(MinimumValueColumnWidth),
        typeof(double),
        typeof(PropertyGrid),
        new PropertyMetadata(64.0, (d, _) => ((PropertyGrid)d).ClampNameColumnWidth()));

    /// <summary>Identifies the <see cref="CanResizeNameColumn"/> dependency property.</summary>
    public static readonly DependencyProperty CanResizeNameColumnProperty = DependencyProperty.Register(
        nameof(CanResizeNameColumn),
        typeof(bool),
        typeof(PropertyGrid),
        new PropertyMetadata(true, (d, _) => ((PropertyGrid)d).UpdateSplitter()));

    /// <summary>Identifies the <see cref="IndentSize"/> dependency property.</summary>
    public static readonly DependencyProperty IndentSizeProperty = DependencyProperty.Register(
        nameof(IndentSize),
        typeof(double),
        typeof(PropertyGrid),
        new PropertyMetadata(14.0));

    /// <summary>Identifies the <see cref="ShowDescription"/> dependency property.</summary>
    public static readonly DependencyProperty ShowDescriptionProperty = DependencyProperty.Register(
        nameof(ShowDescription),
        typeof(bool),
        typeof(PropertyGrid),
        new PropertyMetadata(true, (d, _) => ((PropertyGrid)d).UpdatePaneVisibility()));

    /// <summary>Identifies the <see cref="DescriptionHeight"/> dependency property.</summary>
    public static readonly DependencyProperty DescriptionHeightProperty = DependencyProperty.Register(
        nameof(DescriptionHeight),
        typeof(double),
        typeof(PropertyGrid),
        new PropertyMetadata(64.0, (d, e) => ((PropertyGrid)d).OnDescriptionHeightChanged((double)e.NewValue)));

    /// <summary>Identifies the <see cref="MinimumDescriptionHeight"/> dependency property.</summary>
    public static readonly DependencyProperty MinimumDescriptionHeightProperty = DependencyProperty.Register(
        nameof(MinimumDescriptionHeight),
        typeof(double),
        typeof(PropertyGrid),
        new PropertyMetadata(32.0, (d, _) => ((PropertyGrid)d).ClampDescriptionHeight()));

    /// <summary>Identifies the <see cref="MinimumRowsHeight"/> dependency property.</summary>
    public static readonly DependencyProperty MinimumRowsHeightProperty = DependencyProperty.Register(
        nameof(MinimumRowsHeight),
        typeof(double),
        typeof(PropertyGrid),
        new PropertyMetadata(64.0, (d, _) => ((PropertyGrid)d).ClampDescriptionHeight()));

    /// <summary>Identifies the <see cref="CanResizeDescription"/> dependency property.</summary>
    public static readonly DependencyProperty CanResizeDescriptionProperty = DependencyProperty.Register(
        nameof(CanResizeDescription),
        typeof(bool),
        typeof(PropertyGrid),
        new PropertyMetadata(true, (d, _) => ((PropertyGrid)d).UpdateDescriptionPane()));

    /// <summary>Identifies the <see cref="ShowSearchBox"/> dependency property.</summary>
    public static readonly DependencyProperty ShowSearchBoxProperty = DependencyProperty.Register(
        nameof(ShowSearchBox),
        typeof(bool),
        typeof(PropertyGrid),
        new PropertyMetadata(false, (d, _) => ((PropertyGrid)d).UpdatePaneVisibility()));

    /// <summary>Identifies the <see cref="IsReadOnly"/> dependency property.</summary>
    public static readonly DependencyProperty IsReadOnlyProperty = DependencyProperty.Register(
        nameof(IsReadOnly),
        typeof(bool),
        typeof(PropertyGrid),
        new PropertyMetadata(false, (d, e) => ((PropertyGrid)d).source.IsReadOnly = (bool)e.NewValue));

    /// <summary>Identifies the <see cref="FilterText"/> dependency property.</summary>
    public static readonly DependencyProperty FilterTextProperty = DependencyProperty.Register(
        nameof(FilterText),
        typeof(string),
        typeof(PropertyGrid),
        new PropertyMetadata(null, (d, e) => ((PropertyGrid)d).source.FilterText = e.NewValue as string));

    /// <summary>Identifies the <see cref="ExpansionPolicy"/> dependency property.</summary>
    public static readonly DependencyProperty ExpansionPolicyProperty = DependencyProperty.Register(
        nameof(ExpansionPolicy),
        typeof(PropertyExpansionPolicy),
        typeof(PropertyGrid),
        new PropertyMetadata(
            PropertyExpansionPolicy.Attributed,
            (d, e) => ((PropertyGrid)d).source.ExpansionPolicy = (PropertyExpansionPolicy)e.NewValue));

    /// <summary>Identifies the <see cref="MaximumExpansionDepth"/> dependency property.</summary>
    public static readonly DependencyProperty MaximumExpansionDepthProperty = DependencyProperty.Register(
        nameof(MaximumExpansionDepth),
        typeof(int),
        typeof(PropertyGrid),
        new PropertyMetadata(10, (d, e) => ((PropertyGrid)d).source.MaximumExpansionDepth = (int)e.NewValue));

    /// <summary>Identifies the <see cref="ValidationMode"/> dependency property.</summary>
    public static readonly DependencyProperty ValidationModeProperty = DependencyProperty.Register(
        nameof(ValidationMode),
        typeof(PropertyGridValidationMode),
        typeof(PropertyGrid),
        new PropertyMetadata(
            PropertyGridValidationMode.All,
            (d, e) => ((PropertyGrid)d).source.ValidationMode = (PropertyGridValidationMode)e.NewValue));

    /// <summary>Identifies the <see cref="EditorTemplates"/> dependency property.</summary>
    public static readonly DependencyProperty EditorTemplatesProperty = DependencyProperty.Register(
        nameof(EditorTemplates),
        typeof(PropertyEditorTemplateMap),
        typeof(PropertyGrid),
        new PropertyMetadata(null, (d, e) => ((PropertyGrid)d).OnEditorTemplatesChanged(e.NewValue as PropertyEditorTemplateMap)));

    /// <summary>Identifies the <see cref="Metadata"/> dependency property.</summary>
    public static readonly DependencyProperty MetadataProperty = DependencyProperty.Register(
        nameof(Metadata),
        typeof(PropertyGridMetadata),
        typeof(PropertyGrid),
        new PropertyMetadata(null, (d, e) => ((PropertyGrid)d).OnMetadataChanged(e.NewValue as PropertyGridMetadata)));

    /// <summary>Identifies the <see cref="DescriptionProvider"/> dependency property.</summary>
    public static readonly DependencyProperty DescriptionProviderProperty = DependencyProperty.Register(
        nameof(DescriptionProvider),
        typeof(IPropertyDescriptionProvider),
        typeof(PropertyGrid),
        new PropertyMetadata(null, (d, e) => ((PropertyGrid)d).OnDescriptionProviderChanged(e.NewValue as IPropertyDescriptionProvider)));

    /// <summary>Identifies the <see cref="Culture"/> dependency property.</summary>
    public static readonly DependencyProperty CultureProperty = DependencyProperty.Register(
        nameof(Culture),
        typeof(CultureInfo),
        typeof(PropertyGrid),
        new PropertyMetadata(null, (d, e) => ((PropertyGrid)d).source.Culture = e.NewValue as CultureInfo ?? CultureInfo.CurrentCulture));

    /// <summary>Identifies the <see cref="DefaultCategoryName"/> dependency property.</summary>
    public static readonly DependencyProperty DefaultCategoryNameProperty = DependencyProperty.Register(
        nameof(DefaultCategoryName),
        typeof(string),
        typeof(PropertyGrid),
        new PropertyMetadata(null, (d, e) => ((PropertyGrid)d).source.DefaultCategoryName = e.NewValue as string ?? PropertyGridStrings.DefaultCategoryName));

    /// <summary>Identifies the <see cref="SelectedRow"/> dependency property.</summary>
    public static readonly DependencyProperty SelectedRowProperty = DependencyProperty.Register(
        nameof(SelectedRow),
        typeof(PropertyGridPropertyRow),
        typeof(PropertyGrid),
        new PropertyMetadata(null, (d, _) => ((PropertyGrid)d).OnSelectedRowChanged()));

    private readonly PropertyGridSource source = new();
    private readonly PropertyEditorSelector editorSelector = new();

    private ScrollViewer? scrollViewer;
    private ItemsRepeater? repeater;
    private PropertyGridSplitter? splitter;
    private PropertyGridSplitter? descriptionSplitter;
    private PropertyGridDescriptionPane? descriptionPane;
    private AutoSuggestBox? searchBox;

    /// <summary>Initializes a new instance of the <see cref="PropertyGrid"/> class.</summary>
    public PropertyGrid()
    {
        DefaultStyleKey = typeof(PropertyGrid);
        DefaultStyleResourceUri = new Uri("ms-appx:///Digi21.WinUI.PropertyGrid/Themes/Generic.xaml");
        PropertyGridThemeResources.Ensure();

        editorSelector.Owner = this;
        source.Culture = CultureInfo.CurrentCulture;
        source.RowsChanged = OnRowsChanged;
        source.ValueChanging = OnSourceValueChanging;
        source.ValueChanged = OnSourceValueChanged;
        source.DescriptionFilter = OnAutoGeneratingProperty;

        SizeChanged += (_, _) =>
        {
            ClampNameColumnWidth();
            ClampDescriptionHeight();
        };
    }

    /// <summary>Raised for each property as the rows are built, to change or drop it.</summary>
    /// <remarks>
    /// The least ceremonious way to adjust a type you do not own. For a rule you want everywhere,
    /// <see cref="PropertyGridMetadata"/> says the same things declaratively.
    /// </remarks>
    public event EventHandler<AutoGeneratingPropertyEventArgs>? AutoGeneratingProperty;

    /// <summary>Raised before a row picks its editor, to supply a different one.</summary>
    public event EventHandler<PropertyEditorSelectingEventArgs>? EditorSelecting;

    /// <summary>Raised before a value is written, with a chance to refuse it.</summary>
    public event EventHandler<PropertyValueChangingEventArgs>? PropertyValueChanging;

    /// <summary>Raised after a value has been written.</summary>
    public event EventHandler<PropertyValueChangedEventArgs>? PropertyValueChanged;

    /// <summary>Raised when the user asks to browse for a path.</summary>
    /// <remarks>
    /// The grid never opens a file dialog itself: which one, where it starts and how it is filtered
    /// are the application's business, and in WinUI 3 a picker needs the window handle, which a
    /// control has no dependable way to reach. Handle this, open whatever is right, and write
    /// <c>Row.Value</c> when the answer arrives — the handler is free to be asynchronous.
    /// </remarks>
    public event EventHandler<PropertyGridBrowseRequestedEventArgs>? BrowseRequested;

    /// <summary>Raised when the user asks to edit a value that needs more room than a row.</summary>
    /// <remarks>
    /// Lists and complex objects offer this by default, and any property can ask for it with
    /// <c>[PropertyEditor(PropertyEditorKeys.Dialog)]</c>. The button does not appear at all unless
    /// something is handling this, so a grid that offers no dialogs shows no dead buttons.
    /// </remarks>
    public event EventHandler<PropertyGridEditRequestedEventArgs>? EditRequested;

    /// <summary>Gets or sets the object whose properties are shown.</summary>
    public object? SelectedObject
    {
        get => GetValue(SelectedObjectProperty);
        set => SetValue(SelectedObjectProperty, value);
    }

    /// <summary>Gets or sets a type to show the shape of, with no object behind it.</summary>
    /// <remarks>Every row is read-only: there is nothing to read a value from. Ignored while <see cref="SelectedObject"/> is set.</remarks>
    public Type? SelectedType
    {
        get => (Type?)GetValue(SelectedTypeProperty);
        set => SetValue(SelectedTypeProperty, value);
    }

    /// <summary>Gets or sets how the rows are ordered and grouped.</summary>
    public PropertySort PropertySort
    {
        get => (PropertySort)GetValue(PropertySortProperty);
        set => SetValue(PropertySortProperty, value);
    }

    /// <summary>Gets or sets the width of the name column, which the user can drag.</summary>
    public double NameColumnWidth
    {
        get => (double)GetValue(NameColumnWidthProperty);
        set => SetValue(NameColumnWidthProperty, value);
    }

    /// <summary>Gets or sets how narrow the name column can be dragged.</summary>
    public double MinimumNameColumnWidth
    {
        get => (double)GetValue(MinimumNameColumnWidthProperty);
        set => SetValue(MinimumNameColumnWidthProperty, value);
    }

    /// <summary>Gets or sets how narrow the value column can be squeezed.</summary>
    public double MinimumValueColumnWidth
    {
        get => (double)GetValue(MinimumValueColumnWidthProperty);
        set => SetValue(MinimumValueColumnWidthProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the divider between the columns can be dragged.</summary>
    public bool CanResizeNameColumn
    {
        get => (bool)GetValue(CanResizeNameColumnProperty);
        set => SetValue(CanResizeNameColumnProperty, value);
    }

    /// <summary>Gets or sets how far each level of nesting pushes a name in.</summary>
    public double IndentSize
    {
        get => (double)GetValue(IndentSizeProperty);
        set => SetValue(IndentSizeProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the pane explaining the selected property is shown.</summary>
    public bool ShowDescription
    {
        get => (bool)GetValue(ShowDescriptionProperty);
        set => SetValue(ShowDescriptionProperty, value);
    }

    /// <summary>Gets or sets the height of the description pane, which the user can drag.</summary>
    /// <remarks>
    /// An explanation too long for the height it is given scrolls inside the pane rather than being
    /// cut off, so there is no height at which part of a sentence becomes unreachable.
    /// </remarks>
    public double DescriptionHeight
    {
        get => (double)GetValue(DescriptionHeightProperty);
        set => SetValue(DescriptionHeightProperty, value);
    }

    /// <summary>Gets or sets how short the description pane can be dragged.</summary>
    public double MinimumDescriptionHeight
    {
        get => (double)GetValue(MinimumDescriptionHeightProperty);
        set => SetValue(MinimumDescriptionHeightProperty, value);
    }

    /// <summary>Gets or sets how short the list of rows can be squeezed by dragging the description pane taller.</summary>
    public double MinimumRowsHeight
    {
        get => (double)GetValue(MinimumRowsHeightProperty);
        set => SetValue(MinimumRowsHeightProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the divider above the description pane can be dragged.</summary>
    public bool CanResizeDescription
    {
        get => (bool)GetValue(CanResizeDescriptionProperty);
        set => SetValue(CanResizeDescriptionProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether the grid shows its own search box.</summary>
    public bool ShowSearchBox
    {
        get => (bool)GetValue(ShowSearchBoxProperty);
        set => SetValue(ShowSearchBoxProperty, value);
    }

    /// <summary>Gets or sets a value indicating whether every row refuses to be edited.</summary>
    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>Gets or sets text every shown property has to contain in its name or its explanation.</summary>
    public string? FilterText
    {
        get => (string?)GetValue(FilterTextProperty);
        set => SetValue(FilterTextProperty, value);
    }

    /// <summary>Gets or sets which properties can be opened into child rows.</summary>
    public PropertyExpansionPolicy ExpansionPolicy
    {
        get => (PropertyExpansionPolicy)GetValue(ExpansionPolicyProperty);
        set => SetValue(ExpansionPolicyProperty, value);
    }

    /// <summary>Gets or sets how deeply the grid follows an object graph before it stops offering to go further.</summary>
    public int MaximumExpansionDepth
    {
        get => (int)GetValue(MaximumExpansionDepthProperty);
        set => SetValue(MaximumExpansionDepthProperty, value);
    }

    /// <summary>Gets or sets which layers of validation run when a value is edited.</summary>
    public PropertyGridValidationMode ValidationMode
    {
        get => (PropertyGridValidationMode)GetValue(ValidationModeProperty);
        set => SetValue(ValidationModeProperty, value);
    }

    /// <summary>Gets or sets the editors registered for this grid, searched before the application-wide ones.</summary>
    public PropertyEditorTemplateMap? EditorTemplates
    {
        get => (PropertyEditorTemplateMap?)GetValue(EditorTemplatesProperty);
        set => SetValue(EditorTemplatesProperty, value);
    }

    /// <summary>Gets or sets the store describing types this grid shows, defaulting to the shared one.</summary>
    public PropertyGridMetadata? Metadata
    {
        get => (PropertyGridMetadata?)GetValue(MetadataProperty);
        set => SetValue(MetadataProperty, value);
    }

    /// <summary>Gets or sets how properties are discovered, replacing reflection entirely.</summary>
    public IPropertyDescriptionProvider? DescriptionProvider
    {
        get => (IPropertyDescriptionProvider?)GetValue(DescriptionProviderProperty);
        set => SetValue(DescriptionProviderProperty, value);
    }

    /// <summary>Gets or sets the culture values are shown and parsed in.</summary>
    /// <remarks>
    /// Defaults to the culture of the thread. An application that chooses its own language rather
    /// than following Windows has to say so here as well, or the grid formats dates and decimal
    /// separators differently from everything around it.
    /// </remarks>
    public CultureInfo? Culture
    {
        get => (CultureInfo?)GetValue(CultureProperty);
        set => SetValue(CultureProperty, value);
    }

    /// <summary>Gets or sets the category properties land in when they do not name one.</summary>
    /// <remarks>Shown to the user, so it needs translating along with everything else.</remarks>
    public string? DefaultCategoryName
    {
        get => (string?)GetValue(DefaultCategoryNameProperty);
        set => SetValue(DefaultCategoryNameProperty, value);
    }

    /// <summary>Gets or sets the row the description pane is explaining.</summary>
    public PropertyGridPropertyRow? SelectedRow
    {
        get => (PropertyGridPropertyRow?)GetValue(SelectedRowProperty);
        set => SetValue(SelectedRowProperty, value);
    }

    /// <summary>Gets the validators consulted before a value is written.</summary>
    public IList<IPropertyValidator> Validators => source.Validators;

    /// <summary>Gets the rows the grid is showing, headers and properties together.</summary>
    public IReadOnlyList<PropertyGridRow> Rows => source.Rows;

    /// <summary>Gets the categories the properties were grouped into, whether or not they are open.</summary>
    public IReadOnlyList<PropertyGridCategoryRow> Categories => source.Categories;

    internal PropertyEditorSelector EditorSelector => editorSelector;

    /// <summary>Reads every value again and rebuilds the rows.</summary>
    public void Refresh() => source.Refresh();

    /// <summary>Finds a row by the name of the property it edits.</summary>
    /// <param name="path">The name of the property, or a path such as <c>Address.City</c>.</param>
    /// <returns>The row, or <see langword="null"/> if the grid is not showing it.</returns>
    public PropertyGridPropertyRow? FindRow(string path) => source.FindRow(path);

    /// <summary>Selects a property, so the description pane explains it.</summary>
    /// <param name="path">The name of the property, or a path such as <c>Address.City</c>.</param>
    /// <returns><see langword="true"/> if the grid was showing it.</returns>
    public bool SelectProperty(string path)
    {
        if (source.FindRow(path) is not { } row)
        {
            return false;
        }

        SelectedRow = row;
        return true;
    }

    /// <summary>Opens every category header.</summary>
    public void ExpandAllCategories() => source.ExpandAllCategories();

    /// <summary>Closes every category header.</summary>
    public void CollapseAllCategories() => source.CollapseAllCategories();

    /// <summary>Closes every property that was opened into child rows.</summary>
    public void CollapseAll() => source.CollapseAll();

    /// <summary>Widens or narrows the name column to fit the names it is currently showing.</summary>
    /// <remarks>
    /// Only the realized rows can answer, which is the same "fit what you can see" the shells do —
    /// measuring a thousand names to size a column showing thirty of them would cost more than it
    /// is worth.
    /// </remarks>
    public void AutoSizeNameColumn()
    {
        if (repeater is null)
        {
            return;
        }

        double widest = 0;
        for (int index = 0; index < source.Rows.Count; index++)
        {
            if (repeater.TryGetElement(index) is PropertyGridRowPresenter presenter)
            {
                widest = Math.Max(widest, presenter.NaturalNameWidth);
            }
        }

        if (widest > 0)
        {
            NameColumnWidth = Math.Ceiling(widest) + PropertyGridThemeResources.Value("PropertyGridSplitterThickness", 6.0);
        }
    }

    /// <summary>Grows or shrinks the description pane to fit the explanation it is showing.</summary>
    /// <remarks>What double-clicking the divider above the pane does, and the counterpart of
    /// <see cref="AutoSizeNameColumn"/>. A pane showing nothing keeps the height it has: there is
    /// nothing to fit to, and collapsing to a sliver on an empty selection helps nobody.</remarks>
    public void AutoSizeDescription()
    {
        if (descriptionPane?.NaturalHeight is not > 0)
        {
            return;
        }

        DescriptionHeight = Math.Ceiling(descriptionPane.NaturalHeight);
    }

    /// <summary>Scrolls a row into view, realizing it first if the list had virtualized it away.</summary>
    /// <param name="row">The row to show.</param>
    public void ScrollIntoView(PropertyGridRow row)
    {
        ArgumentNullException.ThrowIfNull(row);

        if (repeater is null)
        {
            return;
        }

        int index = IndexOf(row);
        if (index < 0)
        {
            return;
        }

        // A row that has been scrolled past does not exist as an element, so asking the repeater to
        // create it - and laying it out - has to happen before there is anything to bring into view.
        UIElement element = repeater.GetOrCreateElement(index);
        element.UpdateLayout();
        element.StartBringIntoView();
    }

    internal void MoveSelection(PropertyGridRow from, int delta)
    {
        int index = IndexOf(from);
        if (index < 0)
        {
            return;
        }

        MoveSelectionTo(Math.Clamp(index + delta, 0, source.Rows.Count - 1));
    }

    internal void MoveSelectionTo(int index)
    {
        if (index < 0 || index >= source.Rows.Count || repeater is null)
        {
            return;
        }

        PropertyGridRow target = source.Rows[index];

        if (target is PropertyGridPropertyRow property)
        {
            SelectedRow = property;
        }

        ScrollIntoView(target);

        if (repeater.TryGetElement(index) is Control focusable)
        {
            focusable.Focus(FocusState.Keyboard);
        }
    }

    internal int LastRowIndex => source.Rows.Count - 1;

    /// <summary>Forgets the editors it resolved, so every row picks one again.</summary>
    /// <remarks>Call this after registering editors, or after replacing an editor template in the application's resources.</remarks>
    public void InvalidateEditors()
    {
        editorSelector.Invalidate();
        source.Refresh();
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (repeater is not null)
        {
            repeater.ElementPrepared -= OnElementPrepared;
            repeater.ElementClearing -= OnElementClearing;
        }

        if (searchBox is not null)
        {
            searchBox.TextChanged -= OnSearchTextChanged;
        }

        scrollViewer = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;
        repeater = GetTemplateChild("PART_ItemsRepeater") as ItemsRepeater;
        splitter = GetTemplateChild("PART_Splitter") as PropertyGridSplitter;
        descriptionSplitter = GetTemplateChild("PART_DescriptionSplitter") as PropertyGridSplitter;
        descriptionPane = GetTemplateChild("PART_DescriptionPane") as PropertyGridDescriptionPane;
        searchBox = GetTemplateChild("PART_SearchBox") as AutoSuggestBox;

        if (searchBox is not null)
        {
            searchBox.Text = FilterText ?? string.Empty;
            searchBox.TextChanged += OnSearchTextChanged;
        }

        if (scrollViewer is not null)
        {
            // The splitter is drawn over the scrolling area rather than inside it, so the two only
            // stay lined up while the content cannot scroll sideways. Set here as well as in the
            // template, so that retemplating the grid cannot silently break the alignment.
            scrollViewer.HorizontalScrollMode = ScrollMode.Disabled;
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
        }

        if (repeater is not null)
        {
            repeater.ItemsSource = source.Rows;
            repeater.ElementPrepared += OnElementPrepared;
            repeater.ElementClearing += OnElementClearing;
        }

        if (splitter is not null)
        {
            splitter.Owner = this;
        }

        if (descriptionSplitter is not null)
        {
            descriptionSplitter.Owner = this;
        }

        UpdateSplitter();
        UpdatePaneVisibility();

        if (descriptionPane is not null)
        {
            descriptionPane.Row = SelectedRow;
        }
    }

    private void UpdatePaneVisibility()
    {
        if (descriptionPane is not null)
        {
            descriptionPane.Visibility = ShowDescription ? Visibility.Visible : Visibility.Collapsed;
        }

        if (searchBox is not null)
        {
            searchBox.Visibility = ShowSearchBox ? Visibility.Visible : Visibility.Collapsed;
        }

        UpdateDescriptionPane();
    }

    private void UpdateDescriptionPane()
    {
        if (descriptionPane is not null)
        {
            // A height rather than a MinHeight: once the divider is there the pane has to shrink as
            // well as grow, and a MinHeight would quietly refuse the bottom half of the drag.
            descriptionPane.Height = DescriptionHeight;
        }

        if (descriptionSplitter is null)
        {
            return;
        }

        descriptionSplitter.Visibility = ShowDescription && CanResizeDescription
            ? Visibility.Visible
            : Visibility.Collapsed;

        // Sized and placed from code for the same reason as the column splitter, and pulled up by
        // half its thickness so the band to grab straddles the line the pane draws along its top
        // edge instead of adding a second line just below it.
        double thickness = PropertyGridThemeResources.Value("PropertyGridSplitterThickness", 6.0);
        descriptionSplitter.Height = thickness;
        descriptionSplitter.Margin = new Thickness(0, -thickness / 2, 0, 0);
    }

    private void OnSearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs arguments)
    {
        if (arguments.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            FilterText = sender.Text;
        }
    }

    // Whether an editor should offer its button at all. An affordance that does nothing when pressed
    // is worse than no affordance, and lists get theirs without anybody asking for it.
    internal bool CanBrowse => BrowseRequested is not null;

    internal bool CanEdit => EditRequested is not null;

    internal void RaiseBrowseRequested(PropertyGridPropertyRow row, FilePathKind kind, IReadOnlyList<string> extensions) =>
        BrowseRequested?.Invoke(this, new PropertyGridBrowseRequestedEventArgs(row, kind, extensions));

    internal void RaiseEditRequested(PropertyGridPropertyRow row) =>
        EditRequested?.Invoke(this, new PropertyGridEditRequestedEventArgs(row));

    internal Microsoft.UI.Xaml.DataTemplate? RaiseEditorSelecting(PropertyGridPropertyRow row)
    {
        if (EditorSelecting is null)
        {
            return null;
        }

        PropertyEditorSelectingEventArgs arguments = new(row);
        EditorSelecting(this, arguments);
        return arguments.Template;
    }

    private void OnSelectedObjectChanged(object? value)
    {
        if (value is null && SelectedType is { } type)
        {
            source.SetTargetType(type);
        }
        else
        {
            source.SetTarget(value);
        }

        SelectedRow = null;
    }

    private void OnSelectedTypeChanged(Type? type)
    {
        if (SelectedObject is null)
        {
            source.SetTargetType(type);
            SelectedRow = null;
        }
    }

    private void OnEditorTemplatesChanged(PropertyEditorTemplateMap? map)
    {
        editorSelector.EditorTemplates = map;
        editorSelector.Invalidate();
    }

    private void OnMetadataChanged(PropertyGridMetadata? metadata)
    {
        // Replacing the store means the descriptions built from the old one are stale, and the
        // provider caches per store rather than per grid.
        source.Provider = DescriptionProvider
            ?? (metadata is null
                ? ReflectionPropertyDescriptionProvider.Default
                : new ReflectionPropertyDescriptionProvider(metadata));
    }

    private void OnDescriptionProviderChanged(IPropertyDescriptionProvider? provider)
    {
        source.Provider = provider
            ?? (Metadata is null
                ? ReflectionPropertyDescriptionProvider.Default
                : new ReflectionPropertyDescriptionProvider(Metadata));
    }

    private void OnNameColumnWidthChanged(double value)
    {
        double clamped = Clamp(value);
        if (Math.Abs(clamped - value) > 0.5)
        {
            NameColumnWidth = clamped;
            return;
        }

        UpdateSplitter();
    }

    private void ClampNameColumnWidth()
    {
        double clamped = Clamp(NameColumnWidth);
        if (Math.Abs(clamped - NameColumnWidth) > 0.5)
        {
            NameColumnWidth = clamped;
        }
        else
        {
            UpdateSplitter();
        }
    }

    private double Clamp(double width)
    {
        double available = scrollViewer?.ViewportWidth is > 0 and double viewport ? viewport : ActualWidth;
        if (available <= 0)
        {
            // Nothing has been measured yet, so there is no upper bound to clamp against; the
            // SizeChanged handler comes back once there is.
            return Math.Max(MinimumNameColumnWidth, width);
        }

        double widest = Math.Max(MinimumNameColumnWidth, available - MinimumValueColumnWidth);
        return Math.Clamp(width, MinimumNameColumnWidth, widest);
    }

    private void OnDescriptionHeightChanged(double value)
    {
        double clamped = ClampDescription(value);
        if (Math.Abs(clamped - value) > 0.5)
        {
            DescriptionHeight = clamped;
            return;
        }

        UpdateDescriptionPane();
    }

    private void ClampDescriptionHeight()
    {
        double clamped = ClampDescription(DescriptionHeight);
        if (Math.Abs(clamped - DescriptionHeight) > 0.5)
        {
            DescriptionHeight = clamped;
        }
        else
        {
            UpdateDescriptionPane();
        }
    }

    private double ClampDescription(double height)
    {
        if (ActualHeight <= 0)
        {
            // Nothing has been measured yet, so there is no upper bound to clamp against; the
            // SizeChanged handler comes back once there is.
            return Math.Max(MinimumDescriptionHeight, height);
        }

        double tallest = Math.Max(MinimumDescriptionHeight, ActualHeight - MinimumRowsHeight);
        return Math.Clamp(height, MinimumDescriptionHeight, tallest);
    }

    private void UpdateSplitter()
    {
        if (splitter is null)
        {
            return;
        }

        splitter.Visibility = CanResizeNameColumn ? Visibility.Visible : Visibility.Collapsed;

        // Positioned from code rather than bound: binding a thickness to a number needs a converter
        // for something this callback has already worked out.
        double thickness = PropertyGridThemeResources.Value("PropertyGridSplitterThickness", 6.0);
        splitter.Width = thickness;
        splitter.Margin = new Thickness(NameColumnWidth, 0, 0, 0);
    }

    private void OnElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs arguments)
    {
        // Assigned before the element is first measured, so a row scrolling into view is laid out at
        // the right column width on its first frame instead of jumping on the second.
        switch (arguments.Element)
        {
            case PropertyGridRowPresenter presenter:
                presenter.Owner = this;
                presenter.Row = source.Rows[arguments.Index] as PropertyGridPropertyRow;
                break;

            case PropertyGridCategoryHeader header:
                header.Row = source.Rows[arguments.Index] as PropertyGridCategoryRow;
                break;
        }
    }

    private void OnElementClearing(ItemsRepeater sender, ItemsRepeaterElementClearingEventArgs arguments)
    {
        switch (arguments.Element)
        {
            case PropertyGridRowPresenter presenter:
                presenter.PrepareForRecycling();
                break;

            case PropertyGridCategoryHeader header:
                header.PrepareForRecycling();
                break;
        }
    }

    private void OnRowsChanged()
    {
        if (SelectedRow is { } selected && !source.Rows.Contains(selected))
        {
            SelectedRow = null;
        }
    }

    private int IndexOf(PropertyGridRow row)
    {
        for (int index = 0; index < source.Rows.Count; index++)
        {
            if (ReferenceEquals(source.Rows[index], row))
            {
                return index;
            }
        }

        return -1;
    }

    private void OnSelectedRowChanged()
    {
        // The rows themselves notice: each presenter watches this property on its owner, the same
        // way it watches the column width, so nothing here has to go looking for them.
        if (descriptionPane is not null)
        {
            descriptionPane.Row = SelectedRow;
        }
    }

    private PropertyDescription? OnAutoGeneratingProperty(PropertyDescription description)
    {
        if (AutoGeneratingProperty is null)
        {
            return description;
        }

        AutoGeneratingPropertyEventArgs arguments = new(description);
        AutoGeneratingProperty(this, arguments);
        return arguments.Cancel ? null : arguments.ToDescription();
    }

    private string? OnSourceValueChanging(PropertyGridPropertyRow row, object? oldValue, object? newValue)
    {
        if (PropertyValueChanging is null)
        {
            return null;
        }

        PropertyValueChangingEventArgs arguments = new(row, oldValue, newValue);
        PropertyValueChanging(this, arguments);
        return arguments.Cancel ? arguments.ErrorMessage ?? string.Empty : null;
    }

    private void OnSourceValueChanged(PropertyGridPropertyRow row, object? oldValue, object? newValue) =>
        PropertyValueChanged?.Invoke(this, new PropertyValueChangedEventArgs(row, oldValue, newValue));
}
