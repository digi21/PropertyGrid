using System.Xml.Linq;

using Xunit;

namespace Digi21.WinUI.PropertyGrid.Tests;

// Everything about the grid's own words, which are resource keys and nothing else.
//
// A key is not a name the compiler checks, so the two ways this can rot silently are both checked
// here: that the list an application is told to walk (PropertyGridText.ResourceKeys) still matches
// the dictionary the library ships, and that a key actually reaches the sentence it is supposed to.
public class LocalisationTests
{
    // Declares the grid's strings the way an application does, and takes them away again. There is
    // no Application in a test to hold a resource dictionary, so the library reads this instead -
    // see PropertyGridThemeResources.Substitute. Test parallelization is off for this assembly
    // because of it.
    private sealed class Translation : IDisposable
    {
        private readonly Dictionary<string, object> entries = new(StringComparer.Ordinal);

        internal Translation()
        {
            PropertyGridThemeResources.Substitute = entries;
        }

        internal Translation Declares(string key, string text)
        {
            entries[key] = text;
            return this;
        }

        public void Dispose() => PropertyGridThemeResources.Substitute = null;
    }

    // The dictionary as it is actually shipped, read back off disk rather than trusted. It is
    // copied next to the test assembly by the WinUI targets, the same file the package carries.
    private static IReadOnlyDictionary<string, string> ShippedStrings()
    {
        string[] found = Directory.GetFiles(
            AppContext.BaseDirectory,
            "PropertyGridResources.xaml",
            SearchOption.AllDirectories);

        Assert.True(found.Length > 0, "PropertyGridResources.xaml was not copied next to the tests.");

        XNamespace xaml = "http://schemas.microsoft.com/winfx/2006/xaml";
        XElement root = XDocument.Load(found[0]).Root!;

        return root
            .Elements(xaml + "String")
            .ToDictionary(element => element.Attribute(xaml + "Key")!.Value, element => element.Value, StringComparer.Ordinal);
    }

    [Fact]
    public void EveryKeyTheGridReadsIsDeclaredInTheDictionaryItShips()
    {
        IReadOnlyDictionary<string, string> shipped = ShippedStrings();

        foreach (string key in PropertyGridText.ResourceKeys)
        {
            Assert.True(shipped.ContainsKey(key), $"{key} is read by the library and declared nowhere.");
            Assert.False(string.IsNullOrWhiteSpace(shipped[key]), $"{key} is declared with nothing in it.");

            // The library falls back to its own copy where there is no application to declare
            // anything - the XAML designer, a test, a console. The two have to say the same thing.
            Assert.Equal(shipped[key], PropertyGridText.Defaults[key]);
        }
    }

    [Fact]
    public void EveryStringTheDictionaryDeclaresIsAKeyAnApplicationIsToldAbout()
    {
        // The other direction, which is the one that rots quietly: a key renamed in the dictionary
        // and not in the list leaves an application overriding a name nobody reads any more.
        // Glyphs are exempt - they are code points in a symbol font, not language.
        foreach ((string key, _) in ShippedStrings())
        {
            if (key.EndsWith("Glyph", StringComparison.Ordinal))
            {
                continue;
            }

            Assert.Contains(key, PropertyGridText.ResourceKeys);
        }
    }

    [Fact]
    public void TheKeysAreListedOnceEach()
    {
        Assert.Equal(PropertyGridText.ResourceKeys.Count, PropertyGridText.ResourceKeys.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void TheFormatStringsTakeThePlaceholdersTheDocumentPromises()
    {
        // A translator reading docs/localisation.md has to know how many they get and in what order,
        // and a wrong count throws at run time rather than at build time.
        IReadOnlyDictionary<string, string> shipped = ShippedStrings();

        Assert.Contains("{0}", shipped["PropertyGridNotAValidFormat"], StringComparison.Ordinal);
        Assert.Contains("{1}", shipped["PropertyGridNotAValidFormat"], StringComparison.Ordinal);
        Assert.Contains("{0}", shipped["PropertyGridRequiredValueFormat"], StringComparison.Ordinal);
        Assert.DoesNotContain("{1}", shipped["PropertyGridRequiredValueFormat"], StringComparison.Ordinal);
        Assert.Contains("{0}", shipped["PropertyGridCannotConvertFormat"], StringComparison.Ordinal);
        Assert.Contains("{1}", shipped["PropertyGridCannotConvertFormat"], StringComparison.Ordinal);
        Assert.Contains("{0}", shipped["PropertyGridCollectionSummaryFormat"], StringComparison.Ordinal);
    }

    [Fact]
    public void NamesTheTypesThePersonReadingWouldName()
    {
        Assert.Equal("whole number", PropertyGridText.NameOf(typeof(int)));
        Assert.Equal("number", PropertyGridText.NameOf(typeof(double)));
        Assert.Equal("text", PropertyGridText.NameOf(typeof(string)));
        Assert.Equal("duration", PropertyGridText.NameOf(typeof(TimeSpan)));

        // A nullable is named after what it holds, not after Nullable`1.
        Assert.Equal("date", PropertyGridText.NameOf(typeof(DateOnly?)));

        // And something with no friendly name falls back to what the runtime calls it.
        Assert.Equal(nameof(Uri), PropertyGridText.NameOf(typeof(Uri)));
    }

    [Fact]
    public void EachTypeNameComesFromItsOwnKey()
    {
        // Names the eight of them apart, so that two keys crossed over - date reading the time key -
        // cannot pass. Nothing else in the suite would notice.
        using Translation translation = new Translation()
            .Declares("PropertyGridWholeNumberName", "entero")
            .Declares("PropertyGridNumberName", "real")
            .Declares("PropertyGridBooleanName", "lógico")
            .Declares("PropertyGridCharacterName", "carácter")
            .Declares("PropertyGridTextName", "texto")
            .Declares("PropertyGridDateTimeName", "fecha y hora")
            .Declares("PropertyGridDateName", "fecha")
            .Declares("PropertyGridTimeName", "hora")
            .Declares("PropertyGridDurationName", "duración");

        Assert.Equal("entero", PropertyGridText.NameOf(typeof(long)));
        Assert.Equal("real", PropertyGridText.NameOf(typeof(decimal)));
        Assert.Equal("lógico", PropertyGridText.NameOf(typeof(bool)));
        Assert.Equal("carácter", PropertyGridText.NameOf(typeof(char)));
        Assert.Equal("texto", PropertyGridText.NameOf(typeof(string)));
        Assert.Equal("fecha y hora", PropertyGridText.NameOf(typeof(DateTimeOffset)));
        Assert.Equal("fecha", PropertyGridText.NameOf(typeof(DateOnly)));
        Assert.Equal("hora", PropertyGridText.NameOf(typeof(TimeOnly)));
        Assert.Equal("duración", PropertyGridText.NameOf(typeof(TimeSpan)));
    }

    [Fact]
    public void AnOverrideDeclaredBeforeAGridExistsIsWhatItSays()
    {
        using Translation translation = new Translation()
            .Declares("PropertyGridDefaultCategoryName", "Varios")
            .Declares("PropertyGridCollectionSummaryFormat", "{0} elementos")
            .Declares("PropertyGridNotAValidFormat", "«{0}» no es un {1} válido.")
            .Declares("PropertyGridWholeNumberName", "número entero");

        PropertyGridSource source = new();
        source.SetTarget(new Uncategorized());

        Assert.Equal("Varios", source.Categories[0].Name);
        Assert.Equal("3 elementos", source.FindRow(nameof(Uncategorized.Items))!.Text);

        PropertyGridPropertyRow row = source.FindRow(nameof(Uncategorized.Count))!;
        row.Text = "abc";
        Assert.Equal("«abc» no es un número entero válido.", row.ErrorMessage);
    }

    [Fact]
    public void AnOverrideDeclaredAfterAGridExistsIsWhatItSaysFromThenOn()
    {
        // The one the compiled statics used to get wrong the other way round: nothing is cached, so
        // an application that declares its strings after the first grid has already been built is
        // not left showing whatever was in force at the moment it was constructed.
        PropertyGridSource source = new();
        source.SetTarget(new Uncategorized());

        Assert.Equal("Misc", source.Categories[0].Name);
        Assert.Equal("Count = 3", source.FindRow(nameof(Uncategorized.Items))!.Text);

        using Translation translation = new Translation()
            .Declares("PropertyGridDefaultCategoryName", "Varios")
            .Declares("PropertyGridCollectionSummaryFormat", "{0} elementos")
            .Declares("PropertyGridRequiredValueFormat", "Hace falta un {0}.")
            .Declares("PropertyGridWholeNumberName", "número entero");

        // A sentence built from now on is in the new language without anything being told to rebuild.
        PropertyGridPropertyRow row = source.FindRow(nameof(Uncategorized.Count))!;
        row.Text = "   ";
        Assert.Equal("Hace falta un número entero.", row.ErrorMessage);

        // And what the rows already show follows the next time they are built, which is what an
        // application switching language at run time does anyway.
        Assert.Equal("Varios", source.DefaultCategoryName);
        source.Refresh();
        Assert.Equal("Varios", source.Categories[0].Name);
        Assert.Equal("3 elementos", source.FindRow(nameof(Uncategorized.Items))!.Text);
    }

    [Fact]
    public void ANameSetOnTheGridItselfStillWinsOverTheKey()
    {
        using Translation translation = new Translation().Declares("PropertyGridDefaultCategoryName", "Varios");

        PropertyGridSource source = new() { DefaultCategoryName = "Otros" };
        source.SetTarget(new Uncategorized());
        Assert.Equal("Otros", source.Categories[0].Name);

        // Clearing it asks for the key back rather than for the English behind the key.
        source.DefaultCategoryName = string.Empty;
        Assert.Equal("Varios", source.Categories[0].Name);
    }

    private sealed class Uncategorized
    {
        public int Count { get; set; } = 1;

        public List<string> Items { get; } = ["one", "two", "three"];
    }
}
