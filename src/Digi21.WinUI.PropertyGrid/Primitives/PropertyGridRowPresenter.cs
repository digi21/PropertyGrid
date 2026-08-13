using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;

namespace Digi21.WinUI.PropertyGrid.Primitives;

/// <summary>Renders one property as a row: its name, the gutter, and the editor for its value.</summary>
/// <remarks>
/// The chrome of the row is not negotiable — the indent, the expander, the name, the gutter and the
/// error badge have to be identical in every row or the splitter stops reading as a column. What a
/// consumer replaces is the editor inside the value cell, which is the right amount of rope.
/// </remarks>
public partial class PropertyGridRowPresenter : Control
{
    /// <summary>Identifies the <see cref="Row"/> dependency property.</summary>
    public static readonly DependencyProperty RowProperty = DependencyProperty.Register(
        nameof(Row),
        typeof(PropertyGridPropertyRow),
        typeof(PropertyGridRowPresenter),
        new PropertyMetadata(null, (d, _) => ((PropertyGridRowPresenter)d).OnRowChanged()));

    private PropertyGridRowPanel? rowPanel;
    private ContentPresenter? editorHost;
    private PropertyGrid? observed;
    private PropertyGridPropertyRow? subscribed;
    private long nameWidthToken;
    private long indentToken;
    private long selectionToken;
    private bool isPointerOver;

    /// <summary>Initializes a new instance of the <see cref="PropertyGridRowPresenter"/> class.</summary>
    public PropertyGridRowPresenter()
    {
        DefaultStyleKey = typeof(PropertyGridRowPresenter);
        DefaultStyleResourceUri = new Uri("ms-appx:///Digi21.WinUI.PropertyGrid/Themes/Generic.xaml");
        PropertyGridThemeResources.Ensure();

        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    /// <summary>Gets or sets the property the row shows.</summary>
    public PropertyGridPropertyRow? Row
    {
        get => (PropertyGridPropertyRow?)GetValue(RowProperty);
        set => SetValue(RowProperty, value);
    }

    /// <summary>Gets the width the name cell would need to show its contents in full.</summary>
    public double NaturalNameWidth => rowPanel?.NaturalNameWidth ?? 0;

    internal PropertyGrid? Owner
    {
        get => observed;
        set
        {
            if (ReferenceEquals(observed, value))
            {
                return;
            }

            StopObserving();
            observed = value;
            StartObserving();
            ApplyColumns();
            ApplyEditor();
        }
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        rowPanel = GetTemplateChild("PART_RowPanel") as PropertyGridRowPanel;
        editorHost = GetTemplateChild("PART_EditorHost") as ContentPresenter;

        if (GetTemplateChild("PART_Expander") is ToggleButton expander)
        {
            expander.Checked += OnExpanderToggled;
            expander.Unchecked += OnExpanderToggled;
        }

        ApplyColumns();
        ApplyEditor();
        UpdateStates(false);
    }

    /// <inheritdoc />
    protected override void OnPointerEntered(PointerRoutedEventArgs e)
    {
        base.OnPointerEntered(e);
        isPointerOver = true;
        UpdateStates(true);
    }

    /// <inheritdoc />
    protected override void OnPointerExited(PointerRoutedEventArgs e)
    {
        base.OnPointerExited(e);
        isPointerOver = false;
        UpdateStates(true);
    }

    /// <inheritdoc />
    protected override void OnPointerCanceled(PointerRoutedEventArgs e)
    {
        base.OnPointerCanceled(e);
        isPointerOver = false;
        UpdateStates(true);
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerRoutedEventArgs e)
    {
        base.OnPointerPressed(e);
        Select();
    }

    /// <inheritdoc />
    protected override void OnGotFocus(RoutedEventArgs e)
    {
        base.OnGotFocus(e);

        // Tabbing into the editor inside the row counts as selecting the row, which is what keeps
        // the description pane following the keyboard as well as the pointer.
        Select();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // The repeater assigns the owner before the row is first measured, so this only matters for
        // a replaced row template whose root is something else - a Border wrapping the presenter,
        // say - which the repeater hands us instead.
        Owner ??= this.FindAncestor<PropertyGrid>();
        UpdateStates(false);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e) => StopObserving();

    private void OnRowChanged()
    {
        if (subscribed is not null)
        {
            subscribed.PropertyChanged -= OnRowPropertyChanged;
        }

        subscribed = Row;

        if (subscribed is not null)
        {
            subscribed.PropertyChanged += OnRowPropertyChanged;
        }

        ApplyColumns();
        ApplyEditor();
        UpdateStates(false);
    }

    private void OnRowPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(PropertyGridPropertyRow.IsExpanded):
            case nameof(PropertyGridPropertyRow.IsExpandable):
            case nameof(PropertyGridPropertyRow.HasErrors):
            case nameof(PropertyGridPropertyRow.IsModified):
                UpdateStates(true);
                break;

            case nameof(PropertyGridPropertyRow.RuntimeType):
                // The declared type may be vague enough that the editor depends on what is in the
                // property right now, so replacing the value can change which editor it needs.
                ApplyEditor();
                break;
        }
    }

    private void StartObserving()
    {
        if (observed is null)
        {
            return;
        }

        nameWidthToken = observed.RegisterPropertyChangedCallback(PropertyGrid.NameColumnWidthProperty, (_, _) => ApplyColumns());
        indentToken = observed.RegisterPropertyChangedCallback(PropertyGrid.IndentSizeProperty, (_, _) => ApplyColumns());
        selectionToken = observed.RegisterPropertyChangedCallback(PropertyGrid.SelectedRowProperty, (_, _) => UpdateStates(true));
    }

    private void StopObserving()
    {
        if (observed is null)
        {
            return;
        }

        observed.UnregisterPropertyChangedCallback(PropertyGrid.NameColumnWidthProperty, nameWidthToken);
        observed.UnregisterPropertyChangedCallback(PropertyGrid.IndentSizeProperty, indentToken);
        observed.UnregisterPropertyChangedCallback(PropertyGrid.SelectedRowProperty, selectionToken);
    }

    private void ApplyColumns()
    {
        if (rowPanel is null)
        {
            // The template is not applied yet; OnApplyTemplate calls this again.
            return;
        }

        rowPanel.NameWidth = observed?.NameColumnWidth ?? 160.0;
        rowPanel.GutterWidth = PropertyGridThemeResources.Value("PropertyGridSplitterThickness", 6.0);
        rowPanel.Indent = (Row?.Depth ?? 0) * (observed?.IndentSize ?? 14.0);
    }

    private void ApplyEditor()
    {
        if (editorHost is null)
        {
            return;
        }

        if (Row is null)
        {
            editorHost.Content = null;
            return;
        }

        // The template is resolved here and assigned only when it actually changes, rather than
        // handing the presenter a selector. Handing it a selector makes it re-run the selection and
        // rebuild its child on every content change; assigning a stable template gives it the chance
        // to keep what it built, which is the whole point of recycling the row.
        DataTemplate? template = observed?.EditorSelector.SelectTemplate(Row) as DataTemplate;
        if (!ReferenceEquals(editorHost.ContentTemplate, template))
        {
            editorHost.ContentTemplate = template;
        }

        editorHost.Content = Row;
    }

    private void Select()
    {
        if (observed is { } owner && Row is { } row)
        {
            owner.SelectedRow = row;
        }
    }

    private void OnExpanderToggled(object sender, RoutedEventArgs e)
    {
        if (Row is { } row && sender is ToggleButton expander)
        {
            row.IsExpanded = expander.IsChecked == true;
        }
    }

    private void UpdateStates(bool useTransitions)
    {
        bool selected = observed is { } owner && ReferenceEquals(owner.SelectedRow, Row);

        VisualStateManager.GoToState(this, selected ? "Selected" : isPointerOver ? "PointerOver" : "Normal", useTransitions);
        VisualStateManager.GoToState(this, Row?.HasErrors == true ? "Invalid" : "Valid", useTransitions);
        VisualStateManager.GoToState(
            this,
            Row is not { IsExpandable: true } ? "NotExpandable" : Row.IsExpanded ? "Expanded" : "Collapsed",
            useTransitions);

        if (GetTemplateChild("PART_Expander") is ToggleButton expander && Row is { } row)
        {
            expander.IsChecked = row.IsExpanded;
        }
    }

    // Rows are recycled onto other properties as the list scrolls, so anything the presenter is
    // remembering about the old one has to be cleared with it.
    internal void PrepareForRecycling()
    {
        isPointerOver = false;
        Row = null;
        Owner = null;
    }
}
