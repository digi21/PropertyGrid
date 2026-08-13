using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Digi21.WinUI.PropertyGrid;

// Picks the editor a property gets when nobody registered one for it.
//
// A pure function of the description and the type of the value it currently holds, so the whole
// table is testable by name without a single DataTemplate in sight.
internal static class BuiltInEditors
{
    internal static string KeyFor(PropertyDescription description, Type? runtimeType)
    {
        Type declared = description.PropertyType;
        Type underlying = KnownTypes.Unwrap(declared);
        bool nullable = underlying != declared;

        if (underlying.IsEnum)
        {
            return EnumInfo.IsFlags(underlying) ? PropertyEditorKeys.FlagsEnum : PropertyEditorKeys.Enum;
        }

        if (underlying == typeof(string))
        {
            return TextKeyFor(description);
        }

        switch (Type.GetTypeCode(underlying))
        {
            case TypeCode.Boolean:
                return nullable ? PropertyEditorKeys.NullableBoolean : PropertyEditorKeys.Boolean;
            case TypeCode.Char:
                return PropertyEditorKeys.String;
            case TypeCode.SByte:
            case TypeCode.Byte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Single:
            case TypeCode.Double:
                return PropertyEditorKeys.Number;

            // A number box works in doubles, so these would silently lose their low bits past 2^53.
            case TypeCode.Int64:
            case TypeCode.UInt64:
            case TypeCode.Decimal:
                return PropertyEditorKeys.LargeNumber;
            case TypeCode.DateTime:
                return PropertyEditorKeys.DateTime;
        }

        if (underlying == typeof(DateTimeOffset))
        {
            return PropertyEditorKeys.DateTime;
        }

        if (underlying == typeof(DateOnly))
        {
            return PropertyEditorKeys.Date;
        }

        if (underlying == typeof(TimeOnly))
        {
            return PropertyEditorKeys.Time;
        }

        if (underlying == typeof(TimeSpan))
        {
            return PropertyEditorKeys.TimeSpan;
        }

        if (underlying == typeof(Guid) || underlying == typeof(Uri) || underlying == typeof(Version))
        {
            return PropertyEditorKeys.String;
        }

        if (underlying.FullName == "Windows.UI.Color")
        {
            return PropertyEditorKeys.Color;
        }

        if (IsBrush(underlying))
        {
            return PropertyEditorKeys.Brush;
        }

        if (KnownTypes.IsComposite(underlying))
        {
            return PropertyEditorKeys.Struct;
        }

        if (KnownTypes.IsCollection(underlying))
        {
            return PropertyEditorKeys.Collection;
        }

        TypeConverter converter = TypeDescriptor.GetConverter(underlying);

        if (converter.GetStandardValuesSupported() && converter.GetStandardValuesExclusive())
        {
            return PropertyEditorKeys.StandardValues;
        }

        // The declared type may be an interface or a base class holding something more specific, and
        // what the user can actually explore is whatever is in there right now.
        Type inspected = runtimeType ?? underlying;

        if (!inspected.IsPrimitive && inspected != typeof(object) && HasPropertiesWorthShowing(inspected))
        {
            return PropertyEditorKeys.Complex;
        }

        if (converter.CanConvertFrom(typeof(string)) && converter.CanConvertTo(typeof(string)))
        {
            return PropertyEditorKeys.String;
        }

        return PropertyEditorKeys.ReadOnly;
    }

    private static string TextKeyFor(PropertyDescription description)
    {
        if (description.GetAttribute<PasswordPropertyTextAttribute>() is { Password: true })
        {
            return PropertyEditorKeys.Password;
        }

        return description.GetAttribute<DataTypeAttribute>()?.DataType switch
        {
            System.ComponentModel.DataAnnotations.DataType.Password => PropertyEditorKeys.Password,
            System.ComponentModel.DataAnnotations.DataType.MultilineText => PropertyEditorKeys.MultilineString,
            _ => PropertyEditorKeys.String,
        };
    }

    // Named rather than referenced so that deciding an editor never pulls a XAML type into a model
    // that has to run where no XAML runtime exists.
    private static bool IsBrush(Type type)
    {
        for (Type? current = type; current is not null; current = current.BaseType)
        {
            if (current.FullName == "Microsoft.UI.Xaml.Media.Brush")
            {
                return true;
            }
        }

        return false;
    }

    private static bool HasPropertiesWorthShowing(Type type)
    {
        foreach (PropertyDescription property in ReflectionPropertyDescriptionProvider.Default.GetProperties(type))
        {
            _ = property;
            return true;
        }

        return false;
    }
}
