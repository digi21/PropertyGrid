namespace Digi21.WinUI.PropertyGrid;

/// <summary>The names of the editors the library ships with.</summary>
/// <remarks>
/// <para>
/// Each of these is the key of a <c>DataTemplate</c> the grid merges into the application's
/// resources, so declaring a template under the same key in <c>App.xaml</c> replaces that editor
/// everywhere — the same way redeclaring a brush recolours the grid. To replace an editor for one
/// type rather than for a whole category of them, register a
/// <see cref="PropertyEditorTemplate"/> instead.
/// </para>
/// <para>
/// A property can also ask for one of these by name with
/// <c>[PropertyEditor(PropertyEditorKeys.MultilineString)]</c>.
/// </para>
/// </remarks>
public static class PropertyEditorKeys
{
    /// <summary>A single-line text box. The fallback for anything that round-trips through text.</summary>
    public const string String = "PropertyGridStringEditorTemplate";

    /// <summary>A text box that accepts line breaks.</summary>
    public const string MultilineString = "PropertyGridMultilineStringEditorTemplate";

    /// <summary>A box that hides what is typed into it.</summary>
    public const string Password = "PropertyGridPasswordEditorTemplate";

    /// <summary>
    /// A text box with a browse button beside it. The button raises
    /// <see cref="PropertyGrid.BrowseRequested"/>; the grid never opens a dialog itself.
    /// </summary>
    public const string Path = "PropertyGridPathEditorTemplate";

    /// <summary>
    /// A summary of the value with a button beside it, for anything that needs more room than a row.
    /// The button raises <see cref="PropertyGrid.EditRequested"/>; the grid never opens a dialog
    /// itself, and hides the button when nothing is listening.
    /// </summary>
    public const string Dialog = "PropertyGridDialogEditorTemplate";

    /// <summary>A number box, for the numeric types a <see cref="double"/> can hold exactly.</summary>
    public const string Number = "PropertyGridNumberEditorTemplate";

    /// <summary>A text box, for the numeric types a <see cref="double"/> would round.</summary>
    public const string LargeNumber = "PropertyGridLargeNumberEditorTemplate";

    /// <summary>A check box.</summary>
    public const string Boolean = "PropertyGridBooleanEditorTemplate";

    /// <summary>A check box with a third, indeterminate state.</summary>
    public const string NullableBoolean = "PropertyGridNullableBooleanEditorTemplate";

    /// <summary>A drop-down of the members of an enumeration.</summary>
    public const string Enum = "PropertyGridEnumEditorTemplate";

    /// <summary>A drop-down of tick boxes, one per flag.</summary>
    public const string FlagsEnum = "PropertyGridFlagsEnumEditorTemplate";

    /// <summary>A calendar and a clock.</summary>
    public const string DateTime = "PropertyGridDateTimeEditorTemplate";

    /// <summary>A calendar.</summary>
    public const string Date = "PropertyGridDateEditorTemplate";

    /// <summary>A clock.</summary>
    public const string Time = "PropertyGridTimeEditorTemplate";

    /// <summary>A text box taking a duration, which a clock cannot express past a day.</summary>
    public const string TimeSpan = "PropertyGridTimeSpanEditorTemplate";

    /// <summary>A swatch opening a colour picker.</summary>
    public const string Color = "PropertyGridColorEditorTemplate";

    /// <summary>A swatch opening a colour picker, editing the colour of a solid brush.</summary>
    public const string Brush = "PropertyGridBrushEditorTemplate";

    /// <summary>A text box for the structs that are a handful of numbers, such as a thickness.</summary>
    public const string Struct = "PropertyGridStructEditorTemplate";

    /// <summary>A drop-down of the values a type converter says the property accepts.</summary>
    public const string StandardValues = "PropertyGridStandardValuesEditorTemplate";

    /// <summary>A summary of a list, with a way to look inside it.</summary>
    public const string Collection = "PropertyGridCollectionEditorTemplate";

    /// <summary>A summary of an object, which the name column offers to open.</summary>
    public const string Complex = "PropertyGridComplexEditorTemplate";

    /// <summary>Selectable text. What anything the grid cannot edit falls back to.</summary>
    public const string ReadOnly = "PropertyGridReadOnlyEditorTemplate";

    /// <summary>
    /// A list of the installed font families. The library reserves the name but ships no template
    /// for it: enumerating what is installed needs DirectWrite interop the package will not take a
    /// dependency on. Declare a template under this key to supply your own.
    /// </summary>
    public const string FontFamily = "PropertyGridFontFamilyEditorTemplate";
}
