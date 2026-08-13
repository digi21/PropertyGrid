using System.Globalization;

namespace Digi21.WinUI.PropertyGrid;

/// <summary>Every sentence the grid can put in front of a user, in one place, so it can be translated.</summary>
/// <remarks>
/// <para>
/// These are the ones the grid builds at run time — the reasons an edit was rejected, mostly. The
/// text the templates show instead comes from resource keys, so it is replaced the same way a brush
/// is: <c>PropertyGridSearchPlaceholderText</c>, <c>PropertyGridOkButtonText</c>,
/// <c>PropertyGridCancelButtonText</c>, <c>PropertyGridBrowseToolTipText</c> and
/// <c>PropertyGridEditToolTipText</c>.
/// </para>
/// <para>
/// Set these once, early, from wherever the application keeps its translations. They are static
/// because a validation message has no control to ask, and shared because an application showing
/// two grids in two languages at once is not a thing.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// PropertyGridStrings.DefaultCategoryName = Loc("Misc");
/// PropertyGridStrings.NotAValidFormat = Loc("'{0}' no es un {1} válido.");
/// PropertyGridStrings.WholeNumberName = Loc("número entero");
/// </code>
/// </example>
public static class PropertyGridStrings
{
    /// <summary>Gets or sets the category properties land in when they do not name one.</summary>
    public static string DefaultCategoryName { get; set; } = "Misc";

    // Deliberately absent: a string for "several objects disagree" and one for "not set". Both were
    // declared before anything used them, and public API that never reaches the screen is worse than
    // none - it is a translator's afternoon spent on nothing. The first comes back with multiple
    // selection; the second when a summary cell has somewhere to put it that is not the text a user
    // edits.

    /// <summary>Gets or sets the reason given when text cannot be read as the property's type. Takes the text and the type name.</summary>
    public static string NotAValidFormat { get; set; } = "'{0}' is not a valid {1}.";

    /// <summary>Gets or sets the reason given when a property that cannot be empty was emptied. Takes the type name.</summary>
    public static string RequiredValueFormat { get; set; } = "A {0} is required.";

    /// <summary>Gets or sets the reason given when a value is of the wrong type entirely. Takes both type names.</summary>
    public static string CannotConvertFormat { get; set; } = "A {0} cannot be used as a {1}.";

    /// <summary>Gets or sets how a list is summarised in its row. Takes how many things are in it.</summary>
    public static string CollectionSummaryFormat { get; set; } = "Count = {0}";

    /// <summary>Gets or sets what a whole number is called when a value is rejected.</summary>
    public static string WholeNumberName { get; set; } = "whole number";

    /// <summary>Gets or sets what a real number is called when a value is rejected.</summary>
    public static string NumberName { get; set; } = "number";

    /// <summary>Gets or sets what a true-or-false value is called when a value is rejected.</summary>
    public static string BooleanName { get; set; } = "true or false value";

    /// <summary>Gets or sets what a single character is called when a value is rejected.</summary>
    public static string CharacterName { get; set; } = "single character";

    /// <summary>Gets or sets what text is called when a value is rejected.</summary>
    public static string TextName { get; set; } = "text";

    /// <summary>Gets or sets what a date and time is called when a value is rejected.</summary>
    public static string DateTimeName { get; set; } = "date and time";

    /// <summary>Gets or sets what a date is called when a value is rejected.</summary>
    public static string DateName { get; set; } = "date";

    /// <summary>Gets or sets what a time is called when a value is rejected.</summary>
    public static string TimeName { get; set; } = "time";

    /// <summary>Gets or sets what a duration is called when a value is rejected.</summary>
    public static string DurationName { get; set; } = "duration";

    /// <summary>Names a type the way it should be said to a user rather than the way the runtime spells it.</summary>
    /// <param name="type">The type to name.</param>
    /// <returns>The name to put in a message.</returns>
    /// <remarks>
    /// A message saying "not a valid Int32" reads like a stack trace. One saying "not a valid whole
    /// number" is what the reader is actually being told.
    /// </remarks>
    public static string NameOf(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        Type unwrapped = Nullable.GetUnderlyingType(type) ?? type;

        return Type.GetTypeCode(unwrapped) switch
        {
            TypeCode.SByte or TypeCode.Byte or TypeCode.Int16 or TypeCode.UInt16
                or TypeCode.Int32 or TypeCode.UInt32 or TypeCode.Int64 or TypeCode.UInt64 => WholeNumberName,
            TypeCode.Single or TypeCode.Double or TypeCode.Decimal => NumberName,
            TypeCode.Boolean => BooleanName,
            TypeCode.Char => CharacterName,
            TypeCode.String => TextName,
            TypeCode.DateTime => DateTimeName,
            _ when unwrapped == typeof(DateTimeOffset) => DateTimeName,
            _ when unwrapped == typeof(DateOnly) => DateName,
            _ when unwrapped == typeof(TimeOnly) => TimeName,
            _ when unwrapped == typeof(TimeSpan) => DurationName,
            _ => unwrapped.Name,
        };
    }

    internal static string NotAValid(string text, Type type) =>
        string.Format(CultureInfo.CurrentCulture, NotAValidFormat, text, NameOf(type));

    internal static string RequiredValue(Type type) =>
        string.Format(CultureInfo.CurrentCulture, RequiredValueFormat, NameOf(type));

    internal static string CannotConvert(Type from, Type to) =>
        string.Format(CultureInfo.CurrentCulture, CannotConvertFormat, NameOf(from), NameOf(to));

    internal static string CollectionSummary(int count) =>
        string.Format(CultureInfo.CurrentCulture, CollectionSummaryFormat, count);
}
