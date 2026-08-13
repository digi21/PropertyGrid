using Xunit;

namespace Digi21.WinUI.PropertyGrid.Tests;

public class FlatteningTests
{
    private static PropertyGridSource GridFor(object subject, PropertySort sort = PropertySort.CategorizedAlphabetical)
    {
        PropertyGridSource source = new() { Sort = sort, ExpansionPolicy = PropertyExpansionPolicy.Attributed };
        source.SetTarget(subject);
        return source;
    }

    private static string[] Keys(PropertyGridSource source) => [.. source.Rows.Select(row => row.Key)];

    [Fact]
    public void PutsHeadersAndPropertiesInOneFlatList()
    {
        PropertyGridSource source = GridFor(new CategorizedSubject());

        Assert.Equal(
            ["[Appearance]", "Height", "Width", "[Behaviour]", "IsEnabled", "[Misc]", "Loose"],
            Keys(source));
    }

    [Fact]
    public void LeavesTheHeadersOutWhenTheGridIsNotShowingCategories()
    {
        PropertyGridSource source = GridFor(new CategorizedSubject(), PropertySort.Alphabetical);

        Assert.DoesNotContain(source.Rows, row => row is PropertyGridCategoryRow);
    }

    [Fact]
    public void HidesThePropertiesOfAClosedCategory()
    {
        PropertyGridSource source = GridFor(new CategorizedSubject());
        PropertyGridCategoryRow appearance = source.Categories.Single(category => category.Name == "Appearance");

        appearance.IsExpanded = false;

        // Removed from the list, not merely hidden: a collapsed row that is still in the collection
        // is still realized and measured, which wrecks the scroll extent of a long grid.
        Assert.Equal(["[Appearance]", "[Behaviour]", "IsEnabled", "[Misc]", "Loose"], Keys(source));
    }

    [Fact]
    public void KeepsACategoryClosedAcrossARebuild()
    {
        PropertyGridSource source = GridFor(new CategorizedSubject());
        source.Categories.Single(category => category.Name == "Appearance").IsExpanded = false;

        source.Sort = PropertySort.Categorized;

        Assert.False(source.Categories.Single(category => category.Name == "Appearance").IsExpanded);
    }

    [Fact]
    public void CountsHowManyPropertiesEachCategoryIsShowing()
    {
        PropertyGridSource source = GridFor(new CategorizedSubject());

        Assert.Equal(2, source.Categories.Single(category => category.Name == "Appearance").VisibleCount);
    }

    [Fact]
    public void OffersToOpenAPropertyThatAsksToBeOpened()
    {
        PropertyGridSource source = GridFor(new NestedSubject());

        Assert.True(source.FindRow("Address")!.IsExpandable);
        Assert.False(source.FindRow("Label")!.IsExpandable);
    }

    [Fact]
    public void SplicesTheChildrenInUnderTheirParentWhenItIsOpened()
    {
        PropertyGridSource source = GridFor(new NestedSubject());

        source.FindRow("Address")!.IsExpanded = true;

        Assert.Equal(["[Misc]", "Address", "Address.City", "Address.Street", "Label"], Keys(source));
    }

    [Fact]
    public void TakesTheChildrenBackOutWhenTheParentIsClosed()
    {
        PropertyGridSource source = GridFor(new NestedSubject());
        PropertyGridPropertyRow address = source.FindRow("Address")!;
        address.IsExpanded = true;

        address.IsExpanded = false;

        Assert.Equal(["[Misc]", "Address", "Label"], Keys(source));
    }

    [Fact]
    public void NestsChildrenOneLevelDeeperThanTheirParent()
    {
        PropertyGridSource source = GridFor(new NestedSubject());
        source.FindRow("Address")!.IsExpanded = true;

        Assert.Equal(0, source.FindRow("Address")!.Depth);
        Assert.Equal(1, source.FindRow("Address.City")!.Depth);
    }

    [Fact]
    public void BuildsChildrenOnlyWhenARowIsFirstOpened()
    {
        PropertyGridSource source = GridFor(new NestedSubject());
        PropertyGridPropertyRow address = source.FindRow("Address")!;

        Assert.Empty(address.Children);

        address.IsExpanded = true;

        Assert.Equal(2, address.Children.Count);
    }

    [Fact]
    public void RefusesToFollowAGraphBackToSomethingAlreadyOnThePath()
    {
        // Without this the chevron on a node pointing back at its own ancestor never runs out, and
        // opening it all the way is an infinite list.
        CyclicSubject root = new() { Name = "root" };
        CyclicSubject child = new() { Name = "child", Next = root };
        root.Next = child;

        PropertyGridSource source = GridFor(root);
        source.FindRow("Next")!.IsExpanded = true;

        Assert.False(source.FindRow("Next.Next")!.IsExpandable);
    }

    [Fact]
    public void RefusesToFollowAGraphBackToTheObjectItIsShowing()
    {
        CyclicSubject root = new();
        root.Next = root;

        PropertyGridSource source = GridFor(root);

        Assert.False(source.FindRow("Next")!.IsExpandable);
    }

    [Fact]
    public void StopsOfferingToGoDeeperOnceTheDepthLimitIsReached()
    {
        DeepSubject root = new();
        DeepSubject current = root;
        for (int level = 0; level < 6; level++)
        {
            current.Child = new DeepSubject();
            current = current.Child;
        }

        PropertyGridSource source = new()
        {
            MaximumExpansionDepth = 3,
            ExpansionPolicy = PropertyExpansionPolicy.Attributed,
        };
        source.SetTarget(root);

        PropertyGridPropertyRow row = source.FindRow("Child")!;
        for (int level = 0; level < 3 && row.IsExpandable; level++)
        {
            row.IsExpanded = true;
            row = (PropertyGridPropertyRow)row.Children.Single(child => child.DisplayName == "Child");
        }

        Assert.Equal(3, row.Depth);
        Assert.False(row.IsExpandable);
    }

    [Fact]
    public void RefusesToOpenAStructBecauseEditingOneWouldWriteToACopy()
    {
        // Until a child can write back through its parent, offering the chevron would produce edits
        // that look accepted and are silently lost.
        PropertyGridSource source = GridFor(new ValueTypeHolder());

        Assert.False(source.FindRow("Point")!.IsExpandable);
    }

    [Fact]
    public void RefusesToOpenACollectionUntilThereIsAnEditorForOne()
    {
        PropertyGridSource source = GridFor(new CollectionHolder());

        Assert.False(source.FindRow("Items")!.IsExpandable);
    }

    [Fact]
    public void OpensNothingWhenTheGridIsToldNotTo()
    {
        PropertyGridSource source = new() { ExpansionPolicy = PropertyExpansionPolicy.None };
        source.SetTarget(new NestedSubject());

        Assert.False(source.FindRow("Address")!.IsExpandable);
    }

    [Fact]
    public void OpensAnythingWorthOpeningWhenToldToBeAutomatic()
    {
        PlainNested subject = new();
        PropertyGridSource source = new() { ExpansionPolicy = PropertyExpansionPolicy.Automatic };
        source.SetTarget(subject);

        Assert.True(source.FindRow("Inner")!.IsExpandable);
    }

    [Fact]
    public void LeavesAPlainObjectClosedUnlessItAsksToBeOpened()
    {
        PropertyGridSource source = GridFor(new PlainNested());

        Assert.False(source.FindRow("Inner")!.IsExpandable);
    }

    [Fact]
    public void GivesEveryRowAKeyThatSurvivesARebuild()
    {
        PropertyGridSource source = GridFor(new CategorizedSubject());
        string[] before = Keys(source);

        source.Refresh();

        Assert.Equal(before, Keys(source));
    }

    [Fact]
    public void PutsBackWhatWasOpenAfterARebuild()
    {
        PropertyGridSource source = GridFor(new NestedSubject());
        source.FindRow("Address")!.IsExpanded = true;

        source.Refresh();

        Assert.True(source.FindRow("Address")!.IsExpanded);
        Assert.Contains("Address.City", Keys(source));
    }

    [Fact]
    public void ClosesEverythingWhenAskedTo()
    {
        PropertyGridSource source = GridFor(new NestedSubject());
        source.FindRow("Address")!.IsExpanded = true;

        source.CollapseAll();

        Assert.DoesNotContain("Address.City", Keys(source));
    }

    [Fact]
    public void AnnouncesThatTheListWasRebuilt()
    {
        PropertyGridSource source = GridFor(new NestedSubject());
        int rebuilds = 0;
        source.RowsChanged = () => rebuilds++;

        source.FindRow("Address")!.IsExpanded = true;

        Assert.Equal(1, rebuilds);
    }

    private sealed class PlainNested
    {
        public InnerThing Inner { get; set; } = new();
    }

    private sealed class InnerThing
    {
        public int Value { get; set; }
    }
}
