# Editors

Each row's value cell is a `DataTemplate` whose data context is the
`PropertyGridPropertyRow` being edited. The grid picks one per row from the type of the property.

## What each type gets

| Type | Editor | Key |
|---|---|---|
| `string` | text box | `PropertyGridStringEditorTemplate` |
| `string` with `[DataType(MultilineText)]` | wrapping text box | `PropertyGridMultilineStringEditorTemplate` |
| `string` with `[PasswordPropertyText(true)]` or `[DataType(Password)]` | password box | `PropertyGridPasswordEditorTemplate` |
| `char`, `Guid`, `Uri`, `Version` | text box | `PropertyGridStringEditorTemplate` |
| `byte`…`int`, `uint`, `float`, `double` | number box | `PropertyGridNumberEditorTemplate` |
| `long`, `ulong`, `decimal` | text box | `PropertyGridLargeNumberEditorTemplate` |
| `bool` | check box | `PropertyGridBooleanEditorTemplate` |
| `bool?` | three-state check box | `PropertyGridNullableBooleanEditorTemplate` |
| an enumeration | drop-down | `PropertyGridEnumEditorTemplate` |
| a `[Flags]` enumeration | drop-down of tick boxes | `PropertyGridFlagsEnumEditorTemplate` |
| `DateTime`, `DateTimeOffset` | calendar and clock | `PropertyGridDateTimeEditorTemplate` |
| `DateOnly` | calendar | `PropertyGridDateEditorTemplate` |
| `TimeOnly` | clock | `PropertyGridTimeEditorTemplate` |
| `TimeSpan` | text box | `PropertyGridTimeSpanEditorTemplate` |
| `Color` | swatch opening a picker | `PropertyGridColorEditorTemplate` |
| a `Brush` | swatch opening a picker | `PropertyGridBrushEditorTemplate` |
| `Thickness`, `CornerRadius`, `Point`, `Size`, `Rect` | text box | `PropertyGridStructEditorTemplate` |
| a type whose converter has a fixed list of values | drop-down | `PropertyGridStandardValuesEditorTemplate` |
| a list | a summary | `PropertyGridCollectionEditorTemplate` |
| an object with properties | a summary, and a chevron in the name column | `PropertyGridComplexEditorTemplate` |
| anything else | selectable text | `PropertyGridReadOnlyEditorTemplate` |

Three of those are not the obvious choice, on purpose:

- **`long`, `ulong` and `decimal` do not get the number box.** A `NumberBox` works in doubles, so
  anything past 2⁵³ would come back with its low bits rounded away and nothing would say so.
- **`TimeSpan` does not get a `TimePicker`.** A picker is a clock: it cannot express anything longer
  than a day or shorter than zero, and durations routinely are both.
- **`bool?` is not unwrapped to `bool`.** The third state is the whole point of it.

`PropertyGridFontFamilyEditorTemplate` is a name the library reserves and ships no template for:
listing the installed font families needs DirectWrite interop the package will not take a dependency
on. Declare a template under that key and `[PropertyEditor(PropertyEditorKeys.FontFamily)]` will find
it.

## Replacing an editor everywhere

Each built-in editor is a `DataTemplate` merged into your application's resources under the key in
the table. Declaring one of your own under the same key replaces it throughout the application —
the same way redeclaring a brush recolours the grid:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
        </ResourceDictionary.MergedDictionaries>

        <!-- Every bool in the application now gets a switch instead of a tick box. -->
        <DataTemplate x:Key="PropertyGridBooleanEditorTemplate">
            <ToggleSwitch
                IsEnabled="{Binding IsEditable}"
                IsOn="{Binding NullableBoolValue, Mode=TwoWay}"
                OffContent=""
                OnContent="" />
        </DataTemplate>
    </ResourceDictionary>
</Application.Resources>
```

The names are also constants on `PropertyEditorKeys`, so a property can ask for one by name:

```csharp
[PropertyEditor(PropertyEditorKeys.MultilineString)]
public string Summary { get; set; } = string.Empty;
```

## Registering an editor for a type

To change the editor for particular types rather than for a whole category of them, register a
`PropertyEditorTemplate` on the grid:

```xml
<pg:PropertyGrid SelectedObject="{x:Bind Layer}">
    <pg:PropertyGrid.EditorTemplates>
        <pg:PropertyEditorTemplateMap>

            <!-- one type -->
            <pg:PropertyEditorTemplate TargetType="local:Rating">
                <DataTemplate>
                    <RatingControl MaxRating="5" Value="{Binding DoubleValue, Mode=TwoWay}" />
                </DataTemplate>
            </pg:PropertyEditorTemplate>

            <!-- an interface, and everything implementing it -->
            <pg:PropertyEditorTemplate TargetType="local:IGeometry" MatchDerivedTypes="True">
                <DataTemplate>
                    <TextBlock Text="{Binding Text}" TextTrimming="CharacterEllipsis" />
                </DataTemplate>
            </pg:PropertyEditorTemplate>

            <!-- opted into per property with [PropertyEditor("Percent")] -->
            <pg:PropertyEditorTemplate Key="Percent">
                <DataTemplate>
                    <Slider Maximum="100" Minimum="0" Value="{Binding DoubleValue, Mode=TwoWay}" />
                </DataTemplate>
            </pg:PropertyEditorTemplate>

        </pg:PropertyEditorTemplateMap>
    </pg:PropertyGrid.EditorTemplates>
</pg:PropertyGrid>
```

`PropertyEditorTemplateMap.Default` is the same thing for the whole application, and takes the same
entries from C#.

For a decision the type system cannot carry — this `int` is a percentage, that one is a port number
— handle `EditorSelecting`, which is asked before anything else and is never cached:

```csharp
grid.EditorSelecting += (_, arguments) =>
{
    if (arguments.Row.Name == nameof(Layer.Opacity))
    {
        arguments.Template = percentEditor;
    }
};
```

## What a template binds

Bind the typed properties on the row, not `Value` through a converter. A converter cannot see the
declared type, has nowhere useful to report a bad parse, and would have to be written once per pair
of editor and type.

| Property | For |
|---|---|
| `Text` | anything with a text form. Writing it parses in the grid's culture and reports a failure on the row |
| `DoubleValue` | number boxes and sliders |
| `NullableBoolValue` | tick boxes, including the three-state one |
| `DateValue` | calendars. A `DateTimeOffset?` whatever the property's own date type is |
| `TimeValue` | clocks |
| `EnumMembers`, `SelectedEnumMember` | drop-downs over an enumeration |
| `FlagMembers` | a checklist over a `[Flags]` enumeration; ticking one recomposes the value |
| `StandardValues`, `SelectedStandardValue` | drop-downs over a converter's fixed list |
| `Value` | anything else, coerced to the declared type on the way in |
| `IsEditable`, `IsReadOnly` | whether to let the user type |
| `HasErrors`, `ErrorMessage` | what the last edit was rejected for |

A template declared in a resource dictionary cannot carry event handlers. `TextEditorBehaviors`
supplies the two a text editor needs:

```xml
<TextBox
    primitives:TextEditorBehaviors.CommitOnEnter="True"
    primitives:TextEditorBehaviors.SelectAllOnFocus="True"
    Text="{Binding Text, Mode=TwoWay}" />
```

Without `CommitOnEnter` the value is written when the box loses focus, which after pressing Enter
feels like the grid ignored the keystroke. Escape puts back what the row holds.

## Resolution order

First match wins:

1. the `EditorSelecting` event
2. the name the property asked for with `[PropertyEditor]`, in the grid's map, then the shared one
3. the type the value actually is, when the property is declared as `object`, an interface or an
   abstract class
4. the declared type, exactly
5. the type inside a nullable, exactly, then unwrapped
6. base classes and interfaces, for entries with `MatchDerivedTypes="True"`, nearest and most
   derived first
7. a template in the application's resources named by `[PropertyEditor]`
8. the built-in table above

Within a map, the last entry registered wins, so an application can always override what a library
set up. The grid's own map is searched before `PropertyEditorTemplateMap.Default`.

Resolution is memoized. After registering editors at run time, or replacing a template in the
application's resources, call `grid.InvalidateEditors()`.

## Writing a custom editor

The only real constraint is that the template gets one element, and it should stretch. Everything
else is an ordinary control bound to the row:

```xml
<pg:PropertyEditorTemplate TargetType="local:FilePath">
    <DataTemplate>
        <Grid ColumnSpacing="4">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*" />
                <ColumnDefinition Width="Auto" />
            </Grid.ColumnDefinitions>
            <TextBox
                BorderThickness="0"
                IsReadOnly="{Binding IsReadOnly}"
                Text="{Binding Text, Mode=TwoWay}" />
            <Button
                Grid.Column="1"
                Command="{Binding BrowseCommand, Source={StaticResource Commands}}"
                Content="…" />
        </Grid>
    </DataTemplate>
</pg:PropertyEditorTemplate>
```

If it needs handlers, make it a control and put the control in the template — which is what the
colour editor does, because OK and Cancel cannot be expressed in markup alone.
