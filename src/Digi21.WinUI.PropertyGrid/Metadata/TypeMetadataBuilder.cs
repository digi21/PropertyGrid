using System.Linq.Expressions;

namespace Digi21.WinUI.PropertyGrid;

/// <summary>Describes the properties of a type in a <see cref="PropertyGridMetadata"/> store.</summary>
public class TypeMetadataBuilder
{
    private readonly PropertyGridMetadata metadata;
    private readonly Type type;

    internal TypeMetadataBuilder(PropertyGridMetadata metadata, Type type)
    {
        this.metadata = metadata;
        this.type = type;
    }

    /// <summary>Describes one property by name.</summary>
    /// <param name="propertyName">The name of the property, as it is declared in code.</param>
    /// <param name="configure">Sets the fields to override.</param>
    /// <returns>This builder.</returns>
    public TypeMetadataBuilder Property(string propertyName, Action<PropertyMetadataBuilder> configure)
    {
        ArgumentException.ThrowIfNullOrEmpty(propertyName);
        ArgumentNullException.ThrowIfNull(configure);

        configure(new PropertyMetadataBuilder(metadata, metadata.GetOrAdd(type, propertyName)));
        return this;
    }

    /// <summary>Hides one property by name.</summary>
    /// <param name="propertyName">The name of the property, as it is declared in code.</param>
    /// <returns>This builder.</returns>
    public TypeMetadataBuilder Ignore(string propertyName) =>
        Property(propertyName, property => property.Browsable(false));
}

/// <summary>Describes the properties of a type in a <see cref="PropertyGridMetadata"/> store.</summary>
/// <typeparam name="T">The type being described.</typeparam>
/// <remarks>
/// The typed builder exists so properties can be selected with <c>x =&gt; x.Name</c> instead of a
/// string, which a rename refactoring follows and a typo does not survive.
/// </remarks>
public sealed class TypeMetadataBuilder<T> : TypeMetadataBuilder
{
    internal TypeMetadataBuilder(PropertyGridMetadata metadata)
        : base(metadata, typeof(T))
    {
    }

    /// <summary>Describes one property.</summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="selector">Selects the property, as in <c>x =&gt; x.Name</c>.</param>
    /// <param name="configure">Sets the fields to override.</param>
    /// <returns>This builder.</returns>
    public TypeMetadataBuilder<T> Property<TProperty>(
        Expression<Func<T, TProperty>> selector,
        Action<PropertyMetadataBuilder> configure)
    {
        Property(PropertyGridMetadata.NameOf(selector), configure);
        return this;
    }

    /// <summary>Hides one property.</summary>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    /// <param name="selector">Selects the property, as in <c>x =&gt; x.Name</c>.</param>
    /// <returns>This builder.</returns>
    public TypeMetadataBuilder<T> Ignore<TProperty>(Expression<Func<T, TProperty>> selector)
    {
        Ignore(PropertyGridMetadata.NameOf(selector));
        return this;
    }
}
