using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;

namespace Digi21.WinUI.PropertyGrid.Primitives;

/// <summary>A draggable divider in a <see cref="PropertyGrid"/>: between the two columns, or above the description pane.</summary>
/// <remarks>
/// There is one of each in the whole grid, sitting over what it divides rather than inside it or
/// inside a row. That way neither scrolls out of view, the vertical one draws as an unbroken line
/// down the grid, and there is a single thing to give focus and an automation peer to.
/// </remarks>
public partial class PropertyGridSplitter : Control
{
    /// <summary>Identifies the <see cref="Orientation"/> dependency property.</summary>
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
        nameof(Orientation),
        typeof(Orientation),
        typeof(PropertyGridSplitter),
        new PropertyMetadata(Orientation.Vertical, (d, _) => ((PropertyGridSplitter)d).OnOrientationChanged()));

    private Pointer? capturedPointer;
    private double startPosition;
    private double startValue;

    /// <summary>Initializes a new instance of the <see cref="PropertyGridSplitter"/> class.</summary>
    public PropertyGridSplitter()
    {
        DefaultStyleKey = typeof(PropertyGridSplitter);
        DefaultStyleResourceUri = new Uri("ms-appx:///Digi21.WinUI.PropertyGrid/Themes/Generic.xaml");
        PropertyGridThemeResources.Ensure();
        UpdateCursor();
    }

    /// <summary>Gets or sets which way the divider is drawn, and therefore which way it drags.</summary>
    /// <remarks>
    /// <see cref="Microsoft.UI.Xaml.Controls.Orientation.Vertical"/>, the default, is the line between
    /// the name and the value columns; <see cref="Microsoft.UI.Xaml.Controls.Orientation.Horizontal"/>
    /// is the one above the description pane.
    /// </remarks>
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    internal PropertyGrid? Owner { get; set; }

    private bool IsDragging => capturedPointer is not null;

    private bool IsHorizontal => Orientation == Orientation.Horizontal;

    /// <inheritdoc />
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        UpdateGrip();
        VisualStateManager.GoToState(this, "Normal", false);
    }

    /// <inheritdoc />
    protected override void OnPointerEntered(PointerRoutedEventArgs e)
    {
        base.OnPointerEntered(e);
        VisualStateManager.GoToState(this, IsDragging ? "Pressed" : "PointerOver", true);
    }

    /// <inheritdoc />
    protected override void OnPointerExited(PointerRoutedEventArgs e)
    {
        base.OnPointerExited(e);
        VisualStateManager.GoToState(this, IsDragging ? "Pressed" : "Normal", true);
    }

    /// <inheritdoc />
    protected override void OnPointerPressed(PointerRoutedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (Owner is not { } owner || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (!CapturePointer(e.Pointer))
        {
            return;
        }

        capturedPointer = e.Pointer;

        Point position = e.GetCurrentPoint(owner).Position;
        startPosition = IsHorizontal ? position.Y : position.X;
        startValue = IsHorizontal ? owner.DescriptionHeight : owner.NameColumnWidth;

        VisualStateManager.GoToState(this, "Pressed", true);
        e.Handled = true;
    }

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerRoutedEventArgs e)
    {
        base.OnPointerMoved(e);

        if (!IsDragging || Owner is not { } owner)
        {
            return;
        }

        Point position = e.GetCurrentPoint(owner).Position;

        // Rounded before it is written, so that a drag of a fraction of a pixel does not invalidate
        // the measure of every realized row for a change nobody can see.
        if (IsHorizontal)
        {
            // The pane hangs below the divider, so dragging up has to make it taller, not shorter.
            owner.DescriptionHeight = Math.Round(startValue - (position.Y - startPosition));
        }
        else
        {
            owner.NameColumnWidth = Math.Round(startValue + (position.X - startPosition));
        }

        e.Handled = true;
    }

    /// <inheritdoc />
    protected override void OnDoubleTapped(DoubleTappedRoutedEventArgs e)
    {
        base.OnDoubleTapped(e);

        if (IsHorizontal)
        {
            Owner?.AutoSizeDescription();
        }
        else
        {
            Owner?.AutoSizeNameColumn();
        }

        e.Handled = true;
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerRoutedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (IsDragging)
        {
            ReleasePointerCapture(e.Pointer);
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    protected override void OnPointerCaptureLost(PointerRoutedEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        capturedPointer = null;
        VisualStateManager.GoToState(this, "Normal", true);
    }

    private void OnOrientationChanged()
    {
        UpdateCursor();
        UpdateGrip();
    }

    private void UpdateCursor() =>
        ProtectedCursor = InputSystemCursor.Create(
            IsHorizontal ? InputSystemCursorShape.SizeNorthSouth : InputSystemCursorShape.SizeWestEast);

    private void UpdateGrip()
    {
        if (GetTemplateChild("PART_Grip") is not Rectangle grip)
        {
            return;
        }

        // The band is wide enough to grab; the line drawn inside it is a hairline. The template can
        // express neither "one device-independent pixel from the resources" nor which way the line
        // runs, so both are settled here.
        double thickness = PropertyGridThemeResources.Value("PropertyGridSplitterGripThickness", 1.0);

        if (IsHorizontal)
        {
            grip.Width = double.NaN;
            grip.Height = thickness;
            grip.HorizontalAlignment = HorizontalAlignment.Stretch;
            grip.VerticalAlignment = VerticalAlignment.Center;
        }
        else
        {
            grip.Width = thickness;
            grip.Height = double.NaN;
            grip.HorizontalAlignment = HorizontalAlignment.Center;
            grip.VerticalAlignment = VerticalAlignment.Stretch;
        }
    }
}
