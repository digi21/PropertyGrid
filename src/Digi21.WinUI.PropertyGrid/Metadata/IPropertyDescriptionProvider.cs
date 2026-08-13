namespace Digi21.WinUI.PropertyGrid;

/// <summary>Decides which properties a <see cref="PropertyGrid"/> shows for a type, and what it knows about them.</summary>
/// <remarks>
/// Implement this to drive a grid from something reflection cannot see: a dictionary, an
/// <c>ExpandoObject</c>, a schema loaded at run time. For ordinary CLR objects
/// <see cref="ReflectionPropertyDescriptionProvider"/> already does the job, and
/// <see cref="PropertyGridMetadata"/> adjusts what it produces without replacing it.
/// </remarks>
public interface IPropertyDescriptionProvider
{
    /// <summary>Lists the properties to show for a type.</summary>
    /// <param name="type">The type being shown.</param>
    /// <returns>
    /// The properties, in the order they were declared. The grid re-orders and groups them according
    /// to its <see cref="PropertySort"/>, so the order here only decides ties.
    /// </returns>
    IReadOnlyList<PropertyDescription> GetProperties(Type type);
}
