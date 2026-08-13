using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Digi21.WinUI.PropertyGrid;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

namespace PropertyGridGallery;

// A self-terminating run that measures the things about ItemsRepeater the grid's design rests on,
// writes what it found to a file, and closes.
//
// These are questions no unit test can answer - they need a live visual tree - and answering them by
// reading documentation would be guessing. Run it with:
//
//     dotnet run --project samples/PropertyGridGallery -- --diagnose report.txt
internal sealed class Diagnostics
{
    private const int SyntheticPropertyCount = 400;

    private readonly StringBuilder report = new();
    private readonly HashSet<object> distinctElements = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<object, object> innerEditorByPresenter = new(ReferenceEqualityComparer.Instance);

    private readonly Window window;
    private readonly Digi21.WinUI.PropertyGrid.PropertyGrid grid;
    private readonly string outputPath;

    private ItemsRepeater? repeater;
    private ScrollViewer? scroller;
    private int prepared;
    private int cleared;
    private int reusedInnerEditors;
    private int replacedInnerEditors;
    private int frame;

    internal Diagnostics(string outputPath)
    {
        this.outputPath = outputPath;

        grid = new Digi21.WinUI.PropertyGrid.PropertyGrid
        {
            DescriptionProvider = new SyntheticProvider(SyntheticPropertyCount),
            SelectedObject = new SyntheticBag(),
            PropertySort = PropertySort.Categorized,
        };

        window = new Window
        {
            Title = "PropertyGrid diagnostics",
            Content = new Grid { Children = { grid } },
        };
    }

    internal void Run()
    {
        window.Activate();
        CompositionTarget.Rendering += OnRendering;
    }

    private void OnRendering(object? sender, object e)
    {
        frame++;

        switch (frame)
        {
            case 5:
                Attach();
                break;

            case 10:
                Record("after first layout");
                scroller?.ChangeView(null, 2000, null, disableAnimation: true);
                break;

            case 25:
                Record("after scrolling down");
                scroller?.ChangeView(null, 0, null, disableAnimation: true);
                break;

            case 40:
                Record("after scrolling back up");
                Finish();
                break;
        }
    }

    private void Attach()
    {
        repeater = grid.Descendant<ItemsRepeater>();
        scroller = grid.Descendant<ScrollViewer>();

        report.AppendLine("Digi21.WinUI.PropertyGrid - repeater diagnostics");
        report.AppendLine($"rows in the model: {grid.Rows.Count}");
        report.AppendLine($"found PART_ItemsRepeater: {repeater is not null}");
        report.AppendLine($"found PART_ScrollViewer: {scroller is not null}");
        report.AppendLine($"horizontal scrolling disabled: {scroller?.HorizontalScrollMode}");
        report.AppendLine($"editor dictionary merged into the application: {Application.Current.Resources.ContainsKey(PropertyEditorKeys.String)}");
        report.AppendLine($"brush dictionary merged into the application: {Application.Current.Resources.ContainsKey("PropertyGridRowHeight")}");
        report.AppendLine();

        if (repeater is null)
        {
            return;
        }

        repeater.ElementPrepared += (_, arguments) =>
        {
            prepared++;
            distinctElements.Add(arguments.Element);
        };

        repeater.ElementClearing += (_, _) => cleared++;
    }

    // The two-level design assumes a ContentPresenter re-runs its selector when its content changes
    // and keeps the visual tree it already built when the chosen template is the same. If it does, a
    // recycled row landing on another property of the same kind does no visual-tree work at all.
    //
    // Sampled after a layout pass rather than in ElementPrepared: at preparation time the presenter
    // has been handed its content but has not built anything from it yet, so there is nothing to see.
    private void TrackInnerEditor(UIElement element)
    {
        if (Tree.EditorHost(element) is not { } presenter)
        {
            return;
        }

        object? inner = presenter.Descendant<Control>() as object ?? presenter.Descendant<TextBlock>();
        if (inner is null)
        {
            return;
        }

        if (innerEditorByPresenter.TryGetValue(element, out object? previous))
        {
            if (ReferenceEquals(previous, inner))
            {
                reusedInnerEditors++;
            }
            else
            {
                replacedInnerEditors++;
            }
        }

        innerEditorByPresenter[element] = inner;
    }

    private void Record(string stage)
    {
        int realized = 0;
        int withEditor = 0;

        for (int index = 0; index < grid.Rows.Count; index++)
        {
            if (repeater?.TryGetElement(index) is not { } element)
            {
                continue;
            }

            realized++;

            if (Tree.EditorHost(element)?.Descendant<FrameworkElement>() is not null)
            {
                withEditor++;
            }

            TrackInnerEditor(element);
        }

        // Row heights, because a row that has to grow for a tall editor and a row that must stay
        // dense for a single-line one are the same code path pulling in opposite directions.
        List<string> heights = [];
        for (int index = 0; index < grid.Rows.Count && heights.Count < 6; index++)
        {
            if (repeater?.TryGetElement(index) is FrameworkElement element)
            {
                heights.Add($"{grid.Rows[index].Key}={element.ActualHeight:0.#}");
            }
        }

        report.AppendLine($"--- {stage} ---");
        report.AppendLine($"row heights: {string.Join(", ", heights)}");
        report.AppendLine($"realized elements: {realized} of {grid.Rows.Count}");
        report.AppendLine($"realized elements whose editor resolved: {withEditor}");
        report.AppendLine($"ElementPrepared raised: {prepared}");
        report.AppendLine($"ElementClearing raised: {cleared}");
        report.AppendLine($"distinct element instances ever seen: {distinctElements.Count}");
        report.AppendLine($"recycled onto the same editor template: {reusedInnerEditors}");
        report.AppendLine($"recycled onto a different editor template: {replacedInnerEditors}");
        report.AppendLine();
    }

    // Renders the grid to a PNG so a change can be looked at without a person having to sit in front
    // of it. Not a substitute for trying the thing by hand, but it catches a layout that collapsed.
    internal static async Task CaptureAsync(Window host, FrameworkElement element, string path)
    {
        RenderTargetBitmap bitmap = new();
        await bitmap.RenderAsync(element);

        IBuffer pixels = await bitmap.GetPixelsAsync();

        using InMemoryRandomAccessStream stream = new();
        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);
        encoder.SetPixelData(
            BitmapPixelFormat.Bgra8,
            BitmapAlphaMode.Premultiplied,
            (uint)bitmap.PixelWidth,
            (uint)bitmap.PixelHeight,
            96,
            96,
            pixels.ToArray());
        await encoder.FlushAsync();

        using Stream managed = stream.AsStreamForRead();
        using FileStream file = File.Create(path);
        await managed.CopyToAsync(file);

        _ = host;
    }

    private void Finish()
    {
        CompositionTarget.Rendering -= OnRendering;

        report.AppendLine("--- verdicts ---");
        report.AppendLine(
            distinctElements.Count < prepared
                ? $"RECYCLING WORKS: {prepared} preparations reused {distinctElements.Count} element instances."
                : $"NO RECYCLING: {prepared} preparations produced {distinctElements.Count} distinct elements.");
        report.AppendLine(
            reusedInnerEditors > 0
                ? $"CONTENT PRESENTER KEEPS ITS TREE: {reusedInnerEditors} recycles reused the inner editor."
                : "CONTENT PRESENTER REBUILDS: no recycle reused its inner editor.");

        File.WriteAllText(outputPath, report.ToString());
        window.Close();
        Application.Current.Exit();
    }
}

internal static class Tree
{
    // A row presenter contains several content presenters - the expander button has one of its own -
    // so the editor host is identified by what it is showing rather than by being the first one found.
    internal static ContentPresenter? EditorHost(DependencyObject start)
    {
        int count = VisualTreeHelper.GetChildrenCount(start);
        for (int index = 0; index < count; index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(start, index);
            if (child is ContentPresenter { Content: PropertyGridPropertyRow } match)
            {
                return match;
            }

            if (EditorHost(child) is { } deeper)
            {
                return deeper;
            }
        }

        return null;
    }

    internal static T? Descendant<T>(this DependencyObject start)
        where T : DependencyObject
    {
        int count = VisualTreeHelper.GetChildrenCount(start);
        for (int index = 0; index < count; index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(start, index);
            if (child is T match)
            {
                return match;
            }

            if (child.Descendant<T>() is { } deeper)
            {
                return deeper;
            }
        }

        return null;
    }
}

// A grid driven by something reflection cannot see: the properties are invented at run time and the
// values live in a dictionary. This is the escape hatch IPropertyDescriptionProvider exists for, and
// it happens to be the easiest way to build a list long enough to make virtualization matter.
internal sealed class SyntheticProvider(int count) : IPropertyDescriptionProvider
{
    public IReadOnlyList<PropertyDescription> GetProperties(Type type)
    {
        List<PropertyDescription> properties = [];
        for (int index = 0; index < count; index++)
        {
            string name = $"Field{index:D3}";

            // A mix of types, so that recycling a row is sometimes onto the same editor and
            // sometimes onto a different one - which are two different questions.
            Type propertyType = (index % 5) switch
            {
                0 => typeof(bool),
                1 => typeof(int),
                _ => typeof(string),
            };

            // Every tenth one is multi-line, so the report shows both a dense row and a grown one.
            bool tall = index % 10 == 3;

            properties.Add(new PropertyDescription
            {
                Name = name,
                PropertyType = propertyType,
                DeclaringType = type,
                Accessor = new BagAccessor(name, propertyType, tall),
                CategoryName = $"Group {index / 25:D2}",
                HelpText = $"Synthetic property number {index}.",
                EditorKey = tall ? PropertyEditorKeys.MultilineString : null,
            });
        }

        return properties;
    }
}

internal sealed class SyntheticBag
{
    internal Dictionary<string, object?> Values { get; } = [];
}

internal sealed class BagAccessor(string name, Type propertyType, bool tall = false) : PropertyAccessor
{
    public override bool CanRead => true;

    public override bool CanWrite => true;

    protected override object? GetValueCore(object target) =>
        target is SyntheticBag bag && bag.Values.TryGetValue(name, out object? value)
            ? value
            : propertyType == typeof(string)
                ? tall ? name + "\r\nsecond line\r\nthird line" : name
                : Activator.CreateInstance(propertyType);

    protected override void SetValueCore(object target, object? value)
    {
        if (target is SyntheticBag bag)
        {
            bag.Values[name] = value;
        }
    }
}

