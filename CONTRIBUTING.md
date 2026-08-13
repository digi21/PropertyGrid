# Contributing

Thanks for taking an interest in Digi21.WinUI.PropertyGrid. Issues and pull requests are welcome.

The library is still before its first release, so the public API can still change. If you are
about to build something sizeable on top of it, or your change touches the public API, please
open an issue first: it is cheaper to agree on the shape of an API than to redo a pull request.

## Building and running

You need Windows, the .NET 8 SDK or later, and the Windows App SDK 1.8 packages (they restore from
NuGet; there is no workload to install). Visual Studio is optional; everything below works from the
command line.

```
dotnet build
dotnet test
dotnet run --project samples/PropertyGridGallery
```

The repository holds three projects:

- `src/Digi21.WinUI.PropertyGrid` — the library, and the only thing that ships.
- `samples/PropertyGridGallery` — an unpackaged WinUI app exercising every feature. It is the
  fastest way to try a change by hand, and the place to reproduce a bug.
- `tests/Digi21.WinUI.PropertyGrid.Tests` — xUnit tests.

The library depends on nothing but the Windows App SDK. In particular it does **not** depend on
`CommunityToolkit.Mvvm`: `ObservableObject` and `ObservableValidator` are supported through
`INotifyPropertyChanged` and `INotifyDataErrorInfo`, which are BCL interfaces. The sample references
the toolkit to prove that works with no glue; the tests hand-roll their own doubles so that the test
project stays dependency-free. Please keep both true.

## Reporting bugs

Say which type you were inspecting and what the property looked like — the declared type, whether it
was nullable, and which attributes it carried. Most grid bugs are really "this property resolved to
the wrong editor" or "the value did not write back", and the property's declaration is what decides
both. Please include the Windows build, the Windows App SDK version, and whether the app is packaged
or unpackaged. If you can reproduce it in `samples/PropertyGridGallery`, say how.

## Tests

The tests cover everything that does not need a XAML runtime, which here is most of the library:
property discovery, attribute reading, metadata overrides, ordering and categorization, value
conversion, enum and flag decomposition, change notification, validation, row flattening, and editor
resolution. Those are expected to grow with any change to them.

There is no UI test harness. Control templates, splitter dragging, virtualization and recycling,
focus and keyboard behaviour, and the visuals of each editor are validated by running the gallery and
trying them. Please say in the pull request what you tried by hand.

Two rules keep the tests possible, and both are easy to break by accident:

- **The model layer must not activate a WinRT type.** Rows, descriptors, converters and validators
  are plain objects, not `DependencyObject`s, and they may not call `new SolidColorBrush()` or read
  `Microsoft.UI.Colors`. Reflecting over a type is fine — `typeof(Color)` does not activate anything.
- **Anything read from a resource dictionary in code goes through
  `PropertyGridThemeResources.Value<T>(key, fallback)` with a usable fallback**, because
  `Application.Current` is null in the designer and in tests.

## Code style

`.editorconfig` carries the formatting rules, and the build treats warnings as errors, including
missing XML documentation on public members. Beyond that:

- Public types and members need XML documentation that says what they are for, not what they are
  called. Everything else uses plain `//` comments: the compiler writes a `<member>` entry for
  every `///` comment whatever its accessibility, so a `///` on an internal member ships in the
  package's `.xml` and reads as API that does not exist.
- Comments explain *why*. A lot of this code is shaped by WinUI constraints that look arbitrary
  until you know them — there is no `RelativeSource AncestorType`, `ColumnDefinition` has no
  `DataContext`, `NumberBox` is backed by a `double` — and those are worth a sentence. Comments
  restating the code are not.
- Every value a template shows comes from a `{ThemeResource PropertyGrid*}` key, never a hard-coded
  colour or size. That is what lets an application recolour the grid without retemplating it.
- The type `PropertyGrid` lives in the namespace `Digi21.WinUI.PropertyGrid`. This is legal and
  deliberate, but never fully qualify it inside the library: write `PropertyGrid`, not
  `Digi21.WinUI.PropertyGrid.PropertyGrid`.
- Match the surrounding code: file-scoped namespaces, nullable enabled, explicit types over `var`,
  no abbreviations in names.

## Commits and pull requests

Commit messages are in English and follow the conventional style used in the history
(`feat:`, `fix:`, `chore:`, `docs:`), with a body explaining the reasoning when the subject is not
self-explanatory. Keep one topic per pull request; unrelated cleanups are easier to review on their
own.

When your change affects the public API or the behaviour a user can see, update `README.md` and add
an entry to `CHANGELOG.md` under `Unreleased`.

CI builds the solution, runs the tests and packs the library on every pull request; it has to pass.

## License

By contributing you agree that your contributions are licensed under the
[MIT License](LICENSE), like the rest of the project.
