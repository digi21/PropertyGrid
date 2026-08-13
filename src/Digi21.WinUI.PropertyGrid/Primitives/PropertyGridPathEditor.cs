using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Digi21.WinUI.PropertyGrid.Primitives;

/// <summary>A box holding a path, with a button beside it that asks the application to browse.</summary>
/// <remarks>
/// The button raises <see cref="PropertyGrid.BrowseRequested"/> and does nothing else. The grid
/// never opens a dialog: which one to open, where it starts and how it is filtered are the
/// application's business, and in WinUI 3 a picker needs the window handle, which a control has no
/// dependable way to reach.
/// </remarks>
public partial class PropertyGridPathEditor : Control
{
    /// <summary>Identifies the <see cref="Row"/> dependency property.</summary>
    public static readonly DependencyProperty RowProperty = DependencyProperty.Register(
        nameof(Row),
        typeof(PropertyGridPropertyRow),
        typeof(PropertyGridPathEditor),
        new PropertyMetadata(null));

    private Button? browse;

    /// <summary>Initializes a new instance of the <see cref="PropertyGridPathEditor"/> class.</summary>
    public PropertyGridPathEditor()
    {
        DefaultStyleKey = typeof(PropertyGridPathEditor);
        DefaultStyleResourceUri = new Uri("ms-appx:///Digi21.WinUI.PropertyGrid/Themes/Generic.xaml");
        PropertyGridThemeResources.Ensure();

        // From the data context rather than bound in the editor template: the template's context is
        // already the row, so binding it back in is ceremony that can silently do nothing.
        DataContextChanged += (_, arguments) =>
        {
            if (arguments.NewValue is PropertyGridPropertyRow row)
            {
                Row = row;
            }
        };

        Loaded += (_, _) => UpdateButton();
    }

    /// <summary>Gets or sets the property being edited.</summary>
    public PropertyGridPropertyRow? Row
    {
        get => (PropertyGridPropertyRow?)GetValue(RowProperty);
        set => SetValue(RowProperty, value);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (browse is not null)
        {
            browse.Click -= OnBrowse;
        }

        browse = GetTemplateChild("PART_BrowseButton") as Button;

        if (browse is not null)
        {
            browse.Click += OnBrowse;
        }

        UpdateButton();
    }

    // Nothing handling BrowseRequested means the button cannot do anything, and an affordance that
    // does nothing when pressed is worse than no affordance. Typing the path still works.
    private void UpdateButton()
    {
        if (browse is null)
        {
            return;
        }

        bool offered = Row is { AllowsEditing: true } && this.FindAncestor<PropertyGrid>()?.CanBrowse == true;
        browse.Visibility = offered ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnBrowse(object sender, RoutedEventArgs e)
    {
        if (Row is not { } row)
        {
            return;
        }

        FilePathAttribute? declared = row.Description.GetAttribute<FilePathAttribute>();

        // Without the attribute the type still says enough: a DirectoryInfo is a folder, anything
        // else is a file to open.
        FilePathKind kind = declared?.Kind
            ?? (KnownTypes.Unwrap(row.PropertyType) == typeof(DirectoryInfo) ? FilePathKind.Folder : FilePathKind.OpenFile);

        (this.FindAncestor<PropertyGrid>())?.RaiseBrowseRequested(row, kind, declared?.Extensions ?? []);
    }
}
