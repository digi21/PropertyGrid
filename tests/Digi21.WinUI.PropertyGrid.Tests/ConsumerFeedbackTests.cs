using System.Globalization;
using Xunit;

namespace Digi21.WinUI.PropertyGrid.Tests;

// The cases a real consumer asked for, and the ones it told us not to break. Kept together so the
// reason each exists stays attached to it.
public class ConsumerFeedbackTests
{
    // A provider whose properties are invented at run time, over values kept in a dictionary. This
    // is the shape the consumer drives its GeoPackage inspector with.
    private sealed class DomainProvider(IReadOnlyList<PropertyDescription> properties) : IPropertyDescriptionProvider
    {
        internal int Calls { get; private set; }

        public IReadOnlyList<PropertyDescription> GetProperties(Type type)
        {
            Calls++;
            return properties;
        }
    }

    private sealed class BagAccessor(string name) : PropertyAccessor
    {
        public override bool CanRead => true;

        public override bool CanWrite => true;

        protected override object? GetValueCore(object target) =>
            ((Dictionary<string, object?>)target).TryGetValue(name, out object? value) ? value : null;

        protected override void SetValueCore(object target, object? value) =>
            ((Dictionary<string, object?>)target)[name] = value;
    }

    private static PropertyDescription Coded(string name, params PropertyStandardValue[] values) => new()
    {
        Name = name,
        PropertyType = typeof(int),
        Accessor = new BagAccessor(name),
        StandardValues = values,
    };

    // ---- values declared per property, with a label that is not the value ----

    [Fact]
    public void UsesTheValuesTheDescriptionNamedRatherThanAskingTheType()
    {
        PropertyGridSource source = new()
        {
            Provider = new DomainProvider([Coded("SignType", new PropertyStandardValue(1, "Stop"), new PropertyStandardValue(2, "No entry"))]),
        };
        source.SetTarget(new Dictionary<string, object?> { ["SignType"] = 2 });

        PropertyGridPropertyRow row = source.FindRow("SignType")!;

        Assert.Equal(["Stop", "No entry"], row.StandardValues.Select(standard => standard.DisplayName));
        Assert.Equal("No entry", row.SelectedStandardValue?.DisplayName);
    }

    [Fact]
    public void LetsTwoPropertiesOfTheSameTypeHaveDifferentDomains()
    {
        // The whole reason a TypeConverter is not enough: both are int, both come from the same
        // table, and they accept different sets.
        PropertyGridSource source = new()
        {
            Provider = new DomainProvider(
            [
                Coded("SignType", new PropertyStandardValue(1, "Stop")),
                Coded("Surface", new PropertyStandardValue(7, "Asphalt"), new PropertyStandardValue(8, "Gravel")),
            ]),
        };
        source.SetTarget(new Dictionary<string, object?>());

        Assert.Single(source.FindRow("SignType")!.StandardValues);
        Assert.Equal(2, source.FindRow("Surface")!.StandardValues.Count);
    }

    [Fact]
    public void WritesTheValueBehindTheLabel()
    {
        // What reaches the file is the code, not what the user read.
        Dictionary<string, object?> record = [];
        PropertyGridSource source = new()
        {
            Provider = new DomainProvider([Coded("SignType", new PropertyStandardValue(1, "Stop"), new PropertyStandardValue(2, "No entry"))]),
        };
        source.SetTarget(record);
        PropertyGridPropertyRow row = source.FindRow("SignType")!;

        row.SelectedStandardValue = row.StandardValues[1];

        Assert.Equal(2, record["SignType"]);
    }

    [Fact]
    public void OffersTheStandardValuesEditorWithoutAnybodyNamingIt()
    {
        Assert.Equal(
            PropertyEditorKeys.StandardValues,
            BuiltInEditors.KeyFor(Coded("SignType", new PropertyStandardValue(1, "Stop")), null));
    }

    [Fact]
    public void StillAsksTheTypeWhenTheDescriptionNamedNothing()
    {
        PropertyGridSource source = new();
        source.SetTarget(new Plain());

        // No converter with a fixed list, so nothing to offer - and no exception on the way there.
        Assert.Empty(source.FindRow("Count")!.StandardValues);
    }

    // ---- an editor is chosen per property, not per type ----

    [Fact]
    public void GivesTwoPropertiesOfTheSameTypeDifferentEditors()
    {
        // Both are string. One has a list of its own and the other does not, and that has to be
        // enough to tell them apart - it was not while resolution was memoized by declared type.
        PropertyDescription coded = Coded("Tipo", new PropertyStandardValue(1, "Stop")) with { PropertyType = typeof(string) };
        PropertyDescription plain = new() { Name = "Codigo", PropertyType = typeof(string), Accessor = new BagAccessor("Codigo") };

        Assert.Equal(PropertyEditorKeys.StandardValues, BuiltInEditors.KeyFor(coded, null));
        Assert.Equal(PropertyEditorKeys.String, BuiltInEditors.KeyFor(plain, null));
    }

    [Fact]
    public void DoesNotLetWhicheverCameFirstDecideForTheRest()
    {
        PropertyDescription coded = Coded("Tipo", new PropertyStandardValue(1, "Stop")) with { PropertyType = typeof(string) };
        PropertyDescription plain = new() { Name = "Codigo", PropertyType = typeof(string), Accessor = new BagAccessor("Codigo") };

        // Same answers whichever order they are asked in.
        string plainFirst = BuiltInEditors.KeyFor(plain, null);
        string codedAfter = BuiltInEditors.KeyFor(coded, null);
        string codedFirst = BuiltInEditors.KeyFor(coded, null);
        string plainAfter = BuiltInEditors.KeyFor(plain, null);

        Assert.Equal(plainFirst, plainAfter);
        Assert.Equal(codedFirst, codedAfter);
        Assert.NotEqual(plainFirst, codedFirst);
    }

    // ---- an editor being realized or recycled is not a user edit ----

    [Fact]
    public void IgnoresACombosNullSelectionWhenNoneWasNeverOnOffer()
    {
        // A combo box whose items are replaced resets its selection to null and pushes it through
        // the two-way binding. That is the control correcting itself, not somebody choosing
        // nothing - and the list has no empty entry to choose.
        Dictionary<string, object?> record = new() { ["SignType"] = 1 };
        PropertyGridSource source = new()
        {
            Provider = new DomainProvider([Coded("SignType", new PropertyStandardValue(1, "Stop"))]),
        };
        source.SetTarget(record);

        source.FindRow("SignType")!.SelectedStandardValue = null;

        Assert.Equal(1, record["SignType"]);
    }

    [Fact]
    public void AcceptsANullSelectionWhenTheListOffersOne()
    {
        // Add an empty entry to a property that can actually hold nothing, and clearing becomes a
        // real choice again.
        Dictionary<string, object?> record = new() { ["SignType"] = 1 };
        PropertyGridSource source = new()
        {
            Provider = new DomainProvider(
            [
                Coded("SignType", new PropertyStandardValue(null, "(none)"), new PropertyStandardValue(1, "Stop"))
                    with
                    { PropertyType = typeof(int?) },
            ]),
        };
        source.SetTarget(record);
        PropertyGridPropertyRow row = source.FindRow("SignType")!;

        row.SelectedStandardValue = row.StandardValues[0];

        Assert.Null(record["SignType"]);
    }

    [Fact]
    public void WritesNothingToTheOldObjectWhenTheGridIsGivenAnother()
    {
        // The reported symptom: choose a value, switch tables, and the first one had been cleared.
        Dictionary<string, object?> first = new() { ["SignType"] = 1 };
        WritingAccessor accessor = new("SignType");
        PropertyGridSource source = new()
        {
            Provider = new DomainProvider(
            [
                new PropertyDescription
                {
                    Name = "SignType",
                    PropertyType = typeof(int),
                    Accessor = accessor,
                    StandardValues = [new PropertyStandardValue(1, "Stop")],
                },
            ]),
        };
        source.SetTarget(first);
        accessor.Writes = 0;

        source.SetTarget(new Dictionary<string, object?>());

        Assert.Equal(0, accessor.Writes);
        Assert.Equal(1, first["SignType"]);
    }

    [Fact]
    public void KeepsATrueBooleanTrueWhenAFreshCheckBoxPushesItsEmptyState()
    {
        // A check box starts indeterminate and pushes null before it has been told what to show. On
        // a plain bool that write cannot be honoured, and honouring it half way left the box showing
        // a third state the model never had.
        Booleans subject = new();
        PropertyGridSource source = new();
        source.SetTarget(subject);
        PropertyGridPropertyRow row = source.FindRow("Revisada")!;

        List<string?> announced = [];
        row.PropertyChanged += (_, arguments) => announced.Add(arguments.PropertyName);

        row.NullableBoolValue = null;

        Assert.True(subject.Revisada);
        Assert.True(row.NullableBoolValue);

        // And the box is told to read again, or it would keep showing the empty state for good.
        Assert.Contains(nameof(PropertyGridPropertyRow.NullableBoolValue), announced);
    }

    [Fact]
    public void StillLetsANullableBooleanBeCleared()
    {
        Booleans subject = new() { Maybe = true };
        PropertyGridSource source = new();
        source.SetTarget(subject);

        source.FindRow("Maybe")!.NullableBoolValue = null;

        Assert.Null(subject.Maybe);
    }

    [Fact]
    public void KeepsANumberWhenAFreshNumberBoxPushesNotANumber()
    {
        Booleans subject = new() { Count = 7 };
        PropertyGridSource source = new();
        source.SetTarget(subject);
        PropertyGridPropertyRow row = source.FindRow("Count")!;

        row.DoubleValue = double.NaN;

        Assert.Equal(7, subject.Count);
        Assert.False(row.HasErrors);
    }

    private sealed class Booleans
    {
        public bool Revisada { get; set; } = true;

        public bool? Maybe { get; set; }

        public int Count { get; set; }
    }

    private sealed class WritingAccessor(string name) : PropertyAccessor
    {
        internal int Writes { get; set; }

        public override bool CanRead => true;

        public override bool CanWrite => true;

        protected override object? GetValueCore(object target) =>
            ((Dictionary<string, object?>)target).TryGetValue(name, out object? value) ? value : null;

        protected override void SetValueCore(object target, object? value)
        {
            Writes++;
            ((Dictionary<string, object?>)target)[name] = value;
        }
    }

    // ---- dates that are not set ----

    [Fact]
    public void LeavesAnEmptyDateEmpty()
    {
        // A DATE column nobody has filled in is the normal state of a freshly created file, and it
        // has to read as empty rather than as some date.
        PropertyGridSource source = new();
        source.SetTarget(new Moments());

        PropertyGridPropertyRow row = source.FindRow("Missing")!;

        Assert.Null(row.Value);
        Assert.Null(row.DateValue);
        Assert.Null(row.TimeValue);
    }

    [Fact]
    public void ClearsADateThatCanBeCleared()
    {
        Moments subject = new() { Missing = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Local) };
        PropertyGridSource source = new();
        source.SetTarget(subject);

        source.FindRow("Missing")!.DateValue = null;

        Assert.Null(subject.Missing);
    }

    [Fact]
    public void KeepsADateThatCannotBeClearedWhenAFreshPickerPushesNothing()
    {
        // A picker being realized pushes its own empty state before it has been told what to show,
        // and a plain DateTime has nowhere to put it.
        Moments subject = new();
        PropertyGridSource source = new();
        source.SetTarget(subject);
        PropertyGridPropertyRow row = source.FindRow("Stamp")!;

        List<string?> announced = [];
        row.PropertyChanged += (_, arguments) => announced.Add(arguments.PropertyName);

        row.DateValue = null;
        row.TimeValue = null;

        Assert.Equal(new DateTime(2026, 8, 13, 9, 30, 0, DateTimeKind.Local), subject.Stamp);
        Assert.Contains(nameof(PropertyGridPropertyRow.DateValue), announced);
        Assert.Contains(nameof(PropertyGridPropertyRow.TimeValue), announced);
    }

    [Fact]
    public void KeepsTheTimeWhenOnlyTheDayIsPicked()
    {
        // The calendar picks a day and says nothing about the clock, so the time already on the
        // value has to survive.
        Moments subject = new();
        PropertyGridSource source = new();
        source.SetTarget(subject);

        source.FindRow("Stamp")!.DateValue = new DateTimeOffset(new DateTime(2027, 1, 2, 0, 0, 0, DateTimeKind.Local));

        Assert.Equal(new DateTime(2027, 1, 2, 9, 30, 0, DateTimeKind.Local), subject.Stamp);
    }

    private sealed class Moments
    {
        public DateTime? Missing { get; set; }

        public DateTime Stamp { get; set; } = new(2026, 8, 13, 9, 30, 0, DateTimeKind.Local);
    }

    // ---- what must not break ----

    [Fact]
    public void AsksTheProviderAgainEveryTimeEvenForTheSameType()
    {
        // The consumer swaps provider and object together and the type of the object never changes.
        // A cache keyed on the type in between would show it the previous panel's properties.
        DomainProvider first = new([Coded("A")]);
        DomainProvider second = new([Coded("B")]);

        PropertyGridSource source = new() { Provider = first };
        source.SetTarget(new Dictionary<string, object?>());
        Assert.NotNull(source.FindRow("A"));

        source.Provider = second;
        source.SetTarget(new Dictionary<string, object?>());

        Assert.Null(source.FindRow("A"));
        Assert.NotNull(source.FindRow("B"));
    }

    [Fact]
    public void RebuildsWhenGivenAnotherObjectOfTheSameType()
    {
        DomainProvider provider = new([Coded("A")]);
        PropertyGridSource source = new() { Provider = provider };

        source.SetTarget(new Dictionary<string, object?>());
        int afterFirst = provider.Calls;

        source.SetTarget(new Dictionary<string, object?>());

        Assert.True(provider.Calls > afterFirst, "the provider was not asked again");
    }

    [Fact]
    public void KeepsADescriptionBuildableWithAnInitialiserAndAdjustableWithWith()
    {
        PropertyDescription original = Coded("A") with { DisplayName = "Etiqueta" };

        Assert.Equal("Etiqueta", original.DisplayName);
        Assert.Equal("A", original.Name);
    }

    [Fact]
    public void KeepsAPropertyDeclaredAsObjectOutOfTheComplexEditor()
    {
        // Listed by the consumer as working today: an object-typed property gets a text editor.
        PropertyGridSource source = new();
        source.SetTarget(new Plain { Anything = "some text" });

        Assert.Equal(PropertyEditorKeys.String, BuiltInEditors.KeyFor(source.FindRow("Anything")!.Description, typeof(string)));
    }

    // ---- culture, now settable from the control ----

    [Fact]
    public void FormatsAndParsesInTheCultureItWasGiven()
    {
        Plain subject = new();
        PropertyGridSource source = new() { Culture = new CultureInfo("es-ES") };
        source.SetTarget(subject);
        PropertyGridPropertyRow row = source.FindRow("Ratio")!;

        row.Text = "1,5";

        Assert.Equal(1.5, subject.Ratio);
        Assert.Equal("1,5", row.Text);
    }

    [Fact]
    public void PutsUnclassifiedPropertiesInTheCategoryItWasGiven()
    {
        PropertyGridSource source = new() { DefaultCategoryName = "Otros" };
        source.SetTarget(new Plain());

        Assert.Contains(source.Categories, category => category.Name == "Otros");
    }

    private sealed class Plain
    {
        public int Count { get; set; }

        public double Ratio { get; set; }

        public object? Anything { get; set; }
    }
}
