using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Shapes;

namespace Digi21.WinUI.PropertyGrid.Primitives;

/// <summary>The draggable divider between the name and the value columns of a <see cref="PropertyGrid"/>.</summary>
/// <remarks>
/// There is one of these in the whole grid, sitting over the scrolling area rather than inside it or
/// inside a row. That way it never scrolls out of view, it draws as one unbroken line down the
/// grid, and there is a single thing to give focus and an automation peer to.
/// </remarks>
public partial class PropertyGridSplitter : Control
{
    private Pointer? capturedPointer;
    private double startPosition;
    private double startWidth;

    /// <summary>Initializes a new instance of the <see cref="PropertyGridSplitter"/> class.</summary>
    public PropertyGridSplitter()
    {
        DefaultStyleKey = typeof(PropertyGridSplitter);
        DefaultStyleResourceUri = new Uri("ms-appx:///Digi21.WinUI.PropertyGrid/Themes/Generic.xaml");
        PropertyGridThemeResources.Ensure();
        ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.SizeWestEast);
    }

    internal PropertyGrid? Owner { get; set; }

    private bool IsDragging => capturedPointer is not null;

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
        startPosition = e.GetCurrentPoint(owner).Position.X;
        startWidth = owner.NameColumnWidth;

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

        double delta = e.GetCurrentPoint(owner).Position.X - startPosition;

        // Rounded before it is written, so that a drag of a fraction of a pixel does not invalidate
        // the measure of every realized row for a change nobody can see.
        owner.NameColumnWidth = Math.Round(startWidth + delta);
        e.Handled = true;
    }

    /// <inheritdoc />
    protected override void OnDoubleTapped(DoubleTappedRoutedEventArgs e)
    {
        base.OnDoubleTapped(e);
        Owner?.AutoSizeNameColumn();
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

    private void UpdateGrip()
    {
        if (GetTemplateChild("PART_Grip") is Rectangle grip)
        {
            // The band is wide enough to grab; the line drawn inside it is a hairline. The template
            // cannot express "one device-independent pixel from the resources", so it is set here.
            grip.Width = PropertyGridThemeResources.Value("PropertyGridSplitterGripThickness", 1.0);
        }
    }
}
