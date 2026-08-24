# WinUI 3, the hard way

Every entry here cost a bug, a crash or an afternoon while this library was being written. They are
written down because none of them is guessable from the documentation, several fail *silently*, and
the next person — or the next model — will otherwise pay for them again.

Each one says what happens, why, and what this library does about it.

## The XAML compiler drops some things without a word

### A dependency property called `Template` never gets set from markup

Declare `public DataTemplate? Template` as a DP on a plain `DependencyObject`, set it in XAML, and
it stays null. The object is constructed, every *other* property is set, and no warning or error is
produced. Setting it from code works, which is what makes it maddening: the C# and the XAML disagree
and only one of them is lying.

Presumably it collides with the well-known `Control.Template`.

> `PropertyEditorTemplate.ValueTemplate` is named that for this reason, and nothing else.

### `{ThemeResource}` does not cross between resource dictionaries, and the failure is fatal

The editor templates live in `Themes/PropertyGridEditors.xaml`, which is merged into
`Application.Resources`. The control styles live in `Themes/Generic.xaml`, which is **not** — it is
the control dictionary, reached through `DefaultStyleResourceUri`.

A `{ThemeResource SomeStyleInGeneric}` written in an editor template resolves to nothing and takes
the process down: exit code `0xC0000409`, a fault in `Microsoft.UI.Xaml.dll`, and no message.

> This is why `PropertyGridDateEditor` has a `Mode` enum instead of three styles to choose between.
> Nothing has to look anything up.

### `--` inside an XML comment is a parse error

Writing `--diagnose` in a comment in a `.xaml` file fails the build with `MSB3073` from
`XamlCompiler.exe` and no line number.

## Namescopes are smaller than they look

### `GetTemplateChild` cannot see inside another control's content

An element nested in a `Button`'s content, inside your control template, is outside the namescope
`GetTemplateChild` searches. It returns null and the part is silently missing.

> The colour editor lays its button *over* the swatch and the text rather than containing them.
> That is the only reason its template looks the way it does.

### A flyout builds its content lazily, in a namescope of its own

Parts inside `Button.Flyout` are not found by `GetTemplateChild` at any time. Hook the flyout's
`Opened` and find them with `FindName` on its content, which is also the first moment they exist.

## Bindings quietly change your values

### `null` reaching a nullable struct property becomes `default(T)`

Bind `CalendarDatePicker.Date` (a `DateTimeOffset?`) to a null with a classic `{Binding}` and the
picker receives `default(DateTimeOffset)` — the first of January, year one — which it clamps to the
earliest date it shows: a hundred years ago. An empty date drew as `1/1/1926`, which is a date
somebody could believe.

> The three date editors do not bind their pickers. One control pushes the values in and pulls them
> out, where "no date" can stay no date.

### `bool` does not convert to `Visibility` in a classic binding

`{x:Bind}` has the conversion; `{Binding}` does not. Panes whose visibility follows a property are
wired in code here rather than shipping a converter for two lines of plumbing.

### There is no `RelativeSource AncestorType`

`RelativeSourceMode` is `{None, TemplatedParent, Self}`. Nothing inside a `DataTemplate` can reach
the control that owns it by binding. Walk the visual tree, or have the owner push what is needed —
`PropertyGridRowPanel` gets its column width pushed in for exactly this reason.

### A `ColumnDefinition` is not a `FrameworkElement`

It has no `DataContext` and never inherits one, so `{Binding}` on `ColumnDefinition.Width` has no
source and fails with no exception and no output. Same trap for `Setter.Value`, `GradientStop` and
key frames.

## Editors write to your model before anyone touches them

A control being realized, or recycled onto another row, pushes its own empty state through any
two-way binding **before** it has been told what to show:

- a combo box whose `ItemsSource` is replaced resets `SelectedItem` to null;
- a fresh check box starts indeterminate;
- a fresh number box reports `NaN`;
- a fresh date picker reports no date.

In a property grid that means switching objects can clear a value on the object being left behind,
with nobody typing anything. Enumerations escape it only by accident: their member lists are cached
per type, so recycling between two rows of the same enum never actually replaces `ItemsSource`.

> Every typed property on `PropertyGridPropertyRow` refuses a null that was never a choice the user
> was offered, **and** raises a change notification afterwards — without the second half the control
> keeps showing its empty state for good, because a rejected write changes nothing for it to notice.
>
> The probe counts writes while nobody types. The answer has to be zero.

## Things that only look like they work

### A `ContentPresenter` given a `DataTemplateSelector` rebuilds its child every time

Even when the selector returns the template it already had. Measured: 0 reuses out of 66 recycles.

Resolve the template yourself and assign `ContentTemplate` only when it differs, and the same run
reuses it 37 times out of 66 — the other 29 being genuine changes of editor.

### A brush read from `Application.Resources` in code is the wrong theme

`{ThemeResource}` in a template follows the *element's* theme. `Resources.TryGetValue` in code
returns whichever theme was current, so writing one back paints dark-theme text on a light window.

> Nothing here writes a brush unless a description explicitly asked for one, and clears the property
> otherwise so the template's `{ThemeResource}` shows through.

### A `Style` setter for a `double` cannot say `Auto`

So a fixed `Height` in a control style cannot be undone by a consumer deriving from it — they would
have to copy the whole `ControlTemplate`. Row heights are `MinHeight` here for that reason as much
as for the layout.

### `DependencyObject` cannot be constructed without a XAML runtime

`new` on any `DependencyObject` subclass throws `COMException 0x80040154` in a plain test host. This
is not a limitation to work around; it is the reason the entire model layer — rows, descriptors,
conversion, validation — is plain observable objects. A `DependencyObject`-based row model would
mean no unit tests at all.

### `Application.Current` does not answer null outside an application — it throws

The documented answer is null when there is no application, and that is what happens in a XAML
designer or once a WinUI runtime has been activated in the process. In a plain test host, where it
never has been, the property is a WinRT activation like any other and asking for it throws
`COMException 0x80040154` — the same `REGDB_E_CLASSNOTREG` that `new` on a `DependencyObject` gives.

It stayed hidden here for as long as only control code read the application's resources, because
control code cannot run in a test host anyway. Reading a resource key from the *model* layer — which
is what made the grid's own sentences replaceable — put the call on a path 130 tests walk, and every
one of them failed at once.

> `PropertyGridThemeResources` catches it, treats it as "no application, use the built-in default",
> and remembers the answer: the throw is expensive, and a sentence is built for every rejected edit.

## Not WinUI, but caught here anyway

- **`[PasswordPropertyText]` with no argument means `false`.** It reads like an opt-in and is the
  opposite, so check the value rather than the presence.
- **.NET no longer validates path characters.** `new FileInfo("|")` succeeds. Whether a path is
  usable depends on the file system it lands on, so guessing only refuses paths that would work.
- **.NET is lenient about where group separators fall.** In a culture whose group separator is a
  dot, `double.TryParse("1.5", AllowThousands)` returns fifteen. Real numbers here are parsed
  without `AllowThousands`; whole numbers keep it, because there a dot can only have meant grouping.
- **Reading `Application.Current.Resources` in the `App` constructor throws
  `COMException 0x8000FFFF`** and kills the process before the first window, with no message.
  `InitializeComponent` only records where the dictionary comes from. Do it in `OnLaunched`.

## How these were found

Not by reading. Every one came from a measurement: a probe that counts what actually happened, a
screenshot taken and looked at, or a fix removed to check that the harness could still see the bug.
When you add a measurement, take the fix out and confirm it reports the failure — a harness that
always says zero is not evidence of anything.
