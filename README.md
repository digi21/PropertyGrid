<img src="https://raw.githubusercontent.com/digi21/PropertyGrid/main/assets/icon-256.png" width="96" alt="" />

# Digi21.WinUI.PropertyGrid

[![CI](https://github.com/digi21/PropertyGrid/actions/workflows/ci.yml/badge.svg)](https://github.com/digi21/PropertyGrid/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Digi21.WinUI.PropertyGrid.svg)](https://www.nuget.org/packages/Digi21.WinUI.PropertyGrid)
[![Downloads](https://img.shields.io/nuget/dt/Digi21.WinUI.PropertyGrid.svg)](https://www.nuget.org/packages/Digi21.WinUI.PropertyGrid)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

A property grid for WinUI 3. Point it at an object and it lists the object's properties as editable
rows: an editor chosen from each property's type, grouped into categories, labelled from the
attributes already on your model, validated, and kept in step with the object as it changes.

<img src="https://raw.githubusercontent.com/digi21/PropertyGrid/main/assets/gallery.png" width="820" alt="The gallery, showing categories, editors and a nested object" />

## Features

- **Reflection.** Give it an object, or a type, and the rows build themselves.
- **An editor per type.** Text, numbers, booleans, enumerations, flags, dates, times, durations,
  colours, brushes and more — including the cases that are easy to get wrong, like `decimal` and
  `long` staying away from a double-backed number box.
- **Paths**, with a browse button that asks your application to open the dialog — it knows the
  filter, the starting folder and the window handle; a library does not.
- **A resizable name column.** One splitter drags it in every row at once, at every level of nesting.
- **Categories** that collapse, from `[Category]` or `[Display(GroupName)]`.
- **The attributes you already have**: `[Browsable]`, `[Category]`, `[Description]`,
  `[DisplayName]`, `[ReadOnly]`, `[DefaultValue]`, and the DataAnnotations equivalents.
- **Validation** from DataAnnotations, from validators you supply, and from the object's own
  `INotifyDataErrorInfo`.
- **Nested objects**, opened into indented child rows in the same list, with the columns still lined
  up and cycles in the graph refused rather than followed.
- **`ObservableObject` and anything else** that raises `INotifyPropertyChanged`. Edit the object
  from elsewhere and the grid follows; the subscription is weak, so a transient inspector does not
  pin your view model's lifetime to it.
- **Replaceable everywhere.** Every colour is a resource key, every editor is a `DataTemplate` you
  can redeclare, and every control has a `Default…Style` to derive from.
- **Searching and filtering**, by text or by a predicate of your own.

## Requirements

Windows App SDK 1.8 or later, .NET 8 or later. A package built against 1.8 works unchanged in an
application on 2.x: the WinUI assembly identity is the same and its surface is a superset, and NuGet
resolves the dependency to the highest version in the graph.

## Installation

```
dotnet add package Digi21.WinUI.PropertyGrid
```

## Quickstart

```xml
<Page
    xmlns:pg="using:Digi21.WinUI.PropertyGrid">

    <pg:PropertyGrid SelectedObject="{x:Bind Layer}" />
</Page>
```

That is the whole integration. The model needs nothing from the library:

```csharp
public class Layer : ObservableObject
{
    private string name = "Parcels";
    private int opacity = 80;

    [Category("Identity")]
    [DisplayName("Layer name")]
    [Description("What this layer is called in the legend.")]
    [Required]
    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    [Category("Appearance")]
    [Range(0, 100)]
    [DefaultValue(80)]
    public int Opacity
    {
        get => opacity;
        set => SetProperty(ref opacity, value);
    }
}
```

### Arranging the rows

```xml
<pg:PropertyGrid
    PropertySort="Categorized"
    SelectedObject="{x:Bind Layer}"
    ShowDescription="True"
    ShowSearchBox="True" />
```

`PropertySort` is `NoSort`, `Alphabetical`, `Categorized` or `CategorizedAlphabetical`. `FilterText`
narrows the list to what matches, and `Filter` takes a predicate for anything text cannot express.

### Nested objects

A property opens into indented child rows when it, or its type, says so:

```csharp
[Expandable]
public ServerSettings Server { get; } = new();
```

Set `ExpansionPolicy="Automatic"` to offer it for every object with properties of its own, which
suits an inspector and does not suit a settings dialog.

### Showing a type

```xml
<pg:PropertyGrid SelectedType="local:Layer" />
```

Every row is read-only: there is no object to read a value from. Useful for looking at the shape of
a type rather than at an instance of it.

### Reacting to edits

```csharp
grid.PropertyValueChanging += (_, arguments) =>
{
    if (arguments.Row.Name == nameof(Layer.Name) && IsTaken(arguments.NewValue))
    {
        arguments.Cancel = true;
        arguments.ErrorMessage = "That name is already used.";
    }
};

grid.PropertyValueChanged += (_, arguments) => document.MarkDirty();
```

`NewValue` on the changed event is what the setter actually kept, which is not always what was
typed: a property that clamps its input reports the clamped value.

### Custom editors

```xml
<pg:PropertyGrid SelectedObject="{x:Bind Layer}">
    <pg:PropertyGrid.EditorTemplates>
        <pg:PropertyEditorTemplateMap>
            <pg:PropertyEditorTemplate Key="Percent">
                <DataTemplate>
                    <Slider Maximum="100" Minimum="0" Value="{Binding DoubleValue, Mode=TwoWay}" />
                </DataTemplate>
            </pg:PropertyEditorTemplate>
        </pg:PropertyEditorTemplateMap>
    </pg:PropertyGrid.EditorTemplates>
</pg:PropertyGrid>
```

```csharp
[PropertyEditor("Percent")]
public int Opacity { get; set; }
```

To replace a whole category of editors rather than the one for a type, declare a `DataTemplate`
under the matching name from `PropertyEditorKeys` in your `App.xaml` — every boolean in the
application changes at once. See [docs/editors.md](https://github.com/digi21/PropertyGrid/blob/main/docs/editors.md).

### Theming

Every brush the grid uses is an alias of a WinUI system brush, so it follows the accent colour, both
themes and high contrast on its own. Redeclare a key to change one:

```xml
<SolidColorBrush x:Key="PropertyGridCategoryBackgroundBrush" Color="#20FFFFFF" />
```

See [docs/theming.md](https://github.com/digi21/PropertyGrid/blob/main/docs/theming.md) for the full
list, the metrics, and how to retemplate a control.

### Types you do not own

```csharp
PropertyGridMetadata.Default
    .For<Rect>()
        .Property(rectangle => rectangle.X, property => property.DisplayName("Left").Category("Position"))
        .Ignore(rectangle => rectangle.Height);
```

Or handle `AutoGeneratingProperty` per grid, or replace discovery entirely with an
`IPropertyDescriptionProvider` when the properties are not CLR properties at all. See
[docs/metadata.md](https://github.com/digi21/PropertyGrid/blob/main/docs/metadata.md).

## Not in this version

- Editing the fields of a struct in place. A struct property shows its text form and does not open,
  because a child row would write to a copy and the edit would be silently lost.
- Adding to and removing from a collection. A list shows a summary; the dialog for editing one is
  not written yet.
- Showing several objects at once (`SelectedObjects`).
- `IDictionary`.

## Sample

`samples/PropertyGridGallery` exercises every feature and is the fastest way to try one:

```
dotnet run --project samples/PropertyGridGallery
```

## Contributing

Issues and pull requests are welcome — see
[CONTRIBUTING.md](https://github.com/digi21/PropertyGrid/blob/main/CONTRIBUTING.md).

## License

[MIT](https://github.com/digi21/PropertyGrid/blob/main/LICENSE)
