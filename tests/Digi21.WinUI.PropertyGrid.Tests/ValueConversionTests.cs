using System.Globalization;
using Xunit;

namespace Digi21.WinUI.PropertyGrid.Tests;

public class ValueConversionTests
{
    private static (PropertyGridSource Source, ConversionSubject Subject) NewGrid(string cultureName = "en-US")
    {
        ConversionSubject subject = new();
        PropertyGridSource source = new()
        {
            Provider = new ReflectionPropertyDescriptionProvider(new PropertyGridMetadata()),
            Culture = new CultureInfo(cultureName),
        };
        source.SetTarget(subject);
        return (source, subject);
    }

    private static PropertyGridPropertyRow Row(string name, string cultureName = "en-US")
    {
        (PropertyGridSource source, _) = NewGrid(cultureName);
        return source.FindRow(name) ?? throw new InvalidOperationException($"no row for {name}");
    }

    [Theory]
    [InlineData("Text", "hello", "hello")]
    [InlineData("Letter", "z", 'z')]
    [InlineData("Flag", "true", true)]
    [InlineData("Whole", "42", 42)]
    [InlineData("Big", "9007199254740993", 9007199254740993L)]
    [InlineData("Real", "1.5", 1.5)]
    public void ParsesTheTypesItSupports(string property, string text, object expected)
    {
        PropertyGridPropertyRow row = Row(property);

        row.Text = text;

        Assert.Equal(expected, row.Value);
        Assert.False(row.HasErrors);
    }

    [Fact]
    public void ParsesADecimalWithoutGoingThroughADouble()
    {
        PropertyGridPropertyRow row = Row("Money");

        row.Text = "79228162514264337593543950335";

        Assert.Equal(decimal.MaxValue, row.Value);
    }

    [Fact]
    public void ParsesTheDecimalSeparatorOfTheGridsCulture()
    {
        PropertyGridPropertyRow row = Row("Real", "es-ES");

        row.Text = "1,5";

        Assert.Equal(1.5, row.Value);
        Assert.False(row.HasErrors);
    }

    [Fact]
    public void RefusesTheOtherCulturesDecimalSeparatorInARealNumber()
    {
        // .NET is lenient about where group separators fall, so accepting them here would read the
        // dot as grouping and silently turn one and a half into fifteen. Rejecting it is the only
        // answer that cannot be wrong.
        PropertyGridPropertyRow row = Row("Real", "es-ES");

        row.Text = "1.5";

        Assert.True(row.HasErrors);
        Assert.Equal(0d, row.Value);
    }

    [Fact]
    public void ReadsTheOtherCulturesDecimalSeparatorAsGroupingInAWholeNumber()
    {
        // In a whole number a dot cannot have meant a decimal point, so it is what es-ES says it
        // is - and refusing "1.000" for a thousand would be obtuse.
        PropertyGridPropertyRow row = Row("Whole", "es-ES");

        row.Text = "1.000";

        Assert.Equal(1000, row.Value);
        Assert.False(row.HasErrors);
    }

    [Fact]
    public void ShowsAValueInTheGridsCulture()
    {
        (PropertyGridSource source, ConversionSubject subject) = NewGrid("es-ES");
        subject.Real = 1.5;
        source.Refresh();

        Assert.Equal("1,5", source.FindRow("Real")!.Text);
    }

    [Theory]
    [InlineData("Identifier", "9f1b6e6e-0000-0000-0000-000000000000")]
    [InlineData("Address", "https://example.com/")]
    [InlineData("Duration", "1.02:03:04")]
    public void ParsesTheTypesWithoutATypeCode(string property, string text)
    {
        PropertyGridPropertyRow row = Row(property);

        row.Text = text;

        Assert.False(row.HasErrors);
        Assert.Equal(text, row.Text);
    }

    [Fact]
    public void ClearsANullablePropertyWhenTheTextIsEmptied()
    {
        PropertyGridPropertyRow row = Row("MaybeWhole");
        row.Text = "7";

        row.Text = string.Empty;

        Assert.Null(row.Value);
        Assert.False(row.HasErrors);
    }

    [Fact]
    public void RefusesToEmptyAPropertyThatCannotBeNull()
    {
        PropertyGridPropertyRow row = Row("Whole");

        row.Text = string.Empty;

        Assert.True(row.HasErrors);
        Assert.Equal(0, row.Value);
    }

    [Fact]
    public void LeavesAStringPropertyEmptyRatherThanNullWhenTheTextIsCleared()
    {
        PropertyGridPropertyRow row = Row("Text");
        row.Text = "something";

        row.Text = string.Empty;

        Assert.Equal(string.Empty, row.Value);
    }

    [Fact]
    public void KeepsTheTypedTextAndTheOldValueWhenTheEditIsRejected()
    {
        (PropertyGridSource source, ConversionSubject subject) = NewGrid();
        subject.Whole = 7;
        source.Refresh();
        PropertyGridPropertyRow row = source.FindRow("Whole")!;

        row.Text = "not a number";

        // Both halves matter. Throwing the text away loses what the user was in the middle of
        // typing; writing the value anyway defeats the point of rejecting it.
        Assert.Equal("not a number", row.Text);
        Assert.Equal(7, subject.Whole);
        Assert.True(row.HasErrors);
    }

    [Fact]
    public void ExplainsWhatWasWrongInWordsRatherThanClrTypeNames()
    {
        PropertyGridPropertyRow row = Row("Whole");

        row.Text = "abc";

        Assert.Equal("'abc' is not a valid whole number.", row.ErrorMessage);
    }

    [Fact]
    public void ClearsTheErrorOnceAGoodValueIsTyped()
    {
        PropertyGridPropertyRow row = Row("Whole");
        row.Text = "abc";

        row.Text = "12";

        Assert.False(row.HasErrors);
        Assert.Equal(12, row.Value);
    }

    [Fact]
    public void CoercesAValueThatArrivesAsTheWrongNumericType()
    {
        PropertyGridPropertyRow row = Row("Whole");

        // A number box hands back a double even for an int property.
        row.Value = 12.0d;

        Assert.Equal(12, row.Value);
        Assert.IsType<int>(row.Value);
    }

    [Fact]
    public void ShowsWhatTheSetterKeptRatherThanWhatWasTyped()
    {
        ClampingSubject subject = new();
        PropertyGridSource source = new();
        source.SetTarget(subject);
        PropertyGridPropertyRow row = source.FindRow("Percentage")!;

        row.Text = "150";

        Assert.Equal(100, row.Value);
        Assert.Equal("100", row.Text);
    }

    [Fact]
    public void RefusesEveryEditOnAReadOnlyGrid()
    {
        (PropertyGridSource source, ConversionSubject subject) = NewGrid();
        source.IsReadOnly = true;
        PropertyGridPropertyRow row = source.FindRow("Whole")!;

        row.Value = 99;

        Assert.Equal(0, subject.Whole);
        Assert.True(row.IsReadOnly);
    }

    [Fact]
    public void ShowsATypeWithNoObjectBehindItAsReadOnly()
    {
        PropertyGridSource source = new();
        source.SetTargetType(typeof(ConversionSubject));

        PropertyGridPropertyRow row = source.FindRow("Whole")!;

        Assert.True(row.IsReadOnly);
        Assert.NotEmpty(source.Rows);
    }

    [Fact]
    public void DescribesAnObjectThatNeverOverrodeToStringByItsShortName()
    {
        // The default ToString is the full type name, which in a value cell is noise nobody reads.
        PropertyGridSource source = new() { ExpansionPolicy = PropertyExpansionPolicy.Attributed };
        source.SetTarget(new NestedSubject());

        Assert.Equal("(AddressSubject)", source.FindRow("Address")!.Text);
    }

    [Fact]
    public void LeavesAMeaningfulToStringAlone()
    {
        (PropertyGridSource source, ConversionSubject subject) = NewGrid();
        subject.Address = new Uri("https://example.com/");
        source.Refresh();

        Assert.Equal("https://example.com/", source.FindRow("Address")!.Text);
    }

    [Fact]
    public void ShowsNothingWithoutATarget()
    {
        PropertyGridSource source = new();

        Assert.Empty(source.Rows);
    }
}
