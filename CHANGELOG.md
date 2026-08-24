# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project
adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed

- **Breaking.** `PropertyGridStrings` is gone. Every string it held is now a resource key with the
  same English behind it, declared in `Themes/PropertyGridResources.xaml` and replaced the way a
  brush is: `PropertyGridDefaultCategoryName`, `PropertyGridNotAValidFormat`,
  `PropertyGridRequiredValueFormat`, `PropertyGridCannotConvertFormat`,
  `PropertyGridCollectionSummaryFormat` and the nine type names —
  `PropertyGridWholeNumberName`, `PropertyGridNumberName`, `PropertyGridBooleanName`,
  `PropertyGridCharacterName`, `PropertyGridTextName`, `PropertyGridDateTimeName`,
  `PropertyGridDateName`, `PropertyGridTimeName`, `PropertyGridDurationName`.

  There was no reason for two mechanisms. What a template shows was already a key; what the grid
  built at run time was fourteen static properties, on the grounds that a validation message has no
  control to ask — which does not hold, because a resource lookup does not need one. Being static
  also made them process-wide rather than per application, so two windows, or a host and a plug-in
  that both use this library, shared one set.

  Replacing `PropertyGridStrings.X = "…"` with an `<x:String x:Key="PropertyGridX">…</x:String>` in
  `App.xaml`, or an entry written into `Application.Current.Resources`, is the whole migration.
  There is no shim: one that wrote into the resource dictionary would leave two ways to do it, which
  is the thing being fixed.

- `NameOf(Type)` moves to the new `PropertyGridText` and reads the keys. A `DateTime` is still a
  "date and time" rather than a `DateTime`.

- The keys are read where they are used rather than cached, so a translation declared after the
  first grid already exists reaches every sentence built from then on; what rows already show
  follows the next time they are built. An entry put straight into `Application.Current.Resources`
  wins over the library's own default whichever of the two arrives first.

### Added

- `PropertyGridText.ResourceKeys`, every resource key the grid reads text from — the six a template
  paints as well as the fourteen it builds. A key is not a name the compiler checks, and that is the
  one respect in which this change is a step back: an entry filed under a key the library no longer
  reads fails in silence, and the string reverts to English somewhere nobody looks. Walking this
  list where an application declares its strings turns both that and an untranslated key into a
  startup error. One read-only member replaces fourteen settable ones, and
  [docs/theming.md](docs/theming.md#checking-a-translation-at-startup) has the twelve lines.

- `PropertyGridSelectDatePlaceholderText` is documented; it shipped in 1.0.0 and was missing from
  the list in `docs/theming.md`.

### Fixed

- Asking for `Application.Current` outside an application throws `COMException 0x80040154` rather
  than answering null, and every read of a library resource key went through it unguarded. Nothing
  had noticed, because until now only control code — which cannot run without a XAML runtime anyway
  — read one. Reading a key from the model layer would have thrown on every rejected edit in a
  console host or a test. It is caught, treated as "no application, use the built-in default", and
  remembered.

- `docs/theming.md` gave `PropertyGridRowHeight` as 28. It has been 32 since 1.0.0.

## [1.0.0] — 2026-08-15

The first release. Everything below is covered by tests or by the gallery, and — the part that had
been missing, and the reason this was going to be 0.1 — it has now been integrated into a real
application and works there. The public API is what that integration used, so it is the one being
committed to: from here, anything that breaks it takes a 2.0.

Development builds between releases are published as `<next version>-dev.N`, where N counts commits.

### Added

#### The grid

- `PropertyGrid`, showing the properties of `SelectedObject` as editable rows, or the shape of
  `SelectedType` as read-only ones.
- A name column the user can drag, with one splitter resizing every row at once, at every level of
  nesting. Double-clicking it fits the column to the names on screen.
- Categories that collapse, ordered by `PropertySort`: `NoSort`, `Alphabetical`, `Categorized` or
  `CategorizedAlphabetical`.
- A description pane explaining the selected property, and showing why an edit was rejected. The
  divider above it drags with the mouse to make it taller or shorter, double-clicking that divider
  fits it to the text, and an explanation too long for the height it has been given scrolls inside
  it. `DescriptionHeight`, `MinimumDescriptionHeight`, `MinimumRowsHeight` and `CanResizeDescription`
  set and bound it; `AutoSizeDescription()` is the double-click from code.
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
- Two properties of the same type could not have different editors. Resolution was memoized by
  declared type, runtime type and requested name, and that stopped identifying an editor as soon as
  a single property could carry its own list of values or its own `[FilePath]`: whichever string
  resolved first decided for every other string in the object. The memoization is gone — resolving
  is a handful of type comparisons run once per row realized.
- An editor being realized or recycled wrote its own empty state into the model. A combo box whose
  items are replaced resets its selection to null and pushes it through the two-way binding, so
  switching objects cleared the value on the one being left behind. A null now reaches the model
  only when nothing is a choice the list actually offered, and the control is told to read again so
  it does not sit there showing an empty state.
- A date that was not set drew as a real one. Bound through a classic `{Binding}`, a null reaching
  `CalendarDatePicker.Date` arrives as `default(DateTimeOffset)` and is clamped to the earliest date
  the picker shows — a hundred years ago — so an empty column read as `1/1/1926`. The three date
  editors are now one control that pushes and pulls the values itself, and an empty date shows the
  placeholder.
- A `bool` holding true was drawn as an indeterminate check box. Same cause seen from the other
  side: the box pushed null on creation, the write was refused because a `bool` cannot be cleared,
  and nothing then told the box to go and read the real value. The same guard now covers a number
  box pushing `NaN`.
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
