using Xunit;

namespace Digi21.WinUI.PropertyGrid.Tests;

public class MetadataStoreTests
{
    private static PropertyDescription Find<T>(PropertyGridMetadata metadata, string name) =>
        new ReflectionPropertyDescriptionProvider(metadata)
            .GetProperties(typeof(T))
            .Single(property => property.Name == name);

    [Fact]
    public void OverridesWhatTheAttributesSaid()
    {
        PropertyGridMetadata metadata = new();
        metadata.For<DescribedSubject>().Property(subject => subject.Name, property => property.DisplayName("Nombre"));

        Assert.Equal("Nombre", Find<DescribedSubject>(metadata, "Name").DisplayName);
    }

    [Fact]
    public void LeavesUnmentionedFieldsAlone()
    {
        PropertyGridMetadata metadata = new();
        metadata.For<DescribedSubject>().Property(subject => subject.Name, property => property.DisplayName("Nombre"));

        PropertyDescription name = Find<DescribedSubject>(metadata, "Name");

        Assert.Equal("What to call the person.", name.HelpText);
        Assert.Equal("Identity", name.CategoryName);
    }

    [Fact]
    public void HidesAPropertyThatWasAskedToBeIgnored()
    {
        PropertyGridMetadata metadata = new();
        metadata.For<DescribedSubject>().Ignore(subject => subject.Percentage);

        Assert.DoesNotContain(
            new ReflectionPropertyDescriptionProvider(metadata).GetProperties(typeof(DescribedSubject)),
            property => property.Name == "Percentage");
    }

    [Fact]
    public void BringsBackAPropertyHiddenByBrowsableFalse()
    {
        PropertyGridMetadata metadata = new();
        metadata.For<DiscoverySubject>().Property(nameof(DiscoverySubject.NotBrowsable), property => property.Browsable());

        Assert.Contains(
            new ReflectionPropertyDescriptionProvider(metadata).GetProperties(typeof(DiscoverySubject)),
            property => property.Name == "NotBrowsable");
    }

    [Fact]
    public void AppliesARuleWrittenForABaseTypeToItsSubclasses()
    {
        PropertyGridMetadata metadata = new();
        metadata.For<ShadowBase>().Property(subject => subject.OnlyOnBase, property => property.Category("Inherited"));

        Assert.Equal("Inherited", Find<ShadowDerived>(metadata, "OnlyOnBase").CategoryName);
    }

    [Fact]
    public void LetsARuleWrittenForASubclassWinOverOneWrittenForItsBase()
    {
        PropertyGridMetadata metadata = new();
        metadata.For<ShadowBase>().Property(subject => subject.OnlyOnBase, property => property.Category("Base"));
        metadata.For<ShadowDerived>().Property(nameof(ShadowBase.OnlyOnBase), property => property.Category("Derived"));

        Assert.Equal("Derived", Find<ShadowDerived>(metadata, "OnlyOnBase").CategoryName);
    }

    [Fact]
    public void AppliesARuleWrittenForAnInterface()
    {
        PropertyGridMetadata metadata = new();
        metadata.For<INamed>().Property(subject => subject.Name, property => property.DisplayName("Named"));

        Assert.Equal("Named", Find<IDescribed>(metadata, "Name").DisplayName);
    }

    [Fact]
    public void ChangesItsVersionWheneverARuleIsAdded()
    {
        PropertyGridMetadata metadata = new();
        int before = metadata.Version;

        metadata.For<DescribedSubject>().Property(subject => subject.Name, property => property.DisplayName("Nombre"));

        Assert.NotEqual(before, metadata.Version);
    }

    [Fact]
    public void MakesAProviderRebuildWhatItCachedWhenTheStoreChanges()
    {
        PropertyGridMetadata metadata = new();
        ReflectionPropertyDescriptionProvider provider = new(metadata);

        Assert.Equal("Full name", provider.GetProperties(typeof(DescribedSubject)).Single(p => p.Name == "Name").DisplayName);

        metadata.For<DescribedSubject>().Property(subject => subject.Name, property => property.DisplayName("Nombre"));

        Assert.Equal("Nombre", provider.GetProperties(typeof(DescribedSubject)).Single(p => p.Name == "Name").DisplayName);
    }

    [Fact]
    public void ForgetsEverythingWhenCleared()
    {
        PropertyGridMetadata metadata = new();
        metadata.For<DescribedSubject>().Property(subject => subject.Name, property => property.DisplayName("Nombre"));

        metadata.Clear();

        Assert.Equal("Full name", Find<DescribedSubject>(metadata, "Name").DisplayName);
    }

    [Fact]
    public void SetsEveryFieldItOffers()
    {
        PropertyGridMetadata metadata = new();
        metadata.For<DescribedSubject>().Property(
            subject => subject.Percentage,
            property => property
                .DisplayName("Zoom")
                .Description("How far in.")
                .Category("View")
                .Order(5)
                .ReadOnly()
                .Mergable(false)
                .Editor("Slider")
                .Expandable()
                .DefaultValue(100));

        PropertyDescription percentage = Find<DescribedSubject>(metadata, "Percentage");

        Assert.Equal("Zoom", percentage.DisplayName);
        Assert.Equal("How far in.", percentage.HelpText);
        Assert.Equal("View", percentage.CategoryName);
        Assert.Equal(5, percentage.Order);
        Assert.True(percentage.IsReadOnly);
        Assert.False(percentage.IsMergable);
        Assert.Equal("Slider", percentage.EditorKey);
        Assert.True(percentage.IsExpandable);
        Assert.True(percentage.HasDefaultValue);
        Assert.Equal(100, percentage.DefaultValue);
    }

    [Fact]
    public void RefusesAnExpressionThatDoesNotSelectAProperty()
    {
        PropertyGridMetadata metadata = new();

        Assert.Throws<ArgumentException>(() =>
            metadata.For<DescribedSubject>().Property(subject => subject.Name.Length.ToString(), property => property.Order(1)));
    }
}
