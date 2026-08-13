using Xunit;

namespace Digi21.WinUI.PropertyGrid.Tests;

// Reads every string docs/localisation.md tells a translator to set. It compiles, so the names in
// that document are the names on the class; and nothing here writes to them, because they are static
// and shared and test classes run in parallel.
public class LocalisationTests
{
    public static TheoryData<string> EverySettableString() =>
    [
        PropertyGridStrings.DefaultCategoryName,
        PropertyGridStrings.NotAValidFormat,
        PropertyGridStrings.RequiredValueFormat,
        PropertyGridStrings.CannotConvertFormat,
        PropertyGridStrings.CollectionSummaryFormat,
        PropertyGridStrings.WholeNumberName,
        PropertyGridStrings.NumberName,
        PropertyGridStrings.BooleanName,
        PropertyGridStrings.CharacterName,
        PropertyGridStrings.TextName,
        PropertyGridStrings.DateTimeName,
        PropertyGridStrings.DateName,
        PropertyGridStrings.TimeName,
        PropertyGridStrings.DurationName,
    ];

    [Theory]
    [MemberData(nameof(EverySettableString))]
    public void EveryTranslatableStringHasSomethingInIt(string value)
    {
        Assert.False(string.IsNullOrWhiteSpace(value));
    }

    [Fact]
    public void TheFormatStringsTakeThePlaceholdersTheDocumentPromises()
    {
        // A translator reading the document has to know how many they get and in what order, and a
        // wrong count throws at run time rather than at build time.
        Assert.Contains("{0}", PropertyGridStrings.NotAValidFormat, StringComparison.Ordinal);
        Assert.Contains("{1}", PropertyGridStrings.NotAValidFormat, StringComparison.Ordinal);
        Assert.Contains("{0}", PropertyGridStrings.RequiredValueFormat, StringComparison.Ordinal);
        Assert.DoesNotContain("{1}", PropertyGridStrings.RequiredValueFormat, StringComparison.Ordinal);
        Assert.Contains("{0}", PropertyGridStrings.CannotConvertFormat, StringComparison.Ordinal);
        Assert.Contains("{1}", PropertyGridStrings.CannotConvertFormat, StringComparison.Ordinal);
        Assert.Contains("{0}", PropertyGridStrings.CollectionSummaryFormat, StringComparison.Ordinal);
    }

    [Fact]
    public void NamesTheTypesThePersonReadingWouldName()
    {
        Assert.Equal(PropertyGridStrings.WholeNumberName, PropertyGridStrings.NameOf(typeof(int)));
        Assert.Equal(PropertyGridStrings.NumberName, PropertyGridStrings.NameOf(typeof(double)));
        Assert.Equal(PropertyGridStrings.TextName, PropertyGridStrings.NameOf(typeof(string)));
        Assert.Equal(PropertyGridStrings.DurationName, PropertyGridStrings.NameOf(typeof(TimeSpan)));

        // A nullable is named after what it holds, not after Nullable`1.
        Assert.Equal(PropertyGridStrings.DateName, PropertyGridStrings.NameOf(typeof(DateOnly?)));

        // And something with no friendly name falls back to what the runtime calls it.
        Assert.Equal(nameof(Uri), PropertyGridStrings.NameOf(typeof(Uri)));
    }
}
