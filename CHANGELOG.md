# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project
adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased] — towards 0.1.0

Everything below works and is covered by tests or by the gallery, but nothing has been through a
real application yet, which is why the first version will be 0.1 rather than 1.0 and why the public
API can still change. Builds before the tag are published as `0.1.0-dev.N`.

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
- A path editor — a box with a browse button — for `[FilePath]` properties and for anything typed
  `FileInfo` or `DirectoryInfo`. The button raises `BrowseRequested` carrying the kind of path and
  the extensions it accepts; the application opens whatever dialog is right and writes the row's
  value when the answer arrives. The grid never opens a file dialog itself.
- A dialog editor — a summary with a `…` button — for lists, for complex objects, and for anything
  asking for `[PropertyEditor(PropertyEditorKeys.Dialog)]`. The button raises `EditRequested`, and
  does not appear at all unless something is handling it. It is also how a struct property gets a
  real editor, since the grid will not open one into child rows.
- `PropertyGridPropertyRow.AllowsEditing`, which asks whether an editor should offer to change a
  value at all, as distinct from `IsEditable`, which asks whether the property can be assigned. A
  list declared with only a getter is not assignable and is still meant to be edited.
- `PropertyDescription.StandardValues`: the values one particular property accepts, each with the
  label to show for it. Per property rather than per type, and read at run time, which is what a
  `TypeConverter` cannot do — two coded fields of the same table are both `int` and accept
  different sets. Declaring them is enough to get the drop-down; unset, the type's converter is
  asked exactly as before.
- `PropertyGrid.Culture` and `PropertyGrid.DefaultCategoryName`, for an application that chooses
  its own language instead of following Windows.
- Every word the templates show is now a resource key — `PropertyGridSearchPlaceholderText`,
  `PropertyGridBrowseToolTipText`, `PropertyGridEditToolTipText`, `PropertyGridOkButtonText`,
  `PropertyGridCancelButtonText` — and the sentences built at run time live on the public
  `PropertyGridStrings`. Nothing user-visible is hard-coded in a template any more.

### Fixed

- A row could not grow with its editor. The row style set `Height` rather than `MinHeight`, so an
  editor taller than a line was clipped, and there was no way to undo it from outside: a `Setter`
  for a double does not parse `Auto`. Rows now grow, and the default row height moves from 28 to 32
  so that a text box, a number box and a check box all measure the same.
- A property declared as `object` and holding a string was given the read-only complex editor,
  because a string has properties of its own. A property declared vaguely now resolves its editor
  from the type of the value it holds.
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

- A struct property does not open into child rows: a child would write to a copy and the edit would
  be silently lost. It shows its text form, and can be given a dialog editor instead.
- No dialog is supplied for editing a list. The grid offers the button and the summary; what opens
  is up to the application.
- Showing several objects at once (`SelectedObjects`) is not implemented.
- `IDictionary` is not handled specially.
- No editor for `FontFamily`. The name `PropertyEditorKeys.FontFamily` is reserved so an application
  can supply one; listing the installed families needs interop the package will not depend on.
