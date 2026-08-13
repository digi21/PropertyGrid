using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Digi21.WinUI.PropertyGrid.Primitives;

/// <summary>A summary of a value with a button beside it, for anything that needs more room than a row.</summary>
/// <remarks>
/// The grid's equivalent of the modal editors a desktop property grid has always had. Pressing the
/// button raises <see cref="PropertyGrid.EditRequested"/> and nothing else: what opens is the
/// application's business, and only it knows how a list of its own objects should be presented.
/// </remarks>
public partial class PropertyGridDialogEditor : Control
{
    /// <summary>Identifies the <see cref="Row"/> dependency property.</summary>
    public static readonly DependencyProperty RowProperty = DependencyProperty.Register(
        nameof(Row),
        typeof(PropertyGridPropertyRow),
        typeof(PropertyGridDialogEditor),
        new PropertyMetadata(null));

    private Button? edit;

    /// <summary>Initializes a new instance of the <see cref="PropertyGridDialogEditor"/> class.</summary>
    public PropertyGridDialogEditor()
    {
        DefaultStyleKey = typeof(PropertyGridDialogEditor);
        DefaultStyleResourceUri = new Uri("ms-appx:///Digi21.WinUI.PropertyGrid/Themes/Generic.xaml");
        PropertyGridThemeResources.Ensure();

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

        if (edit is not null)
        {
            edit.Click -= OnEdit;
        }

        edit = GetTemplateChild("PART_EditButton") as Button;

        if (edit is not null)
        {
            edit.Click += OnEdit;
        }

        UpdateButton();
    }

    private void OnEdit(object sender, RoutedEventArgs e)
    {
        if (Row is { } row)
        {
            this.FindAncestor<PropertyGrid>()?.RaiseEditRequested(row);
        }
    }

    // Lists and complex objects get this editor without anybody asking, so an application that
    // handles nothing would otherwise show a button on every one of them that does nothing at all.
    private void UpdateButton()
    {
        if (edit is null)
        {
            return;
        }

        // AllowsEditing rather than IsEditable: a list is usually declared with only a getter and is
        // still meant to be edited, by changing what is in it rather than by assigning a new one.
        bool offered = Row is { AllowsEditing: true } && this.FindAncestor<PropertyGrid>()?.CanEdit == true;
        edit.Visibility = offered ? Visibility.Visible : Visibility.Collapsed;
    }
}
