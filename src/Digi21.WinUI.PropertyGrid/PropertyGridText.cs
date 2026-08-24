using System.Globalization;

namespace Digi21.WinUI.PropertyGrid;

/// <summary>Every sentence the grid can put in front of a user, and the resource keys it reads them from.</summary>
/// <remarks>
/// <para>
/// Everything the grid says on its own account comes from a resource key, whether a template paints
/// it or the grid builds the sentence at run time. Redeclare a key and the grid says your words
/// instead, exactly the way redeclaring a brush recolours it:
/// </para>
/// <code language="xml">
/// &lt;x:String x:Key="PropertyGridDefaultCategoryName"&gt;Varios&lt;/x:String&gt;
/// &lt;x:String x:Key="PropertyGridNotAValidFormat"&gt;«{0}» no es un {1} válido.&lt;/x:String&gt;
/// </code>
/// <para>
/// Nothing is cached. A key is read where it is used, so an override reaches every sentence built
/// from then on, whether it was declared before the first grid existed or after.
/// </para>
/// <para>
/// The grid does not translate what you put in it: a property's name, its category and its
/// description are yours, and reach the grid already in whatever language you chose.
/// </para>
/// </remarks>
public static class PropertyGridText
{
    // What a template paints.
    private const string SearchPlaceholderTextKey = "PropertyGridSearchPlaceholderText";
    private const string SelectDatePlaceholderTextKey = "PropertyGridSelectDatePlaceholderText";
    private const string BrowseToolTipTextKey = "PropertyGridBrowseToolTipText";
    private const string EditToolTipTextKey = "PropertyGridEditToolTipText";
    private const string OkButtonTextKey = "PropertyGridOkButtonText";
    private const string CancelButtonTextKey = "PropertyGridCancelButtonText";

    // What the grid builds at run time - the reasons an edit was rejected, mostly.
    private const string DefaultCategoryNameKey = "PropertyGridDefaultCategoryName";
    private const string NotAValidFormatKey = "PropertyGridNotAValidFormat";
    private const string RequiredValueFormatKey = "PropertyGridRequiredValueFormat";
    private const string CannotConvertFormatKey = "PropertyGridCannotConvertFormat";
    private const string CollectionSummaryFormatKey = "PropertyGridCollectionSummaryFormat";
    private const string WholeNumberNameKey = "PropertyGridWholeNumberName";
    private const string NumberNameKey = "PropertyGridNumberName";
    private const string BooleanNameKey = "PropertyGridBooleanName";
    private const string CharacterNameKey = "PropertyGridCharacterName";
    private const string TextNameKey = "PropertyGridTextName";
    private const string DateTimeNameKey = "PropertyGridDateTimeName";
    private const string DateNameKey = "PropertyGridDateName";
    private const string TimeNameKey = "PropertyGridTimeName";
    private const string DurationNameKey = "PropertyGridDurationName";

    // Every key, in the order docs/theming.md lists them, with the English it falls back to when
    // nothing declares it. The same defaults are written out in Themes/PropertyGridResources.xaml,
    // which is what an application actually overrides; LocalisationTests reads that file and holds
    // the two to the same list, so a key renamed in one and not the other fails the build.
    //
    // Deliberately absent: a string for "several objects disagree" and one for "not set". Both were
    // declared before anything used them, and a key that never reaches the screen is worse than none
    // - it is a translator's afternoon spent on nothing. The first comes back with multiple
    // selection; the second when a summary cell has somewhere to put it that is not the text a user
    // edits.
    private static readonly (string Key, string Default)[] Entries =
    [
        (SearchPlaceholderTextKey, "Search properties"),
        (SelectDatePlaceholderTextKey, "Pick a date"),
        (BrowseToolTipTextKey, "Browse…"),
        (EditToolTipTextKey, "Edit…"),
        (OkButtonTextKey, "OK"),
        (CancelButtonTextKey, "Cancel"),
        (DefaultCategoryNameKey, "Misc"),
        (NotAValidFormatKey, "'{0}' is not a valid {1}."),
        (RequiredValueFormatKey, "A {0} is required."),
        (CannotConvertFormatKey, "A {0} cannot be used as a {1}."),
        (CollectionSummaryFormatKey, "Count = {0}"),
        (WholeNumberNameKey, "whole number"),
        (NumberNameKey, "number"),
        (BooleanNameKey, "true or false value"),
        (CharacterNameKey, "single character"),
        (TextNameKey, "text"),
        (DateTimeNameKey, "date and time"),
        (DateNameKey, "date"),
        (TimeNameKey, "time"),
        (DurationNameKey, "duration"),
    ];

    /// <summary>Gets every resource key the grid reads text from, so that a translation can be checked at startup.</summary>
    /// <remarks>
    /// <para>
    /// A resource key is not a name the compiler checks. An entry declared under a key the library
    /// no longer reads fails in silence: it sits in the dictionary, nobody looks at it, and the
    /// string reverts to English somewhere quiet — a validation message, a category header,
    /// something only a screen reader hears. A key left out of a translation fails the same way.
    /// </para>
    /// <para>
    /// Walking this list where the application declares its strings turns both into a startup error:
    /// </para>
    /// <code>
    /// foreach (string key in PropertyGridText.ResourceKeys)
    /// {
    ///     if (!translated.ContainsKey(key))
    ///     {
    ///         throw new InvalidOperationException($"{key} has no translation.");
    ///     }
    /// }
    ///
    /// foreach (string key in translated.Keys)
    /// {
    ///     if (!PropertyGridText.ResourceKeys.Contains(key))
    ///     {
    ///         throw new InvalidOperationException($"{key} is not a key the grid reads.");
    ///     }
    /// }
    /// </code>
    /// <para>
    /// Only text is listed. The glyph keys are code points in a symbol font, and the brushes, the
    /// metrics and the styles are not language.
    /// </para>
    /// </remarks>
    public static IReadOnlyList<string> ResourceKeys { get; } = Array.AsReadOnly(Array.ConvertAll(Entries, entry => entry.Key));

    /// <summary>Names a type the way it should be said to a user rather than the way the runtime spells it.</summary>
    /// <param name="type">The type to name.</param>
    /// <returns>The name to put in a message.</returns>
    /// <remarks>
    /// A message saying "not a valid Int32" reads like a stack trace. One saying "not a valid whole
    /// number" is what the reader is actually being told. A type the grid has no name for is called
    /// what the runtime calls it, which is the best that can be done without knowing about it.
    /// </remarks>
    public static string NameOf(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        Type unwrapped = Nullable.GetUnderlyingType(type) ?? type;

        return Type.GetTypeCode(unwrapped) switch
        {
            TypeCode.SByte or TypeCode.Byte or TypeCode.Int16 or TypeCode.UInt16
                or TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64 => Read(WholeNumberNameKey),
            TypeCode.Single or TypeCode.Double or TypeCode.Decimal => Read(NumberNameKey),
            TypeCode.Boolean => Read(BooleanNameKey),
            TypeCode.Char => Read(CharacterNameKey),
            TypeCode.String => Read(TextNameKey),
            TypeCode.DateTime => Read(DateTimeNameKey),
            _ when unwrapped == typeof(DateTimeOffset) => Read(DateTimeNameKey),
            _ when unwrapped == typeof(DateOnly) => Read(DateNameKey),
            _ when unwrapped == typeof(TimeOnly) => Read(TimeNameKey),
            _ when unwrapped == typeof(TimeSpan) => Read(DurationNameKey),
            _ => unwrapped.Name,
        };
    }

    // Every key with the English behind it, for the test that holds this file and the resource
    // dictionary to the same list.
    internal static IReadOnlyDictionary<string, string> Defaults { get; } =
        Entries.ToDictionary(entry => entry.Key, entry => entry.Default, StringComparer.Ordinal);

    internal static string DefaultCategoryName => Read(DefaultCategoryNameKey);

    internal static string NotAValid(string text, Type type) =>
        string.Format(CultureInfo.CurrentCulture, Read(NotAValidFormatKey), text, NameOf(type));

    internal static string RequiredValue(Type type) =>
        string.Format(CultureInfo.CurrentCulture, Read(RequiredValueFormatKey), NameOf(type));

    internal static string CannotConvert(Type from, Type to) =>
        string.Format(CultureInfo.CurrentCulture, Read(CannotConvertFormatKey), NameOf(from), NameOf(to));

    internal static string CollectionSummary(int count) =>
        string.Format(CultureInfo.CurrentCulture, Read(CollectionSummaryFormatKey), count);

    // Read where it is used and never held on to: an application is free to declare its strings
    // after the first grid has already been built, and the next sentence has to come out in its
    // language rather than in the one that was in force when something was cached.
    private static string Read(string key) => PropertyGridThemeResources.Value(key, Defaults[key]);
}
