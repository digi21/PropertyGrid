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
