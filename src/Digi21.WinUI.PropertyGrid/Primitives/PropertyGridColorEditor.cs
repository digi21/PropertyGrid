using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.UI;

namespace Digi21.WinUI.PropertyGrid.Primitives;

/// <summary>A swatch that opens a colour picker, for a colour or a solid brush.</summary>
/// <remarks>
/// <para>
/// The picker confirms on OK rather than as the wheel is dragged. Committing live would write a
/// hundred values per gesture, which fills an undo stack and makes every intermediate colour a
/// change event the application has to cope with.
/// </para>
/// <para>
/// This is a control rather than a plain template because a template declared in a resource
/// dictionary cannot carry the handlers that OK and Cancel need.
/// </para>
/// </remarks>
public partial class PropertyGridColorEditor : Control
{
    /// <summary>Identifies the <see cref="Row"/> dependency property.</summary>
    public static readonly DependencyProperty RowProperty = DependencyProperty.Register(
        nameof(Row),
        typeof(PropertyGridPropertyRow),
        typeof(PropertyGridColorEditor),
        new PropertyMetadata(null, (d, _) => ((PropertyGridColorEditor)d).OnRowChanged()));

    private ColorPicker? picker;
    private Button? confirm;
    private Button? cancel;
    private Button? open;
    private Shape? swatch;
    private PropertyGridPropertyRow? subscribed;

    /// <summary>Initializes a new instance of the <see cref="PropertyGridColorEditor"/> class.</summary>
    public PropertyGridColorEditor()
    {
        DefaultStyleKey = typeof(PropertyGridColorEditor);
        DefaultStyleResourceUri = new Uri("ms-appx:///Digi21.WinUI.PropertyGrid/Themes/Generic.xaml");
        PropertyGridThemeResources.Ensure();

        // Taken from the data context rather than bound in the editor template. The template is a
        // DataTemplate whose context is already the row, so binding it back in is ceremony that can
        // silently do nothing; this cannot.
        DataContextChanged += (_, arguments) =>
        {
            if (arguments.NewValue is PropertyGridPropertyRow row)
            {
                Row = row;
            }
        };
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

        Detach();

        open = GetTemplateChild("PART_OpenButton") as Button;
        swatch = GetTemplateChild("PART_Swatch") as Shape;

        // The flyout has not built its content yet, and when it does that content lives in its own
        // namescope, so its parts are found on opening rather than here.
        if (open?.Flyout is Flyout flyout)
        {
            flyout.Opened += OnFlyoutOpened;
        }

        UpdateSwatch();
    }

    private static Color? ColorOf(object? value) => value switch
    {
        Color color => color,
        SolidColorBrush brush => brush.Color,
        _ => null,
    };

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

        UpdateSwatch();
    }

    private void OnRowPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(PropertyGridPropertyRow.Value))
        {
            UpdateSwatch();
        }
    }

    private void OnFlyoutOpened(object? sender, object e)
    {
        if (sender is not Flyout { Content: FrameworkElement content })
        {
            return;
        }

        if (confirm is not null)
        {
            confirm.Click -= OnConfirm;
        }

        if (cancel is not null)
        {
            cancel.Click -= OnCancel;
        }

        picker = content.FindName("PART_Picker") as ColorPicker;
        confirm = content.FindName("PART_Confirm") as Button;
        cancel = content.FindName("PART_Cancel") as Button;

        if (confirm is not null)
        {
            confirm.Click += OnConfirm;
        }

        if (cancel is not null)
        {
            cancel.Click += OnCancel;
        }

        if (picker is not null && ColorOf(Row?.Value) is { } current)
        {
            picker.Color = current;
        }
    }

    private void OnConfirm(object sender, RoutedEventArgs e)
    {
        if (Row is { } row && picker is { } chooser)
        {
            // A brush-typed property needs a brush; a colour-typed one needs the colour. Handing a
            // brush to a Color property would be refused by the coercion and read as a bug here.
            row.Value = row.Value is Brush || typeof(Brush).IsAssignableFrom(KnownTypes.Unwrap(row.PropertyType))
                ? new SolidColorBrush(chooser.Color)
                : chooser.Color;
        }

        Close();
    }

    private void OnCancel(object sender, RoutedEventArgs e) => Close();

    private void Close()
    {
        if (open?.Flyout is { } flyout)
        {
            flyout.Hide();
        }
    }

    private void UpdateSwatch()
    {
        if (swatch is null)
        {
            // The template is not applied yet; OnApplyTemplate calls this again.
            return;
        }

        Color? current = ColorOf(Row?.Value);
        swatch.Fill = current is { } colour ? new SolidColorBrush(colour) : null;
    }

    private void Detach()
    {
        if (open?.Flyout is Flyout flyout)
        {
            flyout.Opened -= OnFlyoutOpened;
        }

        if (confirm is not null)
        {
            confirm.Click -= OnConfirm;
        }

        if (cancel is not null)
        {
            cancel.Click -= OnCancel;
        }
    }
}

