namespace Digi21.WinUI.PropertyGrid;

// What one registered editor says it is for. A plain value rather than the DependencyObject that
// holds it in markup, so the whole matching order is testable without a XAML runtime.
internal readonly record struct EditorCriteria(Type? TargetType, string? Key, bool MatchDerivedTypes);

// Decides which registered editor, if any, a property gets.
//
// The order is fixed and worth knowing, because a surprise here shows up as "why is this property
// using that control". Most specific first:
//
//   1. the name the property asked for by [PropertyEditor]
//   2. the type the value actually is, when the declared type is too vague to be useful
//   3. the declared type, exactly
//   4. the type inside a nullable, exactly
//   5. base classes, nearest first, for entries that opted into matching derived types
//   6. interfaces, most derived first, likewise
//
// Anything unmatched falls through to the built-in table.
internal static class PropertyEditorMatching
{
    internal const int NoMatch = -1;

    internal static int Resolve(
        IReadOnlyList<EditorCriteria> entries,
        Type declaredType,
        Type? runtimeType,
        string? explicitKey)
    {
        if (entries.Count == 0)
        {
            return NoMatch;
        }

        if (!string.IsNullOrEmpty(explicitKey))
        {
            int named = IndexOfKey(entries, explicitKey);
            if (named != NoMatch)
            {
                return named;
            }
        }

        // A property declared as object, as an interface or as an abstract class tells you almost
        // nothing about what to edit; what is in it right now tells you everything.
        if (runtimeType is not null && IsVague(declaredType))
        {
            int byRuntime = IndexOfExactType(entries, runtimeType);
            if (byRuntime != NoMatch)
            {
                return byRuntime;
            }
        }

        int exact = IndexOfExactType(entries, declaredType);
        if (exact != NoMatch)
        {
            return exact;
        }

        // Probing the nullable form first is what lets somebody register a distinct editor for
        // int? - one with a way to clear it - without disturbing plain int.
        Type underlying = KnownTypes.Unwrap(declaredType);
        if (underlying != declaredType)
        {
            int unwrapped = IndexOfExactType(entries, underlying);
            if (unwrapped != NoMatch)
            {
                return unwrapped;
            }
        }

        Type candidate = runtimeType is not null && IsVague(declaredType) ? runtimeType : underlying;

        for (Type? current = candidate.BaseType; current is not null; current = current.BaseType)
        {
            int inherited = IndexOfExactType(entries, current, derivedOnly: true);
            if (inherited != NoMatch)
            {
                return inherited;
            }
        }

        foreach (Type contract in MostDerivedFirst(candidate.GetInterfaces()))
        {
            int implemented = IndexOfExactType(entries, contract, derivedOnly: true);
            if (implemented != NoMatch)
            {
                return implemented;
            }
        }

        return NoMatch;
    }

    private static bool IsVague(Type type) =>
        type == typeof(object) || type.IsInterface || type.IsAbstract;

    private static int IndexOfKey(IReadOnlyList<EditorCriteria> entries, string key)
    {
        // Later registrations win, so an application can override what a library registered by
        // registering the same thing again.
        for (int index = entries.Count - 1; index >= 0; index--)
        {
            if (string.Equals(entries[index].Key, key, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return NoMatch;
    }

    private static int IndexOfExactType(IReadOnlyList<EditorCriteria> entries, Type type, bool derivedOnly = false)
    {
        for (int index = entries.Count - 1; index >= 0; index--)
        {
            EditorCriteria entry = entries[index];
            if (entry.TargetType == type && (!derivedOnly || entry.MatchDerivedTypes))
            {
                return index;
            }
        }

        return NoMatch;
    }

    // A type implements every interface its interfaces extend, and GetInterfaces returns them all
    // flattened in no useful order. Without sorting them, a type hitting two registered interfaces
    // could resolve differently between runs.
    private static Type[] MostDerivedFirst(Type[] interfaces)
    {
        Type[] sorted = [.. interfaces];

        Array.Sort(sorted, (left, right) =>
        {
            if (left == right)
            {
                return 0;
            }

            if (right.IsAssignableFrom(left))
            {
                return -1;
            }

            if (left.IsAssignableFrom(right))
            {
                return 1;
            }

            return string.CompareOrdinal(left.FullName, right.FullName);
        });

        return sorted;
    }
}
