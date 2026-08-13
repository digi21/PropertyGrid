using System.Collections.Concurrent;
using System.Reflection;

namespace Digi21.WinUI.PropertyGrid;

/// <summary>Finds the properties of a type by reflection, and describes them from their attributes.</summary>
/// <remarks>
/// <para>
/// Results are cached per type, because the expensive half of this is reading attributes rather than
/// listing properties, and a grid asks for the same type every time an object of it is selected. The
/// cache is dropped whenever the <see cref="PropertyGridMetadata"/> store it was built against
/// changes.
/// </para>
/// <para>
/// What is deliberately not cached is the values: <see cref="PropertyDescription.Accessor"/> reads
/// through <see cref="PropertyInfo"/> on every call. Compiling accessor delegates would save around
/// a hundred nanoseconds per read, on a control that reads a few dozen values per interaction, at
/// the cost of trimming and ahead-of-time friendliness.
/// </para>
/// </remarks>
public sealed class ReflectionPropertyDescriptionProvider : IPropertyDescriptionProvider
{
    private readonly ConcurrentDictionary<Type, CacheEntry> cache = [];
    private readonly PropertyGridMetadata metadata;

    /// <summary>Initializes a new instance of the <see cref="ReflectionPropertyDescriptionProvider"/> class.</summary>
    /// <param name="metadata">The store of overrides to apply on top of the attributes.</param>
    public ReflectionPropertyDescriptionProvider(PropertyGridMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);

        this.metadata = metadata;
    }

    /// <summary>Gets the provider used by grids that were not given one, reading <see cref="PropertyGridMetadata.Default"/>.</summary>
    public static ReflectionPropertyDescriptionProvider Default { get; } = new(PropertyGridMetadata.Default);

    /// <inheritdoc />
    public IReadOnlyList<PropertyDescription> GetProperties(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        int currentVersion = metadata.Version;

        if (cache.TryGetValue(type, out CacheEntry entry) && entry.MetadataVersion == currentVersion)
        {
            return entry.Properties;
        }

        PropertyDescription[] properties = Build(type);
        cache[type] = new CacheEntry(currentVersion, properties);
        return properties;
    }

    /// <summary>Throws away what the provider remembers about a type, or about every type.</summary>
    /// <param name="type">The type to forget, or <see langword="null"/> to forget all of them.</param>
    /// <remarks>
    /// Changing the metadata store already invalidates the cache on its own. This is for the cases
    /// nothing can observe: a type reloaded by hot reload, attributes rewritten at run time.
    /// </remarks>
    public void Invalidate(Type? type = null)
    {
        if (type is null)
        {
            cache.Clear();
        }
        else
        {
            cache.TryRemove(type, out _);
        }
    }

    private PropertyDescription[] Build(Type type)
    {
        List<PropertyDescription> descriptions = [];

        foreach (PropertyInfo property in Candidates(type))
        {
            ReflectionPropertyAccessor accessor = new(property);
            if (!accessor.CanRead)
            {
                continue;
            }

            PropertyDescription description = AttributeReader.Describe(property, accessor);

            foreach (PropertyMetadataOverride adjustment in metadata.Find(type, property.Name))
            {
                description = adjustment.ApplyTo(description);
            }

            if (description.IsBrowsable)
            {
                descriptions.Add(description);
            }
        }

        return [.. descriptions];
    }

    // Public instance properties, most-derived declaration of each name, in declaration order:
    // the properties a base type declares first, then the ones the type adds itself.
    private static IEnumerable<PropertyInfo> Candidates(Type type)
    {
        Dictionary<string, (PropertyInfo Property, int Depth)> byName = [];

        foreach (PropertyInfo property in Reachable(type))
        {
            if (property.GetIndexParameters().Length > 0 || property.GetMethod is not { IsPublic: true })
            {
                continue;
            }

            int depth = DepthOf(property.DeclaringType);

            // A `new` property is returned alongside the one it hides, and only the one the compiler
            // would bind to should be shown - otherwise the grid lists the same name twice.
            if (!byName.TryGetValue(property.Name, out (PropertyInfo Property, int Depth) existing) || depth > existing.Depth)
            {
                byName[property.Name] = (property, depth);
            }
        }

        return byName.Values
            .OrderBy(candidate => candidate.Depth)
            .ThenBy(candidate => candidate.Property.MetadataToken)
            .Select(candidate => candidate.Property);
    }

    private static IEnumerable<PropertyInfo> Reachable(Type type)
    {
        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            yield return property;
        }

        // An interface does not inherit the members of the interfaces it extends the way a class
        // does, so GetProperties stops at the one interface. A grid handed a variable typed as an
        // interface would otherwise show a fraction of what that variable can do.
        if (!type.IsInterface)
        {
            yield break;
        }

        foreach (Type contract in type.GetInterfaces())
        {
            foreach (PropertyInfo property in contract.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                yield return property;
            }
        }
    }

    // How far the declaring type is from the root of the hierarchy, so that base-declared properties
    // sort before the ones a subclass adds, which is the order they appear in when reading the code.
    private static int DepthOf(Type? declaringType)
    {
        int depth = 0;
        for (Type? current = declaringType; current is not null; current = current.BaseType)
        {
            depth++;
        }

        return depth;
    }

    private readonly record struct CacheEntry(int MetadataVersion, PropertyDescription[] Properties);
}
