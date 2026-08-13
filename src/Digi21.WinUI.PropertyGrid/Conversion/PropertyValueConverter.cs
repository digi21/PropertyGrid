using System.ComponentModel;
using System.Globalization;

namespace Digi21.WinUI.PropertyGrid;

// Turns what the user typed into a value of the declared type, and back.
//
// Everything the grid writes goes through here, which is the point: a value converter attached to a
// binding would not know the declared type, could not say why a parse failed, and would have to be
// written once per (editor, type) pair. One function that takes the type does all of it, and can be
// tested without a XAML runtime.
internal static class PropertyValueConverter
{
    internal static string ToText(object? value, CultureInfo culture)
    {
        switch (value)
        {
            case null:
                return string.Empty;

            case string text:
                return text;

            // A formattable value with no format string still has to be given the culture, or a
            // Spanish user sees "1.5" where they type "1,5" and the round trip stops working.
            case IFormattable formattable:
                return formattable.ToString(null, culture);
        }

        string? described = value.ToString();

        // A type that never overrode ToString describes itself with its own full name, which in a
        // value cell is noise. The short name in brackets says the same thing and admits it.
        return described is null || string.Equals(described, value.GetType().FullName, StringComparison.Ordinal)
            ? "(" + value.GetType().Name + ")"
            : described;
    }

    internal static bool TryParse(
        string? text,
        Type targetType,
        CultureInfo culture,
        out object? value,
        out string? error)
    {
        Type underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
        bool acceptsNull = underlying != targetType || !targetType.IsValueType;

        if (underlying == typeof(string))
        {
            value = text ?? string.Empty;
            error = null;
            return true;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            value = null;
            error = acceptsNull ? null : Strings.RequiredValue(underlying);
            return acceptsNull;
        }

        string trimmed = text.Trim();

        if (underlying.IsEnum)
        {
            bool parsed = Enum.TryParse(underlying, trimmed, ignoreCase: true, out object? member);
            value = parsed ? member : null;
            error = parsed ? null : Strings.NotAValid(trimmed, underlying);
            return parsed;
        }

        if (TryParseKnown(trimmed, underlying, culture, out value))
        {
            error = null;
            return true;
        }

        if (TryParseWithTypeConverter(trimmed, underlying, culture, out value))
        {
            error = null;
            return true;
        }

        value = null;
        error = Strings.NotAValid(trimmed, underlying);
        return false;
    }

    // Coerces a value that is already typed - one coming from a combo box, a colour picker, a
    // programmatic write - into what the property will actually accept.
    internal static bool TryCoerce(
        object? value,
        Type targetType,
        CultureInfo culture,
        out object? coerced,
        out string? error)
    {
        Type underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (value is null)
        {
            coerced = null;
            bool acceptsNull = underlying != targetType || !targetType.IsValueType;
            error = acceptsNull ? null : Strings.RequiredValue(underlying);
            return acceptsNull;
        }

        if (underlying.IsInstanceOfType(value))
        {
            coerced = value;
            error = null;
            return true;
        }

        // A number box hands back a double for an int property, and a combo box bound to strings
        // hands back a string for an enum. Both are the same question the text path already answers.
        if (value is string text)
        {
            return TryParse(text, targetType, culture, out coerced, out error);
        }

        if (underlying.IsEnum && value is IConvertible)
        {
            try
            {
                coerced = Enum.ToObject(underlying, value);
                error = null;
                return true;
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidCastException or OverflowException)
            {
                coerced = null;
                error = Strings.CannotConvert(value.GetType(), underlying);
                return false;
            }
        }

        if (value is IConvertible && typeof(IConvertible).IsAssignableFrom(underlying))
        {
            try
            {
                coerced = Convert.ChangeType(value, underlying, culture);
                error = null;
                return true;
            }
            catch (Exception exception) when (exception is FormatException or InvalidCastException or OverflowException)
            {
                coerced = null;
                error = Strings.CannotConvert(value.GetType(), underlying);
                return false;
            }
        }

        coerced = null;
        error = Strings.CannotConvert(value.GetType(), underlying);
        return false;
    }

    private static bool TryParseKnown(string text, Type underlying, CultureInfo culture, out object? value)
    {
        // Group separators are accepted for whole numbers and refused for real ones, and the
        // asymmetry is deliberate. In a culture where the group separator is a dot, "1.5" typed into
        // an integer can only have meant fifteen, while in a real it almost certainly meant one and
        // a half - and .NET is lenient about where group separators fall, so allowing them would
        // silently turn that into fifteen instead of rejecting it.
        const NumberStyles Integers = NumberStyles.Integer | NumberStyles.AllowThousands;
        const NumberStyles Reals = NumberStyles.Float;

        switch (Type.GetTypeCode(underlying))
        {
            case TypeCode.Boolean:
                return Parsed(bool.TryParse(text, out bool parsedBool), parsedBool, out value);
            case TypeCode.Char:
                return Parsed(char.TryParse(text, out char parsedChar), parsedChar, out value);
            case TypeCode.SByte:
                return Parsed(sbyte.TryParse(text, Integers, culture, out sbyte parsedSByte), parsedSByte, out value);
            case TypeCode.Byte:
                return Parsed(byte.TryParse(text, Integers, culture, out byte parsedByte), parsedByte, out value);
            case TypeCode.Int16:
                return Parsed(short.TryParse(text, Integers, culture, out short parsedShort), parsedShort, out value);
            case TypeCode.UInt16:
                return Parsed(ushort.TryParse(text, Integers, culture, out ushort parsedUShort), parsedUShort, out value);
            case TypeCode.Int32:
                return Parsed(int.TryParse(text, Integers, culture, out int parsedInt), parsedInt, out value);
            case TypeCode.UInt32:
                return Parsed(uint.TryParse(text, Integers, culture, out uint parsedUInt), parsedUInt, out value);
            case TypeCode.Int64:
                return Parsed(long.TryParse(text, Integers, culture, out long parsedLong), parsedLong, out value);
            case TypeCode.UInt64:
                return Parsed(ulong.TryParse(text, Integers, culture, out ulong parsedULong), parsedULong, out value);
            case TypeCode.Single:
                return Parsed(float.TryParse(text, Reals, culture, out float parsedFloat), parsedFloat, out value);
            case TypeCode.Double:
                return Parsed(double.TryParse(text, Reals, culture, out double parsedDouble), parsedDouble, out value);
            case TypeCode.Decimal:
                return Parsed(decimal.TryParse(text, Reals, culture, out decimal parsedDecimal), parsedDecimal, out value);
            case TypeCode.DateTime:
                return Parsed(
                    DateTime.TryParse(text, culture, DateTimeStyles.None, out DateTime parsedDateTime),
                    parsedDateTime,
                    out value);
        }

        if (underlying == typeof(DateTimeOffset))
        {
            return Parsed(
                DateTimeOffset.TryParse(text, culture, DateTimeStyles.None, out DateTimeOffset parsed),
                parsed,
                out value);
        }

        if (underlying == typeof(DateOnly))
        {
            return Parsed(DateOnly.TryParse(text, culture, DateTimeStyles.None, out DateOnly parsed), parsed, out value);
        }

        if (underlying == typeof(TimeOnly))
        {
            return Parsed(TimeOnly.TryParse(text, culture, DateTimeStyles.None, out TimeOnly parsed), parsed, out value);
        }

        if (underlying == typeof(TimeSpan))
        {
            return Parsed(TimeSpan.TryParse(text, culture, out TimeSpan parsed), parsed, out value);
        }

        if (underlying == typeof(Guid))
        {
            return Parsed(Guid.TryParse(text, out Guid parsed), parsed, out value);
        }

        if (underlying == typeof(Version))
        {
            return Parsed(Version.TryParse(text, out Version? parsed), parsed, out value);
        }

        if (underlying == typeof(Uri))
        {
            return Parsed(Uri.TryCreate(text, UriKind.RelativeOrAbsolute, out Uri? parsed), parsed, out value);
        }

        // Neither of these touches the disk, so a path to something that does not exist yet is
        // accepted - which is exactly what a "save as" box needs. Nor do they judge the spelling:
        // .NET stopped rejecting odd characters when it stopped assuming Windows, and whether a path
        // is usable depends on the file system it lands on. The catch below is for the few things
        // that still throw, such as a path past the length limit.
        if (underlying == typeof(FileInfo))
        {
            return Parsed(TryCreatePath(text, path => new FileInfo(path), out FileInfo? file), file, out value);
        }

        if (underlying == typeof(DirectoryInfo))
        {
            return Parsed(TryCreatePath(text, path => new DirectoryInfo(path), out DirectoryInfo? folder), folder, out value);
        }

        value = null;
        return false;
    }

    private static bool TryParseWithTypeConverter(string text, Type underlying, CultureInfo culture, out object? value)
    {
        TypeConverter converter = TypeDescriptor.GetConverter(underlying);
        if (!converter.CanConvertFrom(typeof(string)))
        {
            value = null;
            return false;
        }

        try
        {
            value = converter.ConvertFromString(null, culture, text);
            return true;
        }
        catch (Exception exception) when (exception is not OutOfMemoryException and not StackOverflowException)
        {
            // Converters signal a bad string by throwing whatever they feel like, from FormatException
            // to a bare Exception, so there is nothing narrower to catch here.
            value = null;
            return false;
        }
    }

    private static bool TryCreatePath<T>(string text, Func<string, T> create, out T? created)
        where T : class
    {
        try
        {
            created = create(text);
            return true;
        }
        catch (Exception exception) when (exception is ArgumentException or PathTooLongException or NotSupportedException)
        {
            created = null;
            return false;
        }
    }

    private static bool Parsed<T>(bool success, T parsed, out object? value)
    {
        value = success ? parsed : null;
        return success;
    }
}
