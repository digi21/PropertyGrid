using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Digi21.WinUI.PropertyGrid.Tests;

public class EditorResolutionTests
{
    private static PropertyDescription Describe<T>(string name) =>
        new ReflectionPropertyDescriptionProvider(new PropertyGridMetadata())
            .GetProperties(typeof(T))
            .Single(property => property.Name == name);

    private static string KeyFor<T>(string name, Type? runtimeType = null) =>
        BuiltInEditors.KeyFor(Describe<T>(name), runtimeType);

    [Theory]
    [InlineData("Text", PropertyEditorKeys.String)]
    [InlineData("Letter", PropertyEditorKeys.String)]
    [InlineData("Flag", PropertyEditorKeys.Boolean)]
    [InlineData("MaybeFlag", PropertyEditorKeys.NullableBoolean)]
    [InlineData("Whole", PropertyEditorKeys.Number)]
    [InlineData("MaybeWhole", PropertyEditorKeys.Number)]
    [InlineData("Real", PropertyEditorKeys.Number)]
    [InlineData("Big", PropertyEditorKeys.LargeNumber)]
    [InlineData("Money", PropertyEditorKeys.LargeNumber)]
    [InlineData("Choice", PropertyEditorKeys.Enum)]
    [InlineData("MaybeChoice", PropertyEditorKeys.Enum)]
    [InlineData("Permissions", PropertyEditorKeys.FlagsEnum)]
    [InlineData("Moment", PropertyEditorKeys.DateTime)]
    [InlineData("Stamp", PropertyEditorKeys.DateTime)]
    [InlineData("Day", PropertyEditorKeys.Date)]
    [InlineData("Clock", PropertyEditorKeys.Time)]
    [InlineData("Duration", PropertyEditorKeys.TimeSpan)]
    [InlineData("Identifier", PropertyEditorKeys.String)]
    [InlineData("Address", PropertyEditorKeys.String)]
    public void PicksTheBuiltInEditorForEachType(string property, string expected)
    {
        Assert.Equal(expected, KeyFor<ConversionSubject>(property));
    }

    [Fact]
    public void KeepsTheLargeNumericTypesAwayFromTheNumberBox()
    {
        // A number box works in doubles. Routing a long or a decimal through it would drop the low
        // bits of anything past 2^53 without any sign that it happened.
        Assert.Equal(PropertyEditorKeys.LargeNumber, KeyFor<ConversionSubject>("Big"));
        Assert.Equal(PropertyEditorKeys.LargeNumber, KeyFor<ConversionSubject>("Money"));
    }

    [Fact]
    public void UnwrapsANullableBeforeChoosing()
    {
        Assert.Equal(KeyFor<ConversionSubject>("Whole"), KeyFor<ConversionSubject>("MaybeWhole"));
    }

    [Fact]
    public void GivesANullableBooleanItsOwnThreeStateEditor()
    {
        // The one place unwrapping is wrong: the third state is the whole point of the editor.
        Assert.NotEqual(KeyFor<ConversionSubject>("Flag"), KeyFor<ConversionSubject>("MaybeFlag"));
    }

    [Theory]
    [InlineData("Notes", PropertyEditorKeys.MultilineString)]
    [InlineData("Secret", PropertyEditorKeys.Password)]
    [InlineData("AlsoSecret", PropertyEditorKeys.Password)]
    [InlineData("Plain", PropertyEditorKeys.String)]
    public void ReadsWhatTheAttributesSayAboutAStringProperty(string property, string expected)
    {
        Assert.Equal(expected, KeyFor<TextShapes>(property));
    }

    [Fact]
    public void TakesTheBarePasswordAttributeAtItsWord()
    {
        // [PasswordPropertyText] with no argument means Password = false, not true. It reads like an
        // opt-in and is the opposite, so the check has to look at the value rather than at presence.
        Assert.Equal(PropertyEditorKeys.String, KeyFor<TextShapes>("NotActuallySecret"));
    }

    [Fact]
    public void OffersACollectionItsOwnEditor()
    {
        Assert.Equal(PropertyEditorKeys.Collection, KeyFor<CollectionHolder>("Items"));
    }

    [Fact]
    public void OffersAnObjectWithPropertiesTheComplexEditor()
    {
        Assert.Equal(PropertyEditorKeys.Complex, KeyFor<NestedSubject>("Address"));
    }

    [Fact]
    public void LooksAtWhatIsInAVaguelyDeclaredPropertyRatherThanAtTheDeclaration()
    {
        Assert.Equal(PropertyEditorKeys.Complex, KeyFor<VagueShapes>("Anything", typeof(AddressSubject)));
    }

    [Fact]
    public void FallsBackToSelectableTextForSomethingItCannotEdit()
    {
        Assert.Equal(PropertyEditorKeys.ReadOnly, KeyFor<VagueShapes>("Anything"));
    }

    [Fact]
    public void UsesADropDownWhenATypeConverterSaysTheValuesAreFixed()
    {
        Assert.Equal(PropertyEditorKeys.StandardValues, KeyFor<VagueShapes>("Restricted"));
    }

    [Fact]
    public void UsesATextBoxForATypeThatRoundTripsThroughOne()
    {
        Assert.Equal(PropertyEditorKeys.String, KeyFor<VagueShapes>("Convertible"));
    }

    // ---- the registered-editor matching order ----

    private static int Resolve(
        (Type? TargetType, string? Key, bool MatchDerived)[] entries,
        Type declaredType,
        Type? runtimeType = null,
        string? explicitKey = null)
    {
        EditorCriteria[] criteria = [.. entries.Select(entry => new EditorCriteria(entry.TargetType, entry.Key, entry.MatchDerived))];
        return PropertyEditorMatching.Resolve(criteria, declaredType, runtimeType, explicitKey);
    }

    [Fact]
    public void FindsNothingInAnEmptyMap()
    {
        Assert.Equal(PropertyEditorMatching.NoMatch, Resolve([], typeof(int)));
    }

    [Fact]
    public void MatchesTheDeclaredTypeExactly()
    {
        Assert.Equal(1, Resolve([(typeof(string), null, false), (typeof(int), null, false)], typeof(int)));
    }

    [Fact]
    public void PrefersTheNameThePropertyAskedForOverItsType()
    {
        Assert.Equal(0, Resolve([(null, "Percent", false), (typeof(int), null, false)], typeof(int), explicitKey: "Percent"));
    }

    [Fact]
    public void FallsBackToTheTypeWhenNothingIsRegisteredUnderTheRequestedName()
    {
        Assert.Equal(0, Resolve([(typeof(int), null, false)], typeof(int), explicitKey: "Missing"));
    }

    [Fact]
    public void PrefersWhatIsInAVaguelyDeclaredPropertyOverTheDeclaration()
    {
        Assert.Equal(
            1,
            Resolve([(typeof(object), null, false), (typeof(string), null, false)], typeof(object), typeof(string)));
    }

    [Fact]
    public void IgnoresTheRuntimeTypeWhenTheDeclarationIsSpecificEnough()
    {
        // A property declared as string holds a string; there is nothing more specific to learn, and
        // letting the runtime type win would make a derived type quietly change the editor.
        Assert.Equal(0, Resolve([(typeof(string), null, false), (typeof(object), null, false)], typeof(string), typeof(string)));
    }

    [Fact]
    public void MatchesANullableInItsOwnRightBeforeUnwrappingIt()
    {
        Assert.Equal(
            1,
            Resolve([(typeof(int), null, false), (typeof(int?), null, false)], typeof(int?)));
    }

    [Fact]
    public void UnwrapsANullableWhenNothingIsRegisteredForIt()
    {
        Assert.Equal(0, Resolve([(typeof(int), null, false)], typeof(int?)));
    }

    [Fact]
    public void WalksUpToABaseClassOnlyWhenTheEntryOptedIn()
    {
        Assert.Equal(PropertyEditorMatching.NoMatch, Resolve([(typeof(ShadowBase), null, false)], typeof(ShadowDerived)));
        Assert.Equal(0, Resolve([(typeof(ShadowBase), null, true)], typeof(ShadowDerived)));
    }

    [Fact]
    public void WalksToAnInterfaceOnlyWhenTheEntryOptedIn()
    {
        Assert.Equal(PropertyEditorMatching.NoMatch, Resolve([(typeof(INamed), null, false)], typeof(NamedThing)));
        Assert.Equal(0, Resolve([(typeof(INamed), null, true)], typeof(NamedThing)));
    }

    [Fact]
    public void PrefersTheNearestBaseClass()
    {
        Assert.Equal(
            1,
            Resolve([(typeof(object), null, true), (typeof(ShadowBase), null, true)], typeof(ShadowDerived)));
    }

    [Fact]
    public void PrefersTheMostDerivedInterface()
    {
        // IDescribed extends INamed, so a type implementing both should get the IDescribed editor
        // whichever order GetInterfaces happens to hand them back in.
        Assert.Equal(
            0,
            Resolve([(typeof(IDescribed), null, true), (typeof(INamed), null, true)], typeof(DescribedThing)));
        Assert.Equal(
            1,
            Resolve([(typeof(INamed), null, true), (typeof(IDescribed), null, true)], typeof(DescribedThing)));
    }

    [Fact]
    public void LetsTheLastRegistrationWin()
    {
        Assert.Equal(1, Resolve([(typeof(int), null, false), (typeof(int), null, false)], typeof(int)));
    }

    [Fact]
    public void PrefersAClassMatchOverAnInterfaceOne()
    {
        Assert.Equal(
            0,
            Resolve([(typeof(ShadowBase), null, true), (typeof(INamed), null, true)], typeof(NamedDerived)));
    }

    private sealed class TextShapes
    {
        public string Plain { get; set; } = string.Empty;

        [DataType(System.ComponentModel.DataAnnotations.DataType.MultilineText)]
        public string Notes { get; set; } = string.Empty;

        [PasswordPropertyText(true)]
        public string Secret { get; set; } = string.Empty;

        [PasswordPropertyText]
        public string NotActuallySecret { get; set; } = string.Empty;

        [DataType(System.ComponentModel.DataAnnotations.DataType.Password)]
        public string AlsoSecret { get; set; } = string.Empty;
    }

    private sealed class VagueShapes
    {
        public object? Anything { get; set; }

        public Restricted Restricted { get; set; }

        public Convertible? Convertible { get; set; }
    }

    [TypeConverter(typeof(RestrictedConverter))]
    private struct Restricted;

    private sealed class RestrictedConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context) => true;

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context) => true;

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context) =>
            new(new[] { "one", "two" });
    }

    [TypeConverter(typeof(ConvertibleConverter))]
    private sealed class Convertible;

    private sealed class ConvertibleConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
            sourceType == typeof(string);

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
            destinationType == typeof(string);
    }

    private sealed class NamedThing : INamed
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class DescribedThing : IDescribed
    {
        public string Name { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;
    }

    private sealed class NamedDerived : ShadowBase, INamed
    {
        public string Name { get; set; } = string.Empty;
    }
}
