# Metadata

What the grid shows for a property — its label, the sentence under it, the category it lands in,
whether it can be edited at all — comes from the attributes on the property. For types you cannot
annotate, there are three ways to say the same things from outside.

## Attributes the grid reads

All of these are in the framework; the library adds no dependency to use them.

### `System.ComponentModel`

| Attribute | Effect |
|---|---|
| `[DisplayName("Layer name")]` | the label |
| `[Description("…")]` | the sentence in the description pane |
| `[Category("Appearance")]` | which group the property joins |
| `[Browsable(false)]` | leaves the property out |
| `[EditorBrowsable(Never)]` | likewise: hidden from the person writing the code, hidden from the person running it |
| `[ReadOnly(true)]` | shows the value but refuses to write it |
| `[DefaultValue(80)]` | what the property counts as untouched at, so a changed one can be marked and reset |
| `[MergableProperty(false)]` | keeps the property out of the merged list when several objects are shown |
| `[PasswordPropertyText(true)]` | a password box. Note the argument: the bare attribute means `false` |
| `[TypeConverter(typeof(…))]` | used to parse and format, and for the fixed list of values if it has one |

### `System.ComponentModel.DataAnnotations`

| Attribute | Effect |
|---|---|
| `[Display(Name, Description, GroupName, Order)]` | all four at once |
| `[Editable(false)]` | read-only |
| `[DataType(MultilineText)]`, `[DataType(Password)]` | picks the editor |
| `[Range]`, `[Required]`, `[StringLength]`, `[RegularExpression]` | checked before the value is written |

Where the two families overlap, the single-purpose one wins: `[DisplayName]` beats
`[Display(Name = …)]`, because it can only mean one thing.

### The library's own

| Attribute | Effect |
|---|---|
| `[PropertyOrder(0)]` | where the property sits, lowest first. Honoured in every sort mode |
| `[PropertyEditor("Percent")]` | the editor to use instead of the one the type would resolve to |
| `[Expandable]` | the value can be opened into indented child rows. Also valid on a type |
| `[FilePath(kind, ".gpkg")]` | the property is a path: a box with a browse button, and the kind and extensions travel to `BrowseRequested`. See [editors.md](editors.md) |

```csharp
public class Layer
{
    [Category("Identity")]
    [DisplayName("Layer name")]
    [Description("What this layer is called in the legend.")]
    [PropertyOrder(0)]
    [Required]
    [StringLength(40, MinimumLength = 2)]
    public string Name { get; set; } = "Parcels";

    [Category("Appearance")]
    [Range(0, 100)]
    [DefaultValue(80)]
    public int Opacity { get; set; } = 80;

    [Category("Source")]
    [Expandable]
    public ServerSettings Server { get; } = new();
}
```

## Properties the grid never shows

Regardless of attributes: anything without a public getter, indexers, static properties, and
non-public properties. A property is read-only when it has no setter, a non-public one, or an
`init` accessor — which is an ordinary public setter as far as reflection is concerned, so without
that check every record property would look editable and quietly accept edits the language forbids.

A property whose getter throws becomes one row showing the exception message and refusing to be
edited. It does not take the rest of the grid with it.

## Describing a type you do not own

### The metadata store

Declarative, and applies everywhere:

```csharp
PropertyGridMetadata.Default
    .For<Rect>()
        .Property(rectangle => rectangle.X, property => property
            .DisplayName("Left")
            .Category("Position")
            .Order(0))
        .Property(rectangle => rectangle.Width, property => property.ReadOnly())
        .Ignore(rectangle => rectangle.Height);
```

Only what you mention is overridden; everything else still comes from the attributes. A rule written
for a base type or an interface applies to everything below it, and a rule written for a subclass
wins for that subclass. `Browsable(true)` brings back a property hidden with `[Browsable(false)]`.

Hand a grid its own store when the same type has to look different in two places:

```xml
<pg:PropertyGrid Metadata="{x:Bind InspectorMetadata}" SelectedObject="{x:Bind Shape}" />
```

### The auto-generating event

Imperative, per grid, and the shortest thing that works:

```csharp
grid.AutoGeneratingProperty += (_, arguments) =>
{
    if (arguments.Name.StartsWith("Internal", StringComparison.Ordinal))
    {
        arguments.Cancel = true;
    }
    else
    {
        arguments.DisplayName = Humanize(arguments.Name);
    }
};
```

Every field starts at whatever the attributes and the store said, so you only change what you mean
to. `Cancel` leaves the property out.

### Replacing discovery entirely

When the properties are not CLR properties at all — a dictionary, an `ExpandoObject`, a schema read
at run time — implement `IPropertyDescriptionProvider` and hand it to the grid:

```csharp
internal sealed class SchemaProvider(Schema schema) : IPropertyDescriptionProvider
{
    public IReadOnlyList<PropertyDescription> GetProperties(Type type) =>
    [
        .. schema.Fields.Select(field => new PropertyDescription
        {
            Name = field.Name,
            PropertyType = field.ClrType,
            Accessor = new RecordAccessor(field.Name),
            CategoryName = field.Group,
            HelpText = field.Comment,
        }),
    ];
}
```

`PropertyAccessor` is where reading and writing live; derive from it to reach values that are not
behind a `PropertyInfo`. The grid does everything else — categories, sorting, editors, validation,
change notification — the same way it does for a plain object.

`ICustomTypeDescriptor` and `TypeDescriptionProviderAttribute` are deliberately not supported: this
covers the same ground with a much smaller surface.

## Arranging what it found

`PropertySort` decides the order:

| Mode | Effect |
|---|---|
| `NoSort` | declaration order, base types first, no categories |
| `Alphabetical` | by label, no categories |
| `Categorized` | grouped, each category where it first appears, properties in declaration order |
| `CategorizedAlphabetical` | grouped, categories and properties by name. The default |

An explicit `[PropertyOrder]` is honoured in every mode: it is a statement about where the property
belongs, and no view mode should quietly discard it. Properties that named no category land in
`Misc`, which always sorts last — a pile of unclassified properties above the ones somebody
bothered to classify reads as an accident.

## Validation

Four layers, in order, and each can be turned off through `ValidationMode`:

1. **Conversion.** Turning the typed text into the declared type. Never skipped.
2. **`IPropertyValidator`**, from `grid.Validators`, plus the cancellable `PropertyValueChanging`
   event. For rules that live outside the model.
3. **DataAnnotations** on the property, checked before the value is written.
4. **`INotifyDataErrorInfo`** on the object, read after it is written. This one is authoritative:
   the setter has already run and may have applied a rule the grid cannot see from outside.
   `ObservableValidator` from the CommunityToolkit implements it, and needs no glue.

A rejected edit leaves the typed text in the editor and the old value on the object. Both halves
matter: throwing the text away loses the user's work, and writing the value anyway defeats the
validation.

`IDataErrorInfo` is not supported — it has no change notification, so its errors would go stale.
