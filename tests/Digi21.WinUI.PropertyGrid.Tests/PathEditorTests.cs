using System.ComponentModel;
using Xunit;

namespace Digi21.WinUI.PropertyGrid.Tests;

public class PathEditorTests
{
    private sealed class PathSubject
    {
        [FilePath(FilePathKind.OpenFile, ".gpkg", ".las")]
        public string SourceFile { get; set; } = @"C:\data\parcels.gpkg";

        [FilePath(FilePathKind.SaveFile, ".png")]
        public string? Output { get; set; }

        [FilePath(FilePathKind.Folder)]
        public string WorkingFolder { get; set; } = @"C:\data";

        public FileInfo? File { get; set; }

        public DirectoryInfo? Folder { get; set; }

        [Description("An ordinary string, which must not get the browse button.")]
        public string Label { get; set; } = "not a path";

        [PropertyEditor(PropertyEditorKeys.Dialog)]
        public string Dialogged { get; set; } = "asked for a dialog";
    }

    private static PropertyDescription Describe(string name) =>
        new ReflectionPropertyDescriptionProvider(new PropertyGridMetadata())
            .GetProperties(typeof(PathSubject))
            .Single(property => property.Name == name);

    private static PropertyGridSource NewGrid(PathSubject? subject = null)
    {
        PropertyGridSource source = new();
        source.SetTarget(subject ?? new PathSubject());
        return source;
    }

    [Theory]
    [InlineData("SourceFile")]
    [InlineData("Output")]
    [InlineData("WorkingFolder")]
    public void GivesTheBrowseEditorToAnythingMarkedAsAPath(string name)
    {
        Assert.Equal(PropertyEditorKeys.Path, BuiltInEditors.KeyFor(Describe(name), null));
    }

    [Theory]
    [InlineData("File")]
    [InlineData("Folder")]
    public void GivesItToFileInfoAndDirectoryInfoWithoutBeingAsked(string name)
    {
        // Those types can only mean a path, so making the model say so as well would be ceremony.
        Assert.Equal(PropertyEditorKeys.Path, BuiltInEditors.KeyFor(Describe(name), null));
    }

    [Fact]
    public void LeavesAnOrdinaryStringAlone()
    {
        Assert.Equal(PropertyEditorKeys.String, BuiltInEditors.KeyFor(Describe("Label"), null));
    }

    [Fact]
    public void DoesNotOfferToOpenAFileInfoAsAnObject()
    {
        // Before it was treated as a path, a FileInfo was an object with properties: the row claimed
        // to be editable and offered a read-only summary of CreationTime and friends.
        PropertyGridSource source = new() { ExpansionPolicy = PropertyExpansionPolicy.Automatic };
        source.SetTarget(new PathSubject { File = new FileInfo(@"C:\data\parcels.gpkg") });

        Assert.False(source.FindRow("File")!.IsExpandable);
    }

    [Fact]
    public void CarriesWhatTheAttributeSaysThroughToTheHandler()
    {
        PropertyDescription description = Describe("SourceFile");
        FilePathAttribute? declared = description.GetAttribute<FilePathAttribute>();

        Assert.NotNull(declared);
        Assert.Equal(FilePathKind.OpenFile, declared.Kind);
        Assert.Equal([".gpkg", ".las"], declared.Extensions);
    }

    [Fact]
    public void DefaultsToOpeningAFileWhenTheAttributeSaysNothingElse()
    {
        FilePathAttribute declared = new();

        Assert.Equal(FilePathKind.OpenFile, declared.Kind);
        Assert.Empty(declared.Extensions);
    }

    [Fact]
    public void ShowsAFileInfoAsItsPath()
    {
        PropertyGridSource source = NewGrid(new PathSubject { File = new FileInfo(@"C:\data\parcels.gpkg") });

        Assert.Equal(@"C:\data\parcels.gpkg", source.FindRow("File")!.Text);
    }

    [Fact]
    public void TurnsATypedPathBackIntoAFileInfo()
    {
        // A handler reports what was chosen by writing the row's value, and what a picker hands back
        // is a string whatever the property is declared as.
        PathSubject subject = new();
        PropertyGridSource source = NewGrid(subject);

        source.FindRow("File")!.Value = @"C:\data\chosen.las";

        Assert.Equal(@"C:\data\chosen.las", subject.File?.FullName);
    }

    [Fact]
    public void TurnsATypedPathBackIntoADirectoryInfo()
    {
        PathSubject subject = new();
        PropertyGridSource source = NewGrid(subject);

        source.FindRow("Folder")!.Text = @"C:\data\out";

        Assert.Equal(@"C:\data\out", subject.Folder?.FullName.TrimEnd('\\'));
    }

    [Fact]
    public void AcceptsAPathToSomethingThatDoesNotExistYet()
    {
        // A save box names a file that is about to be written. Refusing it because it is not there
        // would make the editor useless for exactly the case it exists for.
        PathSubject subject = new();
        PropertyGridSource source = NewGrid(subject);
        PropertyGridPropertyRow row = source.FindRow("File")!;

        row.Text = @"C:\data\does-not-exist-yet.png";

        Assert.False(row.HasErrors);
        Assert.Equal(@"C:\data\does-not-exist-yet.png", subject.File?.FullName);
    }

    [Fact]
    public void DoesNotTryToDecideWhetherAPathIsValid()
    {
        // .NET stopped rejecting odd characters when it stopped assuming Windows, and whether a path
        // is usable depends on the file system it lands on rather than on how it is spelt. Guessing
        // here would only refuse paths that would have worked.
        PropertyGridSource source = NewGrid();
        PropertyGridPropertyRow row = source.FindRow("File")!;

        row.Text = "|";

        Assert.False(row.HasErrors);
    }

    [Fact]
    public void SummarisesAListByHowManyThingsAreInIt()
    {
        // Its own ToString is System.Collections.Generic.List`1[System.String], which tells the
        // reader nothing they wanted to know.
        PropertyGridSource source = new();
        source.SetTarget(new CollectionHolder());

        Assert.Equal("Count = 2", source.FindRow("Items")!.Text);
    }

    [Fact]
    public void OffersTheDialogEditorToAListAndToAComplexObject()
    {
        ReflectionPropertyDescriptionProvider provider = new(new PropertyGridMetadata());

        PropertyDescription items = provider.GetProperties(typeof(CollectionHolder)).Single();
        PropertyDescription address = provider.GetProperties(typeof(NestedSubject)).Single(p => p.Name == "Address");

        Assert.Equal(PropertyEditorKeys.Collection, BuiltInEditors.KeyFor(items, null));
        Assert.Equal(PropertyEditorKeys.Complex, BuiltInEditors.KeyFor(address, null));
    }

    [Fact]
    public void LetsAPropertyAskForTheDialogEditorByName()
    {
        Assert.Equal(PropertyEditorKeys.Dialog, Describe("Dialogged").EditorKey);
    }

    [Fact]
    public void OffersToEditAListEvenThoughItHasNoSetter()
    {
        // The usual way to declare a collection is with only a getter, and it is still meant to be
        // edited - by changing what is in it rather than by assigning a new one. Asking whether the
        // property can be assigned would refuse almost every list there is.
        PropertyGridSource source = new();
        source.SetTarget(new LockedCollections());
        PropertyGridPropertyRow row = source.FindRow("Open")!;

        Assert.True(row.IsReadOnly);
        Assert.True(row.AllowsEditing);
    }

    [Fact]
    public void RefusesToEditWhatTheAuthorLockedOrTheGridFroze()
    {
        PropertyGridSource source = new();
        source.SetTarget(new LockedCollections());

        Assert.False(source.FindRow("Locked")!.AllowsEditing);
        Assert.False(source.FindRow("NotEditable")!.AllowsEditing);
        Assert.True(source.FindRow("Open")!.AllowsEditing);

        source.IsReadOnly = true;

        Assert.False(source.FindRow("Open")!.AllowsEditing);
    }

    private sealed class LockedCollections
    {
        [ReadOnly(true)]
        public IList<int> Locked { get; } = [1];

        [System.ComponentModel.DataAnnotations.Editable(false)]
        public IList<int> NotEditable { get; } = [1];

        public IList<int> Open { get; } = [1];
    }

    [Fact]
    public void ClearsANullablePathWhenTheTextIsEmptied()
    {
        PathSubject subject = new() { File = new FileInfo(@"C:\data\parcels.gpkg") };
        PropertyGridSource source = NewGrid(subject);

        source.FindRow("File")!.Text = string.Empty;

        Assert.Null(subject.File);
    }
}
