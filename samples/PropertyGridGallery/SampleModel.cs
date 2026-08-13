using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using Digi21.WinUI.PropertyGrid;

namespace PropertyGridGallery;

// Everything the grid can show, in one object.
//
// An ObservableObject from the toolkit, with no glue at all: the grid only ever looks for
// INotifyPropertyChanged, which is what ObservableObject implements. Any other MVVM library, or a
// hand-rolled implementation, works exactly the same way. The properties are written out rather than
// generated so the attributes the grid reads sit where you would expect to find them.
public class SampleModel : ObservableObject
{
    private string text = "A string";
    private string notes = "Three lines of it," + "\r\n" + "so the row has to grow" + "\r\n" + "instead of clipping.";
    private string password = "hunter2";
    private int whole = 42;
    private double real = 1000.5;
    private decimal big = 9007199254740993m;
    private bool flag = true;
    private bool? maybe;
    private LineStyle outline = LineStyle.Solid;
    private Interactions interactions = Interactions.Select | Interactions.Hover;
    private DateTime moment = new(2026, 8, 13, 9, 30, 0, DateTimeKind.Local);
    private DateOnly day = new(2026, 3, 1);
    private TimeOnly clock = new(9, 30);
    private DateTime? missing;
    private TimeSpan every = TimeSpan.FromMinutes(90);
    private Windows.UI.Color fill = Microsoft.UI.Colors.CornflowerBlue;
    private Uri? address = new("https://example.com/parcels.gpkg");
    private Guid identifier = Guid.NewGuid();
    private string sourceFile = @"C:\data\parcels.gpkg";
    private DirectoryInfo? exportFolder = new(@"C:\data\exports");
    private int opacity = 80;
    private double zoom = 34;
    private string phone = "(816) 220-0000";
    private string host = "192.168.1.1";
    private string code = "AB-1200";
    private bool agreed = true;
    private Quality quality = Quality.Balanced;

    // ---- Standard items: one property per built-in editor ----

    [Category("Standard items")]
    [PropertyOrder(0)]
    [Description("A plain string. Commits when the box loses focus, or on Enter.")]
    public string Text
    {
        get => text;
        set => SetProperty(ref text, value);
    }

    [Category("Standard items")]
    [PropertyOrder(1)]
    [DataType(DataType.MultilineText)]
    [Description("A string that accepts line breaks. The row grows to fit it.")]
    public string Notes
    {
        get => notes;
        set => SetProperty(ref notes, value);
    }

    [Category("Standard items")]
    [PropertyOrder(2)]
    [PasswordPropertyText(true)]
    [Description("Hidden as it is typed.")]
    public string Password
    {
        get => password;
        set => SetProperty(ref password, value);
    }

    [Category("Standard items")]
    [PropertyOrder(3)]
    [Range(0, 1000)]
    [Description("An integer, in a number box with its own range.")]
    public int Whole
    {
        get => whole;
        set => SetProperty(ref whole, value);
    }

    [Category("Standard items")]
    [PropertyOrder(4)]
    [Description("A double, also in a number box.")]
    public double Real
    {
        get => real;
        set => SetProperty(ref real, value);
    }

    [Category("Standard items")]
    [PropertyOrder(5)]
    [Description("A decimal, in a text box: a number box works in doubles and would round this one.")]
    public decimal Big
    {
        get => big;
        set => SetProperty(ref big, value);
    }

    [Category("Standard items")]
    [PropertyOrder(6)]
    [DefaultValue(true)]
    [Description("A tick box.")]
    public bool Flag
    {
        get => flag;
        set => SetProperty(ref flag, value);
    }

    [Category("Standard items")]
    [PropertyOrder(7)]
    [Description("A nullable bool, so the box has a third state for “not decided”.")]
    public bool? Maybe
    {
        get => maybe;
        set => SetProperty(ref maybe, value);
    }

    [Category("Standard items")]
    [PropertyOrder(8)]
    [Description("An enumeration, labelled from its members' attributes.")]
    public LineStyle Outline
    {
        get => outline;
        set => SetProperty(ref outline, value);
    }

    [Category("Standard items")]
    [PropertyOrder(9)]
    [Description("A [Flags] enumeration: a drop-down of tick boxes, one per flag.")]
    public Interactions Interactions
    {
        get => interactions;
        set => SetProperty(ref interactions, value);
    }

    [Category("Standard items")]
    [PropertyOrder(10)]
    [Description("A colour: the swatch says roughly which, the numbers say exactly which.")]
    public Windows.UI.Color Fill
    {
        get => fill;
        set => SetProperty(ref fill, value);
    }

    [Category("Standard items")]
    [PropertyOrder(11)]
    [Description("A date and a time.")]
    public DateTime Moment
    {
        get => moment;
        set => SetProperty(ref moment, value);
    }

    [Category("Standard items")]
    [PropertyOrder(12)]
    [Description("A DateOnly, so a calendar and no clock.")]
    public DateOnly Day
    {
        get => day;
        set => SetProperty(ref day, value);
    }

    [Category("Standard items")]
    [PropertyOrder(13)]
    [Description("A TimeOnly, so a clock and no calendar.")]
    public TimeOnly Clock
    {
        get => clock;
        set => SetProperty(ref clock, value);
    }

    [Category("Standard items")]
    [PropertyOrder(14)]
    [Description("A nullable date that is empty, which is what a column nobody has filled in looks like.")]
    public DateTime? Missing
    {
        get => missing;
        set => SetProperty(ref missing, value);
    }

    [Category("Standard items")]
    [PropertyOrder(15)]
    [Description("A duration, in a text box: a clock cannot express more than a day or less than zero.")]
    public TimeSpan Every
    {
        get => every;
        set => SetProperty(ref every, value);
    }

    [Category("Standard items")]
    [PropertyOrder(15)]
    [Description("A Uri, which round-trips through text.")]
    public Uri? Address
    {
        get => address;
        set => SetProperty(ref address, value);
    }

    [Category("Standard items")]
    [PropertyOrder(16)]
    [ReadOnly(true)]
    [Description("Read-only: shown, selectable, and refused any edit.")]
    public Guid Identifier
    {
        get => identifier;
        set => SetProperty(ref identifier, value);
    }

    // ---- Paths, lists and objects ----

    [Category("Paths and objects")]
    [FilePath(FilePathKind.OpenFile, ".gpkg", ".las")]
    [Description("A path with a browse button. The application opens the dialog; the grid never does.")]
    public string SourceFile
    {
        get => sourceFile;
        set => SetProperty(ref sourceFile, value);
    }

    [Category("Paths and objects")]
    [Description("A DirectoryInfo, which gets the same editor without being asked.")]
    public DirectoryInfo? ExportFolder
    {
        get => exportFolder;
        set => SetProperty(ref exportFolder, value);
    }

    [Category("Paths and objects")]
    [Description("A list: how many there are, and a button that asks the application to edit them.")]
    public IList<int> Scales { get; } = [500, 1000, 5000];

    [Category("Paths and objects")]
    [Expandable]
    [Description("An object, opened into indented child rows in the same list.")]
    public ServerSettings Server { get; } = new();

    // ---- Editors the application supplies, all of them just a DataTemplate ----

    [Category("Application editors")]
    [PropertyEditor("Percent")]
    [Range(0, 100)]
    [Description("A slider and a reset button, declared in the page's XAML with a Click handler.")]
    public int Opacity
    {
        get => opacity;
        set => SetProperty(ref opacity, value);
    }

    [Category("Application editors")]
    [PropertyEditor("SpinAndSlider")]
    [Description("A slider and a number box on the same value.")]
    public double Zoom
    {
        get => zoom;
        set => SetProperty(ref zoom, value);
    }

    [Category("Application editors")]
    [PropertyEditor("Masked")]
    [Description("A text box with a mask in front of it.")]
    public string Phone
    {
        get => phone;
        set => SetProperty(ref phone, value);
    }

    [Category("Application editors")]
    [PropertyEditor("Address")]
    [Description("Four boxes that make one value.")]
    public string Host
    {
        get => host;
        set => SetProperty(ref host, value);
    }

    [Category("Application editors")]
    [PropertyEditor("UpperCase")]
    [Description("Typed in any case, stored in upper.")]
    public string Code
    {
        get => code;
        set => SetProperty(ref code, value);
    }

    [Category("Application editors")]
    [PropertyEditor("Consent")]
    [Description("A tick box with its own caption beside it.")]
    public bool Agreed
    {
        get => agreed;
        set => SetProperty(ref agreed, value);
    }

    [Category("Application editors")]
    [PropertyEditor("Radios")]
    [Description("A group of radio buttons rather than a drop-down. The row grows for them.")]
    public Quality Quality
    {
        get => quality;
        set => SetProperty(ref quality, value);
    }

    [Category("Application editors")]
    [PropertyEditor("Hyperlink")]
    [ReadOnly(true)]
    [Description("A link. The row is read-only; pressing it is not editing.")]
    public string Documentation => "https://github.com/digi21/PropertyGrid";
}

public class ServerSettings : ObservableObject
{
    private string host = "tiles.example.com";
    private int port = 443;
    private bool useTls = true;

    [Description("The host to connect to.")]
    public string Host
    {
        get => host;
        set => SetProperty(ref host, value);
    }

    [Description("The port to connect on.")]
    [Range(1, 65535)]
    public int Port
    {
        get => port;
        set => SetProperty(ref port, value);
    }

    [Description("Whether the connection is encrypted.")]
    public bool UseTls
    {
        get => useTls;
        set => SetProperty(ref useTls, value);
    }
}

public enum LineStyle
{
    Solid,

    [Display(Name = "Dashed line")]
    Dashed,

    [Description("Short dots, for hairline boundaries.")]
    Dotted,
}

[Flags]
public enum Interactions
{
    None = 0,
    Select = 1,
    Hover = 2,
    Edit = 4,
}

public enum Quality
{
    Draft,
    Balanced,
    Best,
}
