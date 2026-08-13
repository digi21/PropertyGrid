namespace Digi21.WinUI.PropertyGrid;

/// <summary>One of the values a property accepts, and what to call it on screen.</summary>
/// <param name="Value">The value that ends up on the object.</param>
/// <param name="DisplayName">What the user chooses by.</param>
/// <param name="Description">A sentence explaining the choice, shown as a tooltip.</param>
/// <remarks>
/// <para>
/// The label and the value are separate on purpose: a coded domain stores a 1 and the person
/// choosing sees "Stop". This is the same shape as <see cref="EnumMemberRow"/>, which is the same
/// problem already solved for enumerations — the difference is that these are decided per property
/// and often at run time, from a database schema or a configuration file.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// new PropertyDescription
/// {
///     Name = "SignType",
///     PropertyType = typeof(int),
///     Accessor = accessor,
///     StandardValues =
///     [
///         new PropertyStandardValue(1, "Stop"),
///         new PropertyStandardValue(2, "No entry"),
///     ],
/// }
/// </code>
/// </example>
public sealed record PropertyStandardValue(object? Value, string DisplayName, string? Description = null)
{
    /// <inheritdoc />
    public override string ToString() => DisplayName;
}
