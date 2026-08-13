using System.Linq.Expressions;
using System.Reflection;

namespace Digi21.WinUI.PropertyGrid;

/// <summary>
/// Describes properties of types you cannot annotate — framework types, generated code, anything
/// coming from another assembly — so the grid presents them as well as it presents your own.
/// </summary>
/// <remarks>
/// <para>
/// Overrides are applied on top of whatever the attributes said, so a store only has to mention the
/// fields it disagrees with. Lookup walks the base types of the object being shown, most derived
/// first, which lets a rule written for a base type cover every subclass while a rule written for
/// one subclass still wins for it.
/// </para>
/// <para>
/// <see cref="Default"/> is shared by every grid in the application; a grid can be handed its own
/// store instead when the same type has to look different in two places.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// PropertyGridMetadata.Default
///     .For&lt;Rect&gt;()
///         .Property(rectangle => rectangle.X, property => property.DisplayName("Left").Category("Position").Order(0))
///         .Property(rectangle => rectangle.Width, property => property.ReadOnly())
///         .Ignore(rectangle => rectangle.Height);
/// </code>
/// </example>
public sealed class PropertyGridMetadata
{
    private readonly object gate = new();
    private readonly Dictionary<Type, Dictionary<string, PropertyMetadataOverride>> overridesByType = [];
    private int version;

    /// <summary>Gets the store every grid uses unless it is given one of its own.</summary>
    public static PropertyGridMetadata Default { get; } = new();

    /// <summary>
    /// Gets a number that changes whenever the store does, so that caches built from it know to
    /// throw their contents away.
    /// </summary>
    public int Version
    {
        get
        {
            lock (gate)
            {
                return version;
            }
        }
    }

    /// <summary>Starts describing the properties of a type.</summary>
    /// <typeparam name="T">The type to describe.</typeparam>
    /// <returns>A builder for the type.</returns>
    public TypeMetadataBuilder<T> For<T>() => new(this);

    /// <summary>Starts describing the properties of a type.</summary>
    /// <param name="type">The type to describe.</param>
    /// <returns>A builder for the type.</returns>
    public TypeMetadataBuilder For(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        return new TypeMetadataBuilder(this, type);
    }

    /// <summary>Forgets every override in the store.</summary>
    public void Clear()
    {
        lock (gate)
        {
            overridesByType.Clear();
            version++;
        }
    }

    // Collects the overrides that apply to one property of one type, least specific first, so the
    // caller can apply them in order and let the most derived rule win.
    internal List<PropertyMetadataOverride> Find(Type inspectedType, string propertyName)
    {
        List<PropertyMetadataOverride> matches = [];

        lock (gate)
        {
            if (overridesByType.Count == 0)
            {
                return matches;
            }

            // Walk to the root first and prepend, so the list ends up base-first: applying it in
            // order leaves the most derived rule on top without a second pass.
            for (Type? current = inspectedType; current is not null; current = current.BaseType)
            {
                if (overridesByType.TryGetValue(current, out Dictionary<string, PropertyMetadataOverride>? forType)
                    && forType.TryGetValue(propertyName, out PropertyMetadataOverride? match))
                {
                    matches.Insert(0, match);
                }
            }

            foreach (Type contract in inspectedType.GetInterfaces())
            {
                if (overridesByType.TryGetValue(contract, out Dictionary<string, PropertyMetadataOverride>? forInterface)
                    && forInterface.TryGetValue(propertyName, out PropertyMetadataOverride? match))
                {
                    matches.Insert(0, match);
                }
            }
        }

        return matches;
    }

    internal PropertyMetadataOverride GetOrAdd(Type type, string propertyName)
    {
        lock (gate)
        {
            if (!overridesByType.TryGetValue(type, out Dictionary<string, PropertyMetadataOverride>? forType))
            {
                forType = [];
                overridesByType[type] = forType;
            }

            if (!forType.TryGetValue(propertyName, out PropertyMetadataOverride? existing))
            {
                existing = new PropertyMetadataOverride();
                forType[propertyName] = existing;
            }

            version++;
            return existing;
        }
    }

    internal void Touch()
    {
        lock (gate)
        {
            version++;
        }
    }

    // Pulls the property name out of `x => x.Foo`, tolerating the boxing conversion the compiler
    // inserts when the lambda is typed as returning object.
    internal static string NameOf(LambdaExpression expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        Expression body = expression.Body is UnaryExpression { NodeType: ExpressionType.Convert } conversion
            ? conversion.Operand
            : expression.Body;

        return body is MemberExpression { Member: PropertyInfo property }
            ? property.Name
            : throw new ArgumentException("The expression must select a property, as in x => x.Name.", nameof(expression));
    }
}
