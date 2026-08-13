namespace Digi21.WinUI.PropertyGrid;

/// <summary>
/// Everything a <see cref="PropertyGrid"/> knows about one property before it has an object to read
/// it from: its name, its type, how it should be labelled and grouped, and how to get at its value.
/// </summary>
/// <remarks>
/// <para>
/// Descriptions are immutable and cached per type, so building one is paid for once and every grid
/// showing that type shares the result. It is a record so that a description can be adjusted with a
/// <c>with</c> expression — which is how metadata overrides and the
/// <c>AutoGeneratingProperty</c> event change one field without rebuilding the rest.
/// </para>
/// <para>
/// Nothing here touches XAML. An <see cref="IPropertyDescriptionProvider"/> can build these from
/// something that is not a CLR type at all.
/// </para>
/// </remarks>
public sealed record PropertyDescription
{
    private readonly string? displayName;

    /// <summary>Gets the name of the property as it is declared in code.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the declared type of the property, which is what decides its editor.</summary>
    /// <remarks>
    /// The declared type, not the type of the current value: a property declared as
    /// <c>IShape</c> keeps offering the editor registered for <c>IShape</c> even while it happens to
    /// hold a <c>Circle</c>, because that is what the property will accept back.
    /// </remarks>
    public required Type PropertyType { get; init; }

    /// <summary>Gets the way to read and write the value of the property.</summary>
    public required PropertyAccessor Accessor { get; init; }

    /// <summary>Gets the type that declares the property.</summary>
    public Type? DeclaringType { get; init; }

    /// <summary>Gets the label shown for the property, which falls back to <see cref="Name"/>.</summary>
    public string DisplayName
    {
        get => displayName ?? Name;
        init => displayName = value;
    }

    /// <summary>Gets the sentence explaining the property, shown in the description pane.</summary>
    public string? HelpText { get; init; }

    /// <summary>Gets the category the property belongs to, or <see langword="null"/> for the default one.</summary>
    public string? CategoryName { get; init; }

    /// <summary>Gets the explicit position of the property, or <see cref="int.MaxValue"/> if it asked for none.</summary>
    public int Order { get; init; } = int.MaxValue;

    /// <summary>Gets a value indicating whether the property is shown at all.</summary>
    public bool IsBrowsable { get; init; } = true;

    /// <summary>Gets a value indicating whether the grid refuses to write the property.</summary>
    public bool IsReadOnly { get; init; }

    /// <summary>Gets a value indicating whether a value for the property is declared.</summary>
    public bool HasDefaultValue { get; init; }

    /// <summary>Gets the declared value of the property, used to tell a modified row from an untouched one.</summary>
    public object? DefaultValue { get; init; }

    /// <summary>
    /// Gets a value indicating whether the property keeps its own editor when several objects are
    /// shown at once, or drops out of the merged list.
    /// </summary>
    public bool IsMergable { get; init; } = true;

    /// <summary>Gets the name of the editor the property asked for, or <see langword="null"/> to resolve one from the type.</summary>
    public string? EditorKey { get; init; }

    /// <summary>
    /// Gets whether the property can be opened into child rows, or <see langword="null"/> to let the
    /// grid's expansion policy decide.
    /// </summary>
    public bool? IsExpandable { get; init; }

    /// <summary>Gets the attributes declared on the property, for validation and for editors to read.</summary>
    public IReadOnlyList<Attribute> Attributes { get; init; } = [];

    /// <summary>Gets a glyph to show before the name, from the symbol font.</summary>
    /// <remarks>
    /// A character such as <c>""</c>. Icons ship as a font rather than as images so that they
    /// follow the shell and the theme, and so that a description can carry one without the library
    /// having to know anything about assets.
    /// </remarks>
    public string? Glyph { get; init; }

    /// <summary>Gets the name of the brush to paint the property's name with, or <see langword="null"/> for the usual one.</summary>
    /// <remarks>
    /// A resource key rather than a brush, so that a description stays a plain object and the colour
    /// still follows the theme. Any key the application's resources hold will do.
    /// </remarks>
    public string? NameBrushKey { get; init; }

    /// <summary>Gets the name of the brush to paint the property's value with, or <see langword="null"/> for the usual one.</summary>
    public string? ValueBrushKey { get; init; }

    /// <summary>Gets the name of the brush to paint the whole row with, or <see langword="null"/> for the usual one.</summary>
    public string? RowBrushKey { get; init; }

    /// <summary>Gets a value indicating whether the editor takes the whole row, with no name beside it.</summary>
    /// <remarks>
    /// For a property whose editor is the point and whose name would only be in the way — a preview
    /// strip, a gradient, a bank of buttons. The splitter still sits where it always did, so the
    /// rows around it stay lined up.
    /// </remarks>
    public bool IsFullWidth { get; init; }

    /// <summary>Gets the values this particular property accepts, or <see langword="null"/> to ask its type.</summary>
    /// <remarks>
    /// <para>
    /// Per property, not per type, which is the difference that matters. A
    /// <see cref="System.ComponentModel.TypeConverter"/>
    /// can only say what every <c>int</c> in the program accepts; two coded fields of the same
    /// database table have different domains, and both are read at run time.
    /// </para>
    /// <para>
    /// Left <see langword="null"/>, the grid falls back to
    /// <see cref="System.ComponentModel.TypeConverter.GetStandardValues()"/> exactly as before.
    /// </para>
    /// </remarks>
    public IReadOnlyList<PropertyStandardValue>? StandardValues { get; init; }

    /// <summary>Finds the first attribute of a type declared on the property.</summary>
    /// <typeparam name="T">The type of attribute to look for.</typeparam>
    /// <returns>The attribute, or <see langword="null"/> if the property does not carry one.</returns>
    public T? GetAttribute<T>()
        where T : Attribute
    {
        foreach (Attribute attribute in Attributes)
        {
            if (attribute is T match)
            {
                return match;
            }
        }

        return null;
    }
}
