using Xunit;

namespace Digi21.WinUI.PropertyGrid.Tests;

public class EnumTests
{
    private static (PropertyGridSource Source, ConversionSubject Subject) NewGrid()
    {
        ConversionSubject subject = new();
        PropertyGridSource source = new();
        source.SetTarget(subject);
        return (source, subject);
    }

    private static PropertyGridPropertyRow Row(string name) => NewGrid().Source.FindRow(name)!;

    [Fact]
    public void OffersEveryMemberOfTheEnumeration()
    {
        Assert.Equal(["Apple", "Pear", "Peach"], Row("Choice").EnumMembers.Select(member => member.Name));
    }

    [Fact]
    public void LabelsAMemberWithWhatItsAttributesAskFor()
    {
        Assert.Equal(["Apple", "Pear tree", "Peach"], Row("Choice").EnumMembers.Select(member => member.DisplayName));
    }

    [Fact]
    public void ReadsTheSentenceExplainingAMember()
    {
        Assert.Equal("A soft one.", Row("Choice").EnumMembers.Single(member => member.Name == "Peach").Description);
    }

    [Fact]
    public void OffersTheMembersOfANullableEnumeration()
    {
        Assert.Equal(3, Row("MaybeChoice").EnumMembers.Count);
    }

    [Fact]
    public void ReportsWhichMemberIsSelected()
    {
        Assert.Equal("Apple", Row("Choice").SelectedEnumMember?.Name);
    }

    [Fact]
    public void ReportsNoSelectionForANullableEnumerationHoldingNothing()
    {
        Assert.Null(Row("MaybeChoice").SelectedEnumMember);
    }

    [Fact]
    public void WritesTheChosenMember()
    {
        (PropertyGridSource source, ConversionSubject subject) = NewGrid();
        PropertyGridPropertyRow row = source.FindRow("Choice")!;

        row.SelectedEnumMember = row.EnumMembers.Single(member => member.Name == "Peach");

        Assert.Equal(Fruit.Peach, subject.Choice);
    }

    [Fact]
    public void ParsesAMemberNameCaseInsensitively()
    {
        (PropertyGridSource source, ConversionSubject subject) = NewGrid();

        source.FindRow("Choice")!.Text = "pear";

        Assert.Equal(Fruit.Pear, subject.Choice);
    }

    [Fact]
    public void RejectsANameThatIsNotAMember()
    {
        PropertyGridPropertyRow row = Row("Choice");

        row.Text = "Banana";

        Assert.True(row.HasErrors);
        Assert.Equal(Fruit.Apple, row.Value);
    }

    [Fact]
    public void OffersOneEntryPerFlagOfAFlagsEnumeration()
    {
        Assert.Equal(["Read", "Write", "Delete", "All"], Row("Permissions").FlagMembers.Select(flag => flag.Member.Name));
    }

    [Fact]
    public void LeavesTheZeroMemberOutOfTheFlagList()
    {
        // "None" is the absence of every flag, not a flag: a tick box for it could never be
        // unticked, and ticking it would mean nothing.
        Assert.DoesNotContain(Row("Permissions").FlagMembers, flag => flag.Member.Name == "None");
    }

    [Fact]
    public void GivesNoFlagListToAnEnumerationThatIsNotFlags()
    {
        Assert.Empty(Row("Choice").FlagMembers);
    }

    [Fact]
    public void TicksTheFlagsThatAreSet()
    {
        (PropertyGridSource source, ConversionSubject subject) = NewGrid();
        subject.Permissions = Access.Read | Access.Delete;
        source.Refresh();

        PropertyGridPropertyRow row = source.FindRow("Permissions")!;

        Assert.True(row.FlagMembers.Single(flag => flag.Member.Name == "Read").IsChecked);
        Assert.False(row.FlagMembers.Single(flag => flag.Member.Name == "Write").IsChecked);
        Assert.True(row.FlagMembers.Single(flag => flag.Member.Name == "Delete").IsChecked);
    }

    [Fact]
    public void TicksACombinedMemberOnlyWhenEveryFlagInItIsSet()
    {
        (PropertyGridSource source, ConversionSubject subject) = NewGrid();
        subject.Permissions = Access.Read | Access.Write;
        source.Refresh();

        Assert.False(source.FindRow("Permissions")!.FlagMembers.Single(flag => flag.Member.Name == "All").IsChecked);

        subject.Permissions = Access.All;
        source.Refresh();

        Assert.True(source.FindRow("Permissions")!.FlagMembers.Single(flag => flag.Member.Name == "All").IsChecked);
    }

    [Fact]
    public void ComposesTheValueFromTheTickedFlags()
    {
        (PropertyGridSource source, ConversionSubject subject) = NewGrid();
        PropertyGridPropertyRow row = source.FindRow("Permissions")!;

        row.FlagMembers.Single(flag => flag.Member.Name == "Read").IsChecked = true;
        row.FlagMembers.Single(flag => flag.Member.Name == "Write").IsChecked = true;

        Assert.Equal(Access.Read | Access.Write, subject.Permissions);
    }

    [Fact]
    public void ClearsAFlagWhenItIsUnticked()
    {
        (PropertyGridSource source, ConversionSubject subject) = NewGrid();
        subject.Permissions = Access.Read | Access.Write;
        source.Refresh();
        PropertyGridPropertyRow row = source.FindRow("Permissions")!;

        row.FlagMembers.Single(flag => flag.Member.Name == "Write").IsChecked = false;

        Assert.Equal(Access.Read, subject.Permissions);
    }

    [Fact]
    public void AcceptsACombinationNoSingleMemberNames()
    {
        (PropertyGridSource source, ConversionSubject subject) = NewGrid();

        source.FindRow("Permissions")!.Value = Access.Read | Access.Delete;

        Assert.Equal(Access.Read | Access.Delete, subject.Permissions);
    }
}
