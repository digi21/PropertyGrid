using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Digi21.WinUI.PropertyGrid.Primitives;

/// <summary>Which halves of a moment a <see cref="PropertyGridDateEditor"/> lets the user set.</summary>
public enum PropertyGridDateEditorMode
{
    /// <summary>A calendar and a clock.</summary>
    DateAndTime,

    /// <summary>A calendar only.</summary>
    Date,

    /// <summary>A clock only.</summary>
    Time,
}

/// <summary>A calendar, a clock, or both, for the date and time properties.</summary>
/// <remarks>
/// <para>
/// A control rather than a plain template because of what a binding does to nothing. Bound with a
/// classic <c>{Binding}</c>, a null reaching <c>CalendarDatePicker.Date</c> arrives as
/// <c>default(DateTimeOffset)</c> — the first of January in year one — which the picker then clamps
/// to the earliest date it will show, a hundred years ago. An empty date came out as
/// <c>1/1/1926</c>, which is a date somebody could believe.
/// </para>
/// <para>
/// So nothing is bound: the values are pushed in and pulled out here, where "no date" can stay no
/// date and the picker shows its placeholder instead.
/// </para>
/// </remarks>
public partial class PropertyGridDateEditor : Control
{
    /// <summary>Identifies the <see cref="Row"/> dependency property.</summary>
    public static readonly DependencyProperty RowProperty = DependencyProperty.Register(
        nameof(Row),
        typeof(PropertyGridPropertyRow),
        typeof(PropertyGridDateEditor),
        new PropertyMetadata(null, (d, _) => ((PropertyGridDateEditor)d).OnRowChanged()));

    /// <summary>Identifies the <see cref="Mode"/> dependency property.</summary>
    public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
        nameof(Mode),
        typeof(PropertyGridDateEditorMode),
        typeof(PropertyGridDateEditor),
        new PropertyMetadata(PropertyGridDateEditorMode.DateAndTime, (d, _) => ((PropertyGridDateEditor)d).Show()));

    private CalendarDatePicker? calendar;
    private TimePicker? clock;
    private PropertyGridPropertyRow? subscribed;
    private bool showing;

    /// <summary>Initializes a new instance of the <see cref="PropertyGridDateEditor"/> class.</summary>
    public PropertyGridDateEditor()
    {
        DefaultStyleKey = typeof(PropertyGridDateEditor);
        DefaultStyleResourceUri = new Uri("ms-appx:///Digi21.WinUI.PropertyGrid/Themes/Generic.xaml");
        PropertyGridThemeResources.Ensure();

        DataContextChanged += (_, arguments) =>
        {
            if (arguments.NewValue is PropertyGridPropertyRow row)
            {
                Row = row;
            }
        };
    }

    /// <summary>Gets or sets the property being edited.</summary>
    public PropertyGridPropertyRow? Row
    {
        get => (PropertyGridPropertyRow?)GetValue(RowProperty);
        set => SetValue(RowProperty, value);
    }

    /// <summary>Gets or sets which halves of a moment the user can set.</summary>
    /// <remarks>
    /// One template holds both pickers and the mode hides whichever is not wanted, rather than there
    /// being three styles to choose between. The editor templates live in a dictionary merged into
    /// the application's resources and the styles would live in the control dictionary, which is not
    /// — a <c>{ThemeResource}</c> across that line resolves to nothing and takes the process with it.
    /// </remarks>
    public PropertyGridDateEditorMode Mode
    {
        get => (PropertyGridDateEditorMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        Detach();

        // Whichever parts the template has: one editor serves the calendar, the clock and both.
        calendar = GetTemplateChild("PART_Calendar") as CalendarDatePicker;
        clock = GetTemplateChild("PART_Clock") as TimePicker;

        if (calendar is not null)
        {
            calendar.DateChanged += OnCalendarChanged;
        }

        if (clock is not null)
        {
            clock.SelectedTimeChanged += OnClockChanged;
        }

        Show();
    }

    private void OnRowChanged()
    {
        if (subscribed is not null)
        {
            subscribed.PropertyChanged -= OnRowPropertyChanged;
        }

        subscribed = Row;

        if (subscribed is not null)
        {
            subscribed.PropertyChanged += OnRowPropertyChanged;
        }

        Show();
    }

    private void OnRowPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PropertyGridPropertyRow.DateValue)
            or nameof(PropertyGridPropertyRow.TimeValue)
            or nameof(PropertyGridPropertyRow.Value))
        {
            Show();
        }
    }

    // Row to controls. The flag is up throughout, because setting either picker raises the same
    // event the user does and the answer would go straight back to the row.
    private void Show()
    {
        if (showing)
        {
            return;
        }

        showing = true;
        try
        {
            if (calendar is not null)
            {
                calendar.Visibility = Mode == PropertyGridDateEditorMode.Time ? Visibility.Collapsed : Visibility.Visible;
                calendar.Date = Row?.DateValue;
                calendar.IsEnabled = Row?.IsEditable ?? false;
            }

            if (clock is not null)
            {
                clock.Visibility = Mode == PropertyGridDateEditorMode.Date ? Visibility.Collapsed : Visibility.Visible;
                clock.SelectedTime = Row?.TimeValue;
                clock.IsEnabled = Row?.IsEditable ?? false;
            }
        }
        finally
        {
            showing = false;
        }
    }

    private void OnCalendarChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs arguments)
    {
        if (!showing && Row is { } row)
        {
            row.DateValue = arguments.NewDate;
        }
    }

    private void OnClockChanged(object? sender, TimePickerSelectedValueChangedEventArgs arguments)
    {
        if (!showing && Row is { } row)
        {
            row.TimeValue = arguments.NewTime;
        }
    }

    private void Detach()
    {
        if (calendar is not null)
        {
            calendar.DateChanged -= OnCalendarChanged;
        }

        if (clock is not null)
        {
            clock.SelectedTimeChanged -= OnClockChanged;
        }
    }
}
