using Xunit;

namespace Digi21.WinUI.PropertyGrid.Tests;

public class AttributeMetadataTests
{
    private static PropertyDescription Find<T>(string name) =>
        new ReflectionPropertyDescriptionProvider(new PropertyGridMetadata())
            .GetProperties(typeof(T))
            .Single(property => property.Name == name);

    [Fact]
    public void ReadsTheSystemComponentModelAttributes()
    {
        PropertyDescription name = Find<DescribedSubject>("Name");

        Assert.Equal("Full name", name.DisplayName);
        Assert.Equal("What to call the person.", name.HelpText);
        Assert.Equal("Identity", name.CategoryName);
        Assert.Equal(1, name.Order);
    }

    [Fact]
    public void FallsBackToTheClrNameWhenNothingLabelsTheProperty()
    {
        Assert.Equal("Percentage", Find<DescribedSubject>("Percentage").DisplayName);
    }

    [Fact]
    public void LeavesThePositionAtTheEndWhenTheePropertyAsksForNone()
    {
        Assert.Equal(int.MaxValue, Find<DescribedSubject>("Percentage").Order);
    }

    [Fact]
    public void ReadsADefaultValue()
    {
        PropertyDescription withDefault = Find<DescribedSubject>("WithDefault");

        Assert.True(withDefault.HasDefaultValue);
        Assert.Equal(42, withDefault.DefaultValue);
    }

    [Fact]
    public void LeavesADefaultValueUndeclaredWhenThePropertyDoesNotGiveOne()
    {
        Assert.False(Find<DescribedSubject>("Percentage").HasDefaultValue);
    }

    [Fact]
    public void ReadsWhetherAPropertySurvivesAMerge()
    {
        Assert.False(Find<DescribedSubject>("NotMergable").IsMergable);
        Assert.True(Find<DescribedSubject>("Percentage").IsMergable);
    }

    [Fact]
    public void ReadsTheEditorAPropertyAskedFor()
    {
        Assert.Equal("Percent", Find<DescribedSubject>("Percentage").EditorKey);
        Assert.Null(Find<DescribedSubject>("Name").EditorKey);
    }

    [Fact]
    public void ReadsExpandabilityFromTheProperty()
    {
        Assert.True(Find<DescribedSubject>("Openable").IsExpandable);
    }

    [Fact]
    public void ReadsExpandabilityFromTheTypeOfTheProperty()
    {
        Assert.True(Find<DescribedSubject>("OpenByType").IsExpandable);
    }

    [Fact]
    public void LetsThePropertyOverrideTheExpandabilityOfItsType()
    {
        Assert.False(Find<DescribedSubject>("ClosedByProperty").IsExpandable);
    }

    [Fact]
    public void LeavesExpandabilityUndecidedWhenNobodySaysAnything()
    {
        // Null is not "no": it hands the decision to the grid's expansion policy.
        Assert.Null(Find<DescribedSubject>("Name").IsExpandable);
    }

    [Fact]
    public void ReadsAllFourValuesOutOfASingleDisplayAttribute()
    {
        PropertyDescription described = Find<AnnotatedSubject>("FromDisplay");

        Assert.Equal("Etiqueta", described.DisplayName);
        Assert.Equal("Una descripcion.", described.HelpText);
        Assert.Equal("Grupo", described.CategoryName);
        Assert.Equal(3, described.Order);
    }

    [Fact]
    public void PrefersTheSinglePurposeAttributeOverDisplay()
    {
        PropertyDescription both = Find<AnnotatedSubject>("Both");

        Assert.Equal("From DisplayName", both.DisplayName);
        Assert.Equal("From Description", both.HelpText);
        Assert.Equal("From Category", both.CategoryName);
        Assert.Equal(2, both.Order);
    }

    [Fact]
    public void KeepsTheAttributesForValidationAndEditorsToRead()
    {
        Assert.NotNull(Find<DescribedSubject>("Percentage").GetAttribute<PropertyEditorAttribute>());
        Assert.Null(Find<DescribedSubject>("Name").GetAttribute<PropertyEditorAttribute>());
    }
}
