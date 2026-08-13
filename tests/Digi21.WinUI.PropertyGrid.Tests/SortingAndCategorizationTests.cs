using Xunit;

namespace Digi21.WinUI.PropertyGrid.Tests;

public class SortingAndCategorizationTests
{
    private static string[] Arrange<T>(PropertySort sort)
    {
        IReadOnlyList<PropertyDescription> properties =
            new ReflectionPropertyDescriptionProvider(new PropertyGridMetadata()).GetProperties(typeof(T));

        return [.. PropertyDescriptionSorter.Sort(properties, sort).Select(property => property.Name)];
    }

    private static string[] Categories<T>(PropertySort sort)
    {
        IReadOnlyList<PropertyDescription> properties =
            new ReflectionPropertyDescriptionProvider(new PropertyGridMetadata()).GetProperties(typeof(T));

        return [.. PropertyDescriptionSorter
            .Sort(properties, sort)
            .Select(property => PropertyDescriptionSorter.CategoryOf(property, PropertyDescriptionSorter.DefaultCategoryName))
            .Distinct()];
    }

    [Fact]
    public void LeavesDeclarationOrderAloneWhenNotSorting()
    {
        Assert.Equal(["Zebra", "Apple", "Mango"], Arrange<UnorderedSubject>(PropertySort.NoSort));
    }

    [Fact]
    public void SortsByDisplayName()
    {
        Assert.Equal(["Apple", "Mango", "Zebra"], Arrange<UnorderedSubject>(PropertySort.Alphabetical));
    }

    [Fact]
    public void KeepsCategoriesInTheOrderTheyFirstAppear()
    {
        Assert.Equal(["Zulu", "Alpha", "Misc"], Categories<UncategorizedFirstSubject>(PropertySort.Categorized));
    }

    [Fact]
    public void SortsCategoriesByNameWhenAskedTo()
    {
        Assert.Equal(["Alpha", "Zulu", "Misc"], Categories<UncategorizedFirstSubject>(PropertySort.CategorizedAlphabetical));
    }

    [Fact]
    public void PutsTheCatchAllCategoryLast()
    {
        // Loose is declared first and has no category. A pile of unclassified properties above the
        // ones somebody bothered to classify reads as an accident, so it goes to the bottom.
        Assert.Equal("Loose", Arrange<UncategorizedFirstSubject>(PropertySort.Categorized)[^1]);
        Assert.Equal("Loose", Arrange<UncategorizedFirstSubject>(PropertySort.CategorizedAlphabetical)[^1]);
    }

    [Fact]
    public void KeepsThePropertiesOfACategoryTogether()
    {
        string[] arranged = Arrange<UncategorizedFirstSubject>(PropertySort.Categorized);

        Assert.Equal(["Zebra", "Zulu", "Apple", "Loose"], arranged);
    }

    [Fact]
    public void HonoursAnExplicitPositionInEveryMode()
    {
        foreach (PropertySort sort in Enum.GetValues<PropertySort>())
        {
            // Name asks for position 1 and every other property asks for nothing, so it leads its
            // category whatever the mode. An explicit order is a statement, not a preference.
            string[] arranged = Arrange<DescribedSubject>(sort);
            int name = Array.IndexOf(arranged, "Name");
            int percentage = Array.IndexOf(arranged, "Percentage");

            Assert.True(name < percentage, $"{sort}: expected Name before Percentage, got [{string.Join(", ", arranged)}]");
        }
    }

    [Fact]
    public void BreaksTiesByDeclarationOrderWhenNotSortingByName()
    {
        Assert.Equal(["Zebra", "Apple", "Mango"], Arrange<UnorderedSubject>(PropertySort.Categorized));
    }

    [Fact]
    public void LeavesASinglePropertyAlone()
    {
        IReadOnlyList<PropertyDescription> one =
            [new ReflectionPropertyDescriptionProvider(new PropertyGridMetadata()).GetProperties(typeof(UnorderedSubject))[0]];

        Assert.Same(one, PropertyDescriptionSorter.Sort(one, PropertySort.Alphabetical));
    }

    [Fact]
    public void UsesTheCategoryNameItIsGivenForPropertiesWithoutOne()
    {
        IReadOnlyList<PropertyDescription> properties =
            new ReflectionPropertyDescriptionProvider(new PropertyGridMetadata()).GetProperties(typeof(UnorderedSubject));

        Assert.All(
            properties,
            property => Assert.Equal("Otros", PropertyDescriptionSorter.CategoryOf(property, "Otros")));
    }
}
