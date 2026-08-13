using System.Globalization;

namespace Digi21.WinUI.PropertyGrid;

// Every sentence the grid can show the user, in one place. The library ships no resource files yet,
// so this is also the single edit an application making the grid speak another language has to
// intercept, and the single file to turn into a .resx when it does.
internal static class Strings
{
    internal const string DefaultCategoryName = "Misc";

    internal const string MultipleValues = "(multiple values)";

    internal const string NullValue = "(none)";

    internal static string NotAValid(string text, Type type) =>
        string.Format(CultureInfo.CurrentCulture, "'{0}' is not a valid {1}.", text, FriendlyNameOf(type));

    internal static string RequiredValue(Type type) =>
        string.Format(CultureInfo.CurrentCulture, "A {0} is required.", FriendlyNameOf(type));

    internal static string CannotConvert(Type from, Type to) =>
        string.Format(
            CultureInfo.CurrentCulture,
            "A {0} cannot be used as a {1}.",
            FriendlyNameOf(from),
            FriendlyNameOf(to));

    internal static string CollectionSummary(int count) =>
        string.Format(CultureInfo.CurrentCulture, "Count = {0}", count);

    // The C# keyword rather than the CLR name: a message saying "not a valid Int32" reads like a
    // stack trace, and one saying "not a valid whole number" is what the user is actually being told.
    internal static string FriendlyNameOf(Type type)
    {
        Type unwrapped = Nullable.GetUnderlyingType(type) ?? type;

        return unwrapped switch
        {
            _ when unwrapped == typeof(sbyte) => "whole number",
            _ when unwrapped == typeof(byte) => "whole number",
            _ when unwrapped == typeof(short) => "whole number",
            _ when unwrapped == typeof(ushort) => "whole number",
            _ when unwrapped == typeof(int) => "whole number",
            _ when unwrapped == typeof(uint) => "whole number",
            _ when unwrapped == typeof(long) => "whole number",
            _ when unwrapped == typeof(ulong) => "whole number",
            _ when unwrapped == typeof(float) => "number",
            _ when unwrapped == typeof(double) => "number",
            _ when unwrapped == typeof(decimal) => "number",
            _ when unwrapped == typeof(bool) => "true or false value",
            _ when unwrapped == typeof(char) => "single character",
            _ when unwrapped == typeof(string) => "text",
            _ when unwrapped == typeof(DateTime) => "date and time",
            _ when unwrapped == typeof(DateTimeOffset) => "date and time",
            _ when unwrapped == typeof(DateOnly) => "date",
            _ when unwrapped == typeof(TimeOnly) => "time",
            _ when unwrapped == typeof(TimeSpan) => "duration",
            _ => unwrapped.Name,
        };
    }
}
