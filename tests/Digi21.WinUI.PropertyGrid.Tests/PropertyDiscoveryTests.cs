using Xunit;

namespace Digi21.WinUI.PropertyGrid.Tests;

public class PropertyDiscoveryTests
{
    private static ReflectionPropertyDescriptionProvider NewProvider() => new(new PropertyGridMetadata());

    private static PropertyDescription Find<T>(string name) =>
        NewProvider().GetProperties(typeof(T)).Single(property => property.Name == name);

    private static string[] NamesOf<T>() =>
        [.. NewProvider().GetProperties(typeof(T)).Select(property => property.Name)];

    [Fact]
    public void FindsPublicInstancePropertiesWithAPublicGetter()
    {
        Assert.Contains("Plain", NamesOf<DiscoverySubject>());
    }

    [Theory]
    [InlineData("WriteOnly")]
    [InlineData("Static")]
    [InlineData("Internal")]
    [InlineData("Item")]
    [InlineData("NotBrowsable")]
    [InlineData("HiddenFromEditors")]
    public void SkipsWhatCannotOrShouldNotBeShown(string name)
    {
        Assert.DoesNotContain(name, NamesOf<DiscoverySubject>());
    }

    [Theory]
    [InlineData("NoSetter")]
    [InlineData("PrivateSetter")]
    [InlineData("MarkedReadOnly")]
    [InlineData("NotEditable")]
    public void TreatsUnwritablePropertiesAsReadOnly(string name)
    {
        Assert.True(Find<DiscoverySubject>(name).IsReadOnly);
    }

    [Fact]
    public void TreatsAnInitOnlySetterAsReadOnly()
    {
        // An init accessor is a public setter as far as reflection is concerned, and invoking it
        // after construction succeeds. Only the modreq on its return parameter says otherwise.
        Assert.True(Find<DiscoverySubject>("InitOnly").IsReadOnly);
    }

    [Fact]
    public void TreatsAnOrdinaryPropertyAsWritable()
    {
        Assert.False(Find<DiscoverySubject>("Plain").IsReadOnly);
    }

    [Fact]
    public void ListsAPropertyHiddenByNewOnlyOnce()
    {
        string[] names = NamesOf<ShadowDerived>();

        Assert.Single(names, name => name == "Value");
    }

    [Fact]
    public void BindsAPropertyHiddenByNewToTheMostDerivedDeclaration()
    {
        PropertyDescription value = Find<ShadowDerived>("Value");
        ShadowDerived subject = new();

        Assert.Equal(typeof(ShadowDerived), value.DeclaringType);
        Assert.Equal("derived", value.Accessor.GetValue(subject));
    }

    [Fact]
    public void ListsInheritedPropertiesBeforeDeclaredOnes()
    {
        string[] names = NamesOf<ShadowDerived>();

        Assert.True(
            Array.IndexOf(names, "OnlyOnBase") < Array.IndexOf(names, "OnlyOnDerived"),
            $"expected the base property first, got [{string.Join(", ", names)}]");
    }

    [Fact]
    public void KeepsDeclarationOrder()
    {
        Assert.Equal(["Zebra", "Apple", "Mango"], NamesOf<UnorderedSubject>());
    }

    [Fact]
    public void ListsThePropertiesOfEveryInterfaceAnInterfaceExtends()
    {
        // GetProperties on an interface stops at that one interface, unlike on a class, so a grid
        // handed a variable typed as IDescribed would otherwise never see Name.
        Assert.Equal(["Name", "Summary"], NamesOf<IDescribed>().Order());
    }

    [Fact]
    public void ReadsAValueThroughTheAccessor()
    {
        DiscoverySubject subject = new() { Plain = "hello" };

        Assert.Equal("hello", Find<DiscoverySubject>("Plain").Accessor.GetValue(subject));
    }

    [Fact]
    public void WritesAValueThroughTheAccessor()
    {
        DiscoverySubject subject = new();

        Find<DiscoverySubject>("Plain").Accessor.SetValue(subject, "written");

        Assert.Equal("written", subject.Plain);
    }

    [Fact]
    public void ReportsAThrowingGetterInsteadOfLettingItEscape()
    {
        ThrowingSubject subject = new();

        bool read = Find<ThrowingSubject>("Broken").Accessor.TryGetValue(subject, out object? value, out Exception? error);

        Assert.False(read);
        Assert.Null(value);
        Assert.Equal("the getter said no", error?.Message);
    }

    [Fact]
    public void UnwrapsWhatReflectionWrappedAroundAThrowingSetter()
    {
        ThrowingSubject subject = new();

        bool written = Find<ThrowingSubject>("Rejecting").Accessor.TrySetValue(subject, "x", out Exception? error);

        Assert.False(written);
        Assert.IsType<ArgumentException>(error);
        Assert.Contains("the setter said no", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void OneBrokenPropertyDoesNotStopTheOthersFromBeingListed()
    {
        Assert.Equal(["Fine", "Broken", "Rejecting"], NamesOf<ThrowingSubject>());
    }

    [Fact]
    public void ReusesTheDescriptionsItAlreadyBuiltForAType()
    {
        ReflectionPropertyDescriptionProvider provider = NewProvider();

        Assert.Same(provider.GetProperties(typeof(DiscoverySubject)), provider.GetProperties(typeof(DiscoverySubject)));
    }

    [Fact]
    public void RebuildsTheDescriptionsAfterBeingInvalidated()
    {
        ReflectionPropertyDescriptionProvider provider = NewProvider();
        IReadOnlyList<PropertyDescription> first = provider.GetProperties(typeof(DiscoverySubject));

        provider.Invalidate(typeof(DiscoverySubject));

        Assert.NotSame(first, provider.GetProperties(typeof(DiscoverySubject)));
    }
}
