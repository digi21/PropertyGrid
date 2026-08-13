# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project
adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-08-13

First version. Everything below works and is covered by tests or by the gallery, but nothing has
been through a real application yet — hence 0.1 rather than 1.0. The public API can still change.

### Added

#### The grid

- `PropertyGrid`, showing the properties of `SelectedObject` as editable rows, or the shape of
  `SelectedType` as read-only ones.
- A name column the user can drag, with one splitter resizing every row at once, at every level of
  nesting. Double-clicking it fits the column to the names on screen.
- Categories that collapse, ordered by `PropertySort`: `NoSort`, `Alphabetical`, `Categorized` or
  `CategorizedAlphabetical`.
- A description pane explaining the selected property, and showing why an edit was rejected.
- Filtering by `FilterText` or by a `Filter` predicate, with an optional built-in search box.
- Keyboard navigation: arrows between rows, left and right to close and open, Home and End,
  F2 or Enter into the editor, Escape back out.

#### Reading a type

- Discovery by reflection, honouring `[Browsable]`, `[EditorBrowsable]`, `[Category]`,
  `[Description]`, `[DisplayName]`, `[ReadOnly]`, `[DefaultValue]`, `[MergableProperty]`,
  `[PasswordPropertyText]` and `[TypeConverter]`, plus the DataAnnotations `[Display]`, `[Editable]`
  and `[DataType]`.
- `[PropertyOrder]`, `[PropertyEditor]` and `[Expandable]` for what those do not cover.
- `PropertyGridMetadata`, a fluent store describing types you cannot annotate, per grid or shared.
- An `AutoGeneratingProperty` event for the same thing imperatively.
- `IPropertyDescriptionProvider` and `PropertyAccessor` for objects whose properties are not CLR
  properties at all.

#### Editing

- Editors for text, multiline text, passwords, every numeric type, booleans and nullable booleans,
  enumerations, `[Flags]` enumerations, dates, times, durations, colours, brushes, the geometry
  structs, and a fixed list of values from a type converter.
- `PropertyEditorTemplateMap`, registering an editor for a type, an interface and its
  implementations, or a name a property asks for.
- Replacing a whole category of editors by redeclaring a `DataTemplate` under a
  `PropertyEditorKeys` name in the application's resources.
- An `EditorSelecting` event for decisions the type system does not carry.
- Nested objects opened into indented child rows in the same list, with cycles in the graph refused
  and a depth limit.

#### Reacting

- Two-way with anything raising `INotifyPropertyChanged`, subscribed weakly so a transient grid does
  not pin the object's lifetime to it.
- Validation from DataAnnotations, from `IPropertyValidator`s the grid is given, from a cancellable
  `PropertyValueChanging` event, and from the object's own `INotifyDataErrorInfo`.
- A rejected edit keeps the typed text in the editor and the old value on the object.

#### Appearance

- Every brush an alias of a WinUI system brush, in Default, Light and HighContrast dictionaries, and
  every metric a named resource — all replaceable from the application's resources without
  retemplating anything.
- A `Default…Style` per control to derive from, and a documented `PART_` contract for replacing one
  outright.

### Known limitations

- A struct property shows its text form and does not open into child rows: a child would write to a
  copy and the edit would be silently lost.
- A collection shows a summary; adding, removing and reordering are not implemented.
- Showing several objects at once (`SelectedObjects`) is not implemented.
- `IDictionary` is not handled specially.
- No editor for `FontFamily`. The name `PropertyEditorKeys.FontFamily` is reserved so an application
  can supply one; listing the installed families needs interop the package will not depend on.
