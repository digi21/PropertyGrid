using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace Digi21.WinUI.PropertyGrid.Tests;

// Hand-rolled observable and validating objects rather than CommunityToolkit.Mvvm ones. The library
// only ever touches INotifyPropertyChanged and INotifyDataErrorInfo, so the tests prove that is
// enough by implementing exactly those - and the test project stays free of dependencies.
internal class Observable : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        Raise(propertyName);
        return true;
    }

    protected void Raise(string? propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    internal void RaiseEverythingChanged() => Raise(string.Empty);
}

internal sealed class ObservableSubject : Observable
{
    private string name = "initial";
    private int count;

    public string Name
    {
        get => name;
        set => SetField(ref name, value);
    }

    public int Count
    {
        get => count;
        set => SetField(ref count, value);
    }
}

internal sealed class ClampingSubject
{
    private int percentage;

    // A setter that stores something other than what it was handed. The row has to show what was
    // kept, not what was typed.
    public int Percentage
    {
        get => percentage;
        set => percentage = Math.Clamp(value, 0, 100);
    }
}

internal sealed class ValidatingSubject : Observable, INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> errors = [];
    private string identifier = string.Empty;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public bool HasErrors => errors.Count > 0;

    public string Identifier
    {
        get => identifier;
        set
        {
            SetField(ref identifier, value);

            // A domain rule the grid cannot see from the outside: only the object knows the
            // identifier has to be uppercase, and it says so after the value is already stored.
            if (value.Length > 0 && value != value.ToUpperInvariant())
            {
                errors[nameof(Identifier)] = ["The identifier must be uppercase."];
            }
            else
            {
                errors.Remove(nameof(Identifier));
            }

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(Identifier)));
        }
    }

    public IEnumerable GetErrors(string? propertyName) =>
        propertyName is not null && errors.TryGetValue(propertyName, out List<string>? found) ? found : Array.Empty<string>();
}

internal sealed class AnnotatedValidationSubject
{
    [Range(1, 10)]
    public int Rating { get; set; } = 5;

    [Required]
    [StringLength(4, MinimumLength = 2)]
    public string Code { get; set; } = "abc";
}

internal sealed class ConversionSubject
{
    public string Text { get; set; } = string.Empty;

    public char Letter { get; set; } = 'a';

    public bool Flag { get; set; }

    public bool? MaybeFlag { get; set; }

    public int Whole { get; set; }

    public int? MaybeWhole { get; set; }

    public long Big { get; set; }

    public decimal Money { get; set; }

    public double Real { get; set; }

    public Guid Identifier { get; set; }

    public Uri? Address { get; set; }

    public TimeSpan Duration { get; set; }

    public DateTime Moment { get; set; } = new(2026, 8, 13, 10, 30, 0, DateTimeKind.Local);

    public DateTimeOffset Stamp { get; set; }

    public DateOnly Day { get; set; } = new(2026, 8, 13);

    public TimeOnly Clock { get; set; } = new(10, 30);

    public Fruit Choice { get; set; } = Fruit.Apple;

    public Fruit? MaybeChoice { get; set; }

    public Access Permissions { get; set; }
}

internal enum Fruit
{
    Apple,

    [Display(Name = "Pear tree")]
    Pear,

    [Description("A soft one.")]
    Peach,
}

[Flags]
internal enum Access
{
    None = 0,
    Read = 1,
    Write = 2,
    Delete = 4,
    All = Read | Write | Delete,
}

internal sealed class NestedSubject
{
    [Expandable]
    public AddressSubject Address { get; set; } = new();

    public string Label { get; set; } = "root";
}

internal sealed class AddressSubject : Observable
{
    private string city = "Madrid";

    public string City
    {
        get => city;
        set => SetField(ref city, value);
    }

    public string Street { get; set; } = "Gran Via";
}

internal sealed class CyclicSubject
{
    public string Name { get; set; } = "node";

    [Expandable]
    public CyclicSubject? Next { get; set; }
}

internal sealed class DeepSubject
{
    public string Name { get; set; } = "level";

    [Expandable]
    public DeepSubject? Child { get; set; }
}

internal sealed class ValueTypeHolder
{
    [Expandable]
    public ValueTypePoint Point { get; set; }
}

internal struct ValueTypePoint
{
    public int X { get; set; }

    public int Y { get; set; }
}

internal sealed class CollectionHolder
{
    [Expandable]
    public List<string> Items { get; set; } = ["a", "b"];
}

internal sealed class CategorizedSubject
{
    [Category("Appearance")]
    [Description("How wide the thing is.")]
    public int Width { get; set; }

    [Category("Appearance")]
    public int Height { get; set; }

    [Category("Behaviour")]
    public bool IsEnabled { get; set; }

    public string Loose { get; set; } = string.Empty;
}
