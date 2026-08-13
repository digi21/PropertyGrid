# Theming

The grid paints nothing with a literal colour. Every brush it uses is an alias of a WinUI system
brush, and every size comes from a named resource, so it follows the accent colour, the light and
dark themes and high contrast without any help — and any one of those values can be replaced by
declaring the same key in your application.

## Overriding the colours

Declare the keys you want to change in `App.xaml`, inside theme dictionaries:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />

            <!--
                Theme dictionaries only take effect inside a *merged* dictionary. Declaring
                ResourceDictionary.ThemeDictionaries directly in Application.Resources silently does
                nothing, which is the single most common way this goes wrong.
            -->
            <ResourceDictionary>
                <ResourceDictionary.ThemeDictionaries>
                    <ResourceDictionary x:Key="Default">
                        <SolidColorBrush x:Key="PropertyGridCategoryBackgroundBrush" Color="#20FFFFFF" />
                    </ResourceDictionary>
                    <ResourceDictionary x:Key="Light">
                        <SolidColorBrush x:Key="PropertyGridCategoryBackgroundBrush" Color="#14000000" />
                    </ResourceDictionary>
                </ResourceDictionary.ThemeDictionaries>
            </ResourceDictionary>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

You do not have to merge anything of the library's: it inserts its own dictionary at the bottom of
`Application.Resources` the first time a grid is created, so whatever you declare is found first.

### Surfaces and rows

| Key | Defaults to | Paints |
|---|---|---|
| `PropertyGridBackgroundBrush` | `LayerFillColorDefaultBrush` | behind the whole grid |
| `PropertyGridBorderBrush` | `CardStrokeColorDefaultBrush` | the outer border |
| `PropertyGridRowBackgroundBrush` | `SubtleFillColorTransparentBrush` | a property row at rest |
| `PropertyGridRowAlternateBackgroundBrush` | `SubtleFillColorSecondaryBrush` | every other row, when banding is on |
| `PropertyGridRowPointerOverBackgroundBrush` | `SubtleFillColorSecondaryBrush` | the row under the pointer |
| `PropertyGridRowSelectedBackgroundBrush` | `SubtleFillColorTertiaryBrush` | the selected row |
| `PropertyGridRowSeparatorBrush` | `DividerStrokeColorDefaultBrush` | the line between rows |
| `PropertyGridColumnRuleBrush` | `DividerStrokeColorDefaultBrush` | the rule between the two columns |
| `PropertyGridIndentGuideBrush` | `DividerStrokeColorDefaultBrush` | the guides down a nested run |

### The name column

| Key | Defaults to | Paints |
|---|---|---|
| `PropertyGridNameForegroundBrush` | `TextFillColorPrimaryBrush` | a property name |
| `PropertyGridModifiedNameForegroundBrush` | `AccentTextFillColorPrimaryBrush` | a name whose value differs from its default |
| `PropertyGridValueForegroundBrush` | `TextFillColorPrimaryBrush` | a value |
| `PropertyGridReadOnlyForegroundBrush` | `TextFillColorSecondaryBrush` | a value the grid will not write |
| `PropertyGridExpanderGlyphBrush` | `TextFillColorSecondaryBrush` | the chevrons |

### Category headers

| Key | Defaults to |
|---|---|
| `PropertyGridCategoryBackgroundBrush` | `CardBackgroundFillColorSecondaryBrush` |
| `PropertyGridCategoryForegroundBrush` | `TextFillColorSecondaryBrush` |
| `PropertyGridCategoryBorderBrush` | `DividerStrokeColorDefaultBrush` |

### The splitter

| Key | Defaults to |
|---|---|
| `PropertyGridSplitterBrush` | `DividerStrokeColorDefaultBrush` |
| `PropertyGridSplitterPointerOverBrush` | `AccentFillColorDefaultBrush` |
| `PropertyGridSplitterPressedBrush` | `AccentFillColorTertiaryBrush` |

### Validation and the description pane

| Key | Defaults to |
|---|---|
| `PropertyGridErrorBorderBrush` | `SystemFillColorCriticalBrush` |
| `PropertyGridErrorForegroundBrush` | `SystemFillColorCriticalBrush` |
| `PropertyGridErrorBackgroundBrush` | `SystemFillColorCriticalBackgroundBrush` |
| `PropertyGridDescriptionPaneBackgroundBrush` | `CardBackgroundFillColorDefaultBrush` |
| `PropertyGridDescriptionPaneBorderBrush` | `DividerStrokeColorDefaultBrush` |
| `PropertyGridDescriptionTitleForegroundBrush` | `TextFillColorPrimaryBrush` |
| `PropertyGridDescriptionForegroundBrush` | `TextFillColorSecondaryBrush` |
| `PropertyGridColorSwatchBorderBrush` | `ControlStrokeColorDefaultBrush` |

## Metrics

The same in every theme, so they go in the dictionary root rather than in a theme dictionary.

| Key | Type | Default |
|---|---|---|
| `PropertyGridRowHeight` | `Double` | 28 |
| `PropertyGridCategoryHeaderHeight` | `Double` | 30 |
| `PropertyGridNameColumnWidth` | `Double` | 160 |
| `PropertyGridMinimumNameColumnWidth` | `Double` | 48 |
| `PropertyGridMinimumValueColumnWidth` | `Double` | 64 |
| `PropertyGridSplitterThickness` | `Double` | 6 |
| `PropertyGridSplitterGripThickness` | `Double` | 1 |
| `PropertyGridIndentSize` | `Double` | 14 |
| `PropertyGridExpanderSize` | `Double` | 16 |
| `PropertyGridDescriptionPaneHeight` | `Double` | 64 |
| `PropertyGridNamePadding` | `Thickness` | `6,0,6,0` |
| `PropertyGridEditorPadding` | `Thickness` | `4,1,4,1` |
| `PropertyGridBorderThickness` | `Thickness` | `1` |
| `PropertyGridCornerRadius` | `CornerRadius` | `4` |

The last four column metrics reach their properties through setters in `DefaultPropertyGridStyle`,
so setting `NameColumnWidth` on a grid directly still wins over them.

`PropertyGridSplitterThickness`, `PropertyGridSplitterGripThickness` and `PropertyGridIndentSize`
are read from code as well as from templates — the layout cannot express them — so replacing them
takes effect the next time a row is laid out.

## Words

Everything the templates put on screen is a resource key, replaced the same way a brush is:

| Key | Defaults to |
|---|---|
| `PropertyGridSearchPlaceholderText` | `Search properties` |
| `PropertyGridBrowseToolTipText` | `Browse…` |
| `PropertyGridEditToolTipText` | `Edit…` |
| `PropertyGridOkButtonText` | `OK` |
| `PropertyGridCancelButtonText` | `Cancel` |

The sentences the grid builds at run time — why an edit was rejected, how a list is summarised, the
name of the catch-all category — are not in a template and cannot be resource keys. They live on
`PropertyGridStrings`, which is public and settable:

```csharp
PropertyGridStrings.DefaultCategoryName = Loc("Varios");
PropertyGridStrings.NotAValidFormat = Loc("'{0}' no es un {1} válido.");
PropertyGridStrings.WholeNumberName = Loc("número entero");
```

Set them once, early. `PropertyGrid.Culture` and `PropertyGrid.DefaultCategoryName` are per grid,
for an application that chooses its own language rather than following Windows.

[localisation.md](localisation.md) has all of this translated into nine languages, contributed by an
application that ships in them.

> **Do it in `OnLaunched`, not in the `App` constructor.** Reading
> `Application.Current.Resources` from the constructor throws `COMException 0x8000FFFF`: the
> dictionary does not exist yet, because `InitializeComponent` only records where it comes from.
> The process dies before the first window appears and says nothing about why, which makes it an
> expensive mistake to debug — and the constructor is the obvious place to put this.

```csharp
protected override void OnLaunched(LaunchActivatedEventArgs args)
{
    PropertyGridStrings.DefaultCategoryName = Loc("Varios");
    Resources["PropertyGridSearchPlaceholderText"] = Loc("Buscar propiedades");

    window = new MainWindow();
    window.Activate();
}
```

## Icons and text

Glyphs come from the symbol font, so they follow the shell rather than shipping as assets:
`PropertyGridIconFontFamily`, `PropertyGridIconFontSize`, `PropertyGridExpandedGlyph`,
`PropertyGridCollapsedGlyph`, `PropertyGridErrorGlyph`, `PropertyGridResetGlyph`,
`PropertyGridEllipsisGlyph`, `PropertyGridClearGlyph`.

Text styles chain to the WinUI type ramp, so they follow it: `PropertyGridNameTextStyle`,
`PropertyGridValueTextStyle`, `PropertyGridCategoryTextStyle`, `PropertyGridDescriptionTextStyle`.

## Retemplating

When a colour is not enough, derive from the default style rather than copying it. Merge the
library's control dictionary so `BasedOn` can find it:

```xml
<ResourceDictionary Source="ms-appx:///Digi21.WinUI.PropertyGrid/Themes/Generic.xaml" />
```

then:

```xml
<Style
    x:Key="MyPropertyGridStyle"
    BasedOn="{StaticResource DefaultPropertyGridStyle}"
    TargetType="pg:PropertyGrid">
    <Setter Property="CornerRadius" Value="0" />
</Style>
```

Every control has one: `DefaultPropertyGridStyle`, `DefaultPropertyGridRowPresenterStyle`,
`DefaultPropertyGridCategoryHeaderStyle`, `DefaultPropertyGridSplitterStyle`,
`DefaultPropertyGridDescriptionPaneStyle` and `DefaultPropertyGridColorEditorStyle`.

If you replace a template wholesale, keep the `PART_` names. The control finds its pieces by them,
and a missing part is not an error — the feature just quietly stops working.

| Control | Parts |
|---|---|
| `PropertyGrid` | `PART_ScrollViewer`, `PART_ItemsRepeater`, `PART_Splitter`, `PART_SearchBox`, `PART_DescriptionPane` |
| `PropertyGridRowPresenter` | `PART_RowPanel`, `PART_NameCell`, `PART_Expander`, `PART_NameText`, `PART_Gutter`, `PART_EditorHost`, `PART_ErrorIcon` |
| `PropertyGridCategoryHeader` | `PART_ExpanderGlyph`, `PART_Title` |
| `PropertyGridSplitter` | `PART_Grip` |
| `PropertyGridDescriptionPane` | `PART_Title`, `PART_Text` |
| `PropertyGridColorEditor` | `PART_Swatch`, `PART_OpenButton`, and `PART_Picker` / `PART_Confirm` / `PART_Cancel` inside its flyout |

Two constraints a replacement template has to respect:

- **`PART_RowPanel` lays its first three children out by position**: the name cell, the gutter, then
  the editor host. It is a `PropertyGridRowPanel` rather than a `Grid` because a repeater measures
  every row on its own, and WinUI has neither a shared size scope nor an ancestor binding to carry
  one column width into all of them — so the grid pushes it in instead. Replacing the panel with a
  `Grid` breaks the alignment the splitter depends on.
- **`PART_ScrollViewer` must not scroll horizontally.** The splitter is drawn over the scrolling
  area rather than inside it, so that it reads as one unbroken line and never scrolls away. The
  control forces this from code as well as declaring it in the template, so a retemplate cannot
  silently break it.

## How it works

The library merges `Themes/PropertyGridResources.xaml` into `Application.Resources` at the *bottom*
of the collection, the first time one of its controls is constructed. That makes it a layer of
defaults: anything the application declares, and any dictionary it merges itself, is looked up
first. It is what `XamlControlsResources` does for WinUI's own controls, except that you do not have
to add anything to `App.xaml`.

The keys deliberately do not live in `Themes/Generic.xaml` beside the templates that use them. A
`{ThemeResource}` inside a control template resolves against the dictionary the template was parsed
in before it looks at `Application.Resources`, so keeping them together would make them outrank
anything an application declared, and the grid could only be recoloured by retemplating it. The
editor templates live in a third dictionary for exactly the same reason — see
[editors.md](editors.md).
