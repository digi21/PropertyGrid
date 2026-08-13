using System.Collections;

namespace Digi21.WinUI.PropertyGrid;

// Answers the one question the grid keeps asking about a type: is this a value the user edits in a
// single control, or a thing made of other things that should open into child rows?
internal static class KnownTypes
{
    private static readonly HashSet<Type> Simple =
    [
        typeof(string),
        typeof(decimal),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(DateOnly),
        typeof(TimeOnly),
        typeof(TimeSpan),
        typeof(Guid),
        typeof(Uri),
        typeof(Version),
    ];

    // Structs that are conceptually a handful of numbers. They are simple in the sense that they have
    // a text form worth typing, but they are also worth opening: "Left, Top, Right, Bottom" is easier
    // to nudge one field at a time than to retype as a whole.
    private static readonly HashSet<string> CompositeStructNames =
    [
        "Windows.Foundation.Point",
        "Windows.Foundation.Size",
        "Windows.Foundation.Rect",
        "Microsoft.UI.Xaml.Thickness",
        "Microsoft.UI.Xaml.CornerRadius",
        "Microsoft.UI.Xaml.GridLength",
        "Windows.UI.Color",
    ];

    internal static Type Unwrap(Type type) => Nullable.GetUnderlyingType(type) ?? type;

    internal static bool IsSimple(Type type)
    {
        Type unwrapped = Unwrap(type);

        return unwrapped.IsPrimitive
            || unwrapped.IsEnum
            || Simple.Contains(unwrapped)
            || CompositeStructNames.Contains(unwrapped.FullName ?? string.Empty);
    }

    internal static bool IsComposite(Type type) =>
        CompositeStructNames.Contains(Unwrap(type).FullName ?? string.Empty);

    // A string is an IEnumerable and a dictionary is a collection, and neither should be listed
    // element by element: the first is a value, the second needs an editor this version does not
    // ship. Everything else that enumerates is a list the user can look inside.
    internal static bool IsCollection(Type type)
    {
        Type unwrapped = Unwrap(type);

        return unwrapped != typeof(string)
            && !typeof(IDictionary).IsAssignableFrom(unwrapped)
            && typeof(IEnumerable).IsAssignableFrom(unwrapped);
    }
}
