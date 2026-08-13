using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace Digi21.WinUI.PropertyGrid.Primitives;

/// <summary>
/// Arranges the three cells of a property row — the name, the splitter gutter and the editor — so
/// that the name column is exactly as wide in every row of the same grid.
/// </summary>
/// <remarks>
/// <para>
/// The width cannot be a property of the row, and cannot be reached by a binding either. A repeater
/// measures every row on its own, WinUI has no shared size scope, and it has no
/// <c>RelativeSource AncestorType</c> to carry one value into all of them — while a
/// <see cref="ColumnDefinition"/> is not a <see cref="FrameworkElement"/> at all, so it has no data
/// context and a binding on its width silently does nothing.
/// </para>
/// <para>
/// So the owning grid pushes the width in instead, which is also exactly what lets a single splitter
/// resize every row at once.
/// </para>
/// </remarks>
public partial class PropertyGridRowPanel : Panel
{
    /// <summary>Identifies the <see cref="NameWidth"/> dependency property.</summary>
    public static readonly DependencyProperty NameWidthProperty = DependencyProperty.Register(
        nameof(NameWidth),
        typeof(double),
        typeof(PropertyGridRowPanel),
        new PropertyMetadata(160.0, (d, _) => ((PropertyGridRowPanel)d).InvalidateMeasure()));

    /// <summary>Identifies the <see cref="GutterWidth"/> dependency property.</summary>
    public static readonly DependencyProperty GutterWidthProperty = DependencyProperty.Register(
        nameof(GutterWidth),
        typeof(double),
        typeof(PropertyGridRowPanel),
        new PropertyMetadata(6.0, (d, _) => ((PropertyGridRowPanel)d).InvalidateMeasure()));

    /// <summary>Identifies the <see cref="Indent"/> dependency property.</summary>
    public static readonly DependencyProperty IndentProperty = DependencyProperty.Register(
        nameof(Indent),
        typeof(double),
        typeof(PropertyGridRowPanel),
        new PropertyMetadata(0.0, (d, _) => ((PropertyGridRowPanel)d).InvalidateMeasure()));

    /// <summary>Gets or sets the width of the name column, measured from the left edge of the row.</summary>
    public double NameWidth
    {
        get => (double)GetValue(NameWidthProperty);
        set => SetValue(NameWidthProperty, value);
    }

    /// <summary>Gets or sets the width of the band the splitter sits over.</summary>
    public double GutterWidth
    {
        get => (double)GetValue(GutterWidthProperty);
        set => SetValue(GutterWidthProperty, value);
    }

    /// <summary>Gets or sets how far the name cell is pushed in to show how deeply the row is nested.</summary>
    /// <remarks>
    /// Nesting eats into the name cell and never moves the split, so the two columns stay lined up at
    /// every depth. That is the property that makes a deep object graph readable.
    /// </remarks>
    public double Indent
    {
        get => (double)GetValue(IndentProperty);
        set => SetValue(IndentProperty, value);
    }

    /// <summary>Gets the width the name cell would need to show its contents in full.</summary>
    public double NaturalNameWidth { get; private set; }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        (UIElement? name, UIElement? gutter, UIElement? editor) = Cells();

        // Rows never scroll sideways, so the editor takes whatever the name column leaves. An
        // infinite width means nobody constrained the row - it was measured outside a viewport, or
        // put in a horizontally scrolling host, which is not supported - and the row falls back to
        // what its children want so it is at least not zero-sized.
        bool bounded = !double.IsInfinity(availableSize.Width);
        double width = bounded ? availableSize.Width : 0.0;

        double nameWidth = bounded
            ? Math.Max(0, Math.Min(NameWidth, width) - Indent)
            : double.PositiveInfinity;

        name?.Measure(new Size(nameWidth, availableSize.Height));
        NaturalNameWidth = (name?.DesiredSize.Width ?? 0) + Indent;

        gutter?.Measure(new Size(GutterWidth, availableSize.Height));

        double editorWidth = bounded
            ? Math.Max(0, width - NameWidth - GutterWidth)
            : double.PositiveInfinity;
        editor?.Measure(new Size(editorWidth, availableSize.Height));

        if (!bounded)
        {
            width = NaturalNameWidth + GutterWidth + (editor?.DesiredSize.Width ?? 0);
        }

        double height = Math.Max(
            Math.Max(name?.DesiredSize.Height ?? 0, editor?.DesiredSize.Height ?? 0),
            gutter?.DesiredSize.Height ?? 0);

        return new Size(width, height);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        (UIElement? name, UIElement? gutter, UIElement? editor) = Cells();
        double split = Math.Clamp(NameWidth, 0, finalSize.Width);

        name?.Arrange(new Rect(Indent, 0, Math.Max(0, split - Indent), finalSize.Height));
        gutter?.Arrange(new Rect(split, 0, Math.Min(GutterWidth, Math.Max(0, finalSize.Width - split)), finalSize.Height));
        editor?.Arrange(new Rect(
            Math.Min(split + GutterWidth, finalSize.Width),
            0,
            Math.Max(0, finalSize.Width - split - GutterWidth),
            finalSize.Height));

        return finalSize;
    }

    // The three cells are identified by position rather than by name, so that a replacement row
    // template does not have to know about any attached property to lay itself out correctly.
    private (UIElement? Name, UIElement? Gutter, UIElement? Editor) Cells() => (
        Children.Count > 0 ? Children[0] : null,
        Children.Count > 1 ? Children[1] : null,
        Children.Count > 2 ? Children[2] : null);
}
