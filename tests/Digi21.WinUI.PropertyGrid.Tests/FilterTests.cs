using Xunit;

namespace Digi21.WinUI.PropertyGrid.Tests;

public class FilterTests
{
    private static PropertyGridSource GridFor(object subject)
    {
        PropertyGridSource source = new();
        source.SetTarget(subject);
        return source;
    }

    private static string[] Keys(PropertyGridSource source) => [.. source.Rows.Select(row => row.Key)];

    [Fact]
    public void KeepsOnlyThePropertiesWhoseNameMatches()
    {
        PropertyGridSource source = GridFor(new CategorizedSubject());

        source.FilterText = "eight";

        Assert.Equal(["[Appearance]", "Height"], Keys(source));
    }

    [Fact]
    public void MatchesWithoutRegardToCase()
    {
        PropertyGridSource source = GridFor(new CategorizedSubject());

        source.FilterText = "WIDTH";

        Assert.Contains("Width", Keys(source));
    }

    [Fact]
    public void MatchesTheDescriptionAsWellAsTheName()
    {
        PropertyGridSource source = GridFor(new CategorizedSubject());

        source.FilterText = "how wide";

        Assert.Equal(["[Appearance]", "Width"], Keys(source));
    }

    [Fact]
    public void DropsACategoryThatHasNothingLeftInIt()
    {
        PropertyGridSource source = GridFor(new CategorizedSubject());

        source.FilterText = "enabled";

        Assert.Equal(["[Behaviour]", "IsEnabled"], Keys(source));
    }

    [Fact]
    public void ShowsTheMatchesOfAClosedCategoryWhileAFilterIsOn()
    {
        // A collapsed category would hide the very matches the user is searching for, so the filter
        // wins - without discarding the collapsed state, which comes back when the filter clears.
        PropertyGridSource source = GridFor(new CategorizedSubject());
        source.Categories.Single(category => category.Name == "Appearance").IsExpanded = false;

        source.FilterText = "Width";

        Assert.Equal(["[Appearance]", "Width"], Keys(source));
    }

    [Fact]
    public void PutsBackTheClosedCategoryWhenTheFilterIsCleared()
    {
        PropertyGridSource source = GridFor(new CategorizedSubject());
        source.Categories.Single(category => category.Name == "Appearance").IsExpanded = false;
        source.FilterText = "Width";

        source.FilterText = null;

        Assert.Equal(["[Appearance]", "[Behaviour]", "IsEnabled", "[Misc]", "Loose"], Keys(source));
    }

    [Fact]
    public void ShowsEverythingAgainWhenTheFilterIsCleared()
    {
        PropertyGridSource source = GridFor(new CategorizedSubject());
        source.FilterText = "Width";

        source.FilterText = "   ";

        Assert.Equal(7, source.Rows.Count);
    }

    [Fact]
    public void AppliesATestOfItsOwnAsWell()
    {
        PropertyGridSource source = GridFor(new CategorizedSubject());

        source.Filter = row => row.PropertyType == typeof(bool);

        Assert.Equal(["[Behaviour]", "IsEnabled"], Keys(source));
    }

    [Fact]
    public void RequiresBothTheTextAndTheTestToMatch()
    {
        PropertyGridSource source = GridFor(new CategorizedSubject());
        source.Filter = row => row.PropertyType == typeof(int);

        source.FilterText = "Loose";

        Assert.Empty(source.Rows);
    }

    [Fact]
    public void DoesNotRebuildTheRowsJustToFilterThem()
    {
        PropertyGridSource source = GridFor(new CategorizedSubject());
        PropertyGridPropertyRow before = source.FindRow("Width")!;

        source.FilterText = "Width";

        Assert.Same(before, source.FindRow("Width"));
    }
}
