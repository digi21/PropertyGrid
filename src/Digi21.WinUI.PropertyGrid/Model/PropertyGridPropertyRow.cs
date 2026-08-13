using System.ComponentModel;

namespace Digi21.WinUI.PropertyGrid;

/// <summary>One property of an object, as a <see cref="PropertyGrid"/> shows and edits it.</summary>
/// <remarks>
/// <para>
/// Editor templates bind the typed properties on this row — <see cref="Text"/>,
/// <see cref="DoubleValue"/>, <see cref="SelectedEnumMember"/> and the rest — rather than binding
/// <see cref="Value"/> through a value converter. A converter cannot see the declared type, has
/// nowhere useful to report a bad parse, and would have to be written once per pair of editor and
/// type. These funnel every write through one place that knows the type, the culture and the
/// validation attributes.
/// </para>
/// <para>
/// An edit that fails to convert or validate leaves the typed text in the editor and the old value
/// on the object. Both halves matter: throwing the text away loses the user's work, and writing the
/// value anyway defeats the validation.
/// </para>
/// </remarks>
public sealed class PropertyGridPropertyRow : PropertyGridRow
{
    private readonly List<PropertyGridRow> children = [];
    private readonly List<string> errors = [];
    private object? value;
    private string text = string.Empty;
    private bool isExpandable;
    private bool childrenBuilt;
    private bool writing;

    internal PropertyGridPropertyRow(
        PropertyGridSource source,
        PropertyDescription description,
        object target,
        string key,
        int depth,
        PropertyGridCategoryRow? category,
        PropertyGridPropertyRow? parent)
        : base(source, key, depth)
    {
        Description = description;
        Target = target;
        Category = category;
        Parent = parent;

        ReadFromTarget();
    }

    /// <summary>Gets what the grid knows about the property before reading it.</summary>
    public PropertyDescription Description { get; }

    /// <summary>Gets the object the property is read from and written to.</summary>
    public object Target { get; }

    /// <summary>Gets the category header the row sits under, if the grid is showing categories.</summary>
    public PropertyGridCategoryRow? Category { get; }

    /// <summary>Gets the row whose value this property belongs to, or <see langword="null"/> at the top level.</summary>
    public PropertyGridPropertyRow? Parent { get; }

    /// <summary>Gets the name of the property as it is declared in code.</summary>
    public string Name => Description.Name;

    /// <inheritdoc />
    public override string DisplayName => Description.DisplayName;

    /// <summary>Gets the sentence explaining the property, shown in the description pane.</summary>
    public string? HelpText => Description.HelpText;

    /// <summary>Gets the declared type of the property, which is what decides its editor.</summary>
    public Type PropertyType => Description.PropertyType;

    /// <summary>Gets the type of the value the property currently holds, which can be more specific than <see cref="PropertyType"/>.</summary>
    public Type? RuntimeType => value?.GetType();

    /// <summary>Gets a value indicating whether the grid refuses to write the property.</summary>
    public bool IsReadOnly => Description.IsReadOnly || Source.IsReadOnly || !Description.Accessor.CanWrite;

    /// <summary>Gets a value indicating whether the editor accepts input, which is the opposite of <see cref="IsReadOnly"/>.</summary>
    public bool IsEditable => !IsReadOnly;

    /// <summary>
    /// Gets a value indicating whether an editor should offer to change the value at all, including
    /// by changing what it holds rather than by replacing it.
    /// </summary>
    /// <remarks>
    /// Not the same as <see cref="IsEditable"/>, and the difference matters for lists. A collection
    /// is very often declared with only a getter — <c>public IList&lt;int&gt; Scales { get; } = [];</c>
    /// — and is still meant to be edited, by adding to it rather than by assigning a new one. What
    /// settles it is whether the grid was made read-only or the property was marked as such, not
    /// whether it happens to have a setter.
    /// </remarks>
    public bool AllowsEditing
    {
        get
        {
            if (Source.IsReadOnly)
            {
                return false;
            }

            if (Description.GetAttribute<System.ComponentModel.ReadOnlyAttribute>() is { IsReadOnly: true })
            {
                return false;
            }

            return Description.GetAttribute<System.ComponentModel.DataAnnotations.EditableAttribute>() is not { AllowEdit: false };
        }
    }

    /// <summary>Gets or sets the value of the property.</summary>
    /// <remarks>
    /// Setting this runs the whole write path: coercion to the declared type, the validators, the
    /// cancellable value-changing event, the write itself, and then a re-read, because a setter is
    /// free to store something other than what it was handed.
    /// </remarks>
    public object? Value
    {
        get => value;
        set => TryWrite(value);
    }

    /// <summary>Gets or sets the value of the property as text, in the grid's culture.</summary>
    public string Text
    {
        get => text;
        set
        {
            if (string.Equals(text, value, StringComparison.Ordinal))
            {
                return;
            }

            text = value ?? string.Empty;
            RaisePropertyChanged();

            if (IsReadOnly)
            {
                return;
            }

            if (PropertyValueConverter.TryParse(text, PropertyType, Source.Culture, out object? parsed, out string? error))
            {
                TryWrite(parsed);
            }
            else
            {
                SetErrors(error is null ? [] : [error]);
            }
        }
    }

    /// <summary>Gets or sets the value as a number, for the editors backed by a number box.</summary>
    /// <remarks>
    /// A number box works in doubles, so <see cref="long"/>, <see cref="ulong"/> and
    /// <see cref="decimal"/> lose precision past 2^53 and are given a text editor instead. Nothing
    /// stops this property from being read for them; the editor resolution is what keeps it from
    /// happening.
    /// </remarks>
    public double DoubleValue
    {
        get => value is IConvertible convertible ? SafeToDouble(convertible) : double.NaN;
        set
        {
            if (double.IsNaN(value) && !AcceptsNull)
            {
                RejectEditorDefault();
                return;
            }

            TryWrite(double.IsNaN(value) ? null : value);
        }
    }

    /// <summary>Gets or sets the value as a three-state flag, for the check box editors.</summary>
    public bool? NullableBoolValue
    {
        get => value as bool?;
        set
        {
            if (value is null && !AcceptsNull)
            {
                RejectEditorDefault();
                return;
            }

            TryWrite(value);
        }
    }

    /// <summary>Gets or sets the value as a date, for the calendar editors.</summary>
    public DateTimeOffset? DateValue
    {
        get => value switch
        {
            DateTimeOffset offset => offset,
            DateTime moment => new DateTimeOffset(moment.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(moment, DateTimeKind.Local)
                : moment),
            DateOnly date => new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local)),
            _ => null,
        };
        set
        {
            if (value is null && !AcceptsNull)
            {
                RejectEditorDefault();
                return;
            }

            TryWrite(value is null ? null : FromDate(value.Value));
        }
    }

    /// <summary>Gets or sets the time of day part of the value, for the clock editors.</summary>
    public TimeSpan? TimeValue
    {
        get => value switch
        {
            TimeSpan span => span,
            TimeOnly time => time.ToTimeSpan(),
            DateTimeOffset offset => offset.TimeOfDay,
            DateTime moment => moment.TimeOfDay,
            _ => null,
        };
        set
        {
            if (value is null && !AcceptsNull)
            {
                RejectEditorDefault();
                return;
            }

            TryWrite(value is null ? null : FromTime(value.Value));
        }
    }

    /// <summary>Gets the members to choose from when the property is an enumeration, or an empty list.</summary>
    public IReadOnlyList<EnumMemberRow> EnumMembers { get; private set; } = [];

    /// <summary>Gets or sets the chosen member when the property is an enumeration.</summary>
    public EnumMemberRow? SelectedEnumMember
    {
        get
        {
            foreach (EnumMemberRow member in EnumMembers)
            {
                if (Equals(member.Value, value))
                {
                    return member;
                }
            }

            return null;
        }

        set
        {
            if (value is null && !OffersNull(EnumMembers.Select(member => member.Value)))
            {
                RejectEditorDefault();
                return;
            }

            TryWrite(value?.Value);
        }
    }

    /// <summary>Gets the flags to tick when the property is a <see cref="FlagsAttribute"/> enumeration, or an empty list.</summary>
    public IReadOnlyList<FlagMemberRow> FlagMembers { get; private set; } = [];

    /// <summary>Gets the values the property accepts, each with its label, or an empty list.</summary>
    /// <remarks>
    /// Taken from <see cref="PropertyDescription.StandardValues"/> when the description named them,
    /// and from the type's converter otherwise.
    /// </remarks>
    public IReadOnlyList<PropertyStandardValue> StandardValues { get; private set; } = [];

    /// <summary>Gets or sets the chosen entry when the property accepts a fixed set of values.</summary>
    public PropertyStandardValue? SelectedStandardValue
    {
        get
        {
            foreach (PropertyStandardValue standard in StandardValues)
            {
                if (Equals(standard.Value, value))
                {
                    return standard;
                }
            }

            return null;
        }

        set
        {
            if (value is null && !OffersNull(StandardValues.Select(standard => standard.Value)))
            {
                RejectEditorDefault();
                return;
            }

            TryWrite(value?.Value);
        }
    }

    /// <summary>Gets a value indicating whether the value differs from the one the property declares as its default.</summary>
    public bool IsModified => Description.HasDefaultValue && !Equals(value, Description.DefaultValue);

    /// <summary>Gets a value indicating whether the value can be put back to the declared default.</summary>
    public bool CanResetValue => !IsReadOnly && IsModified;

    /// <summary>Gets a value indicating whether the last edit was rejected.</summary>
    public bool HasErrors => errors.Count > 0;

    /// <summary>Gets the first thing wrong with the value, or <see langword="null"/> if nothing is.</summary>
    public string? ErrorMessage => errors.Count > 0 ? errors[0] : null;

    /// <summary>Gets everything wrong with the value.</summary>
    public IReadOnlyList<string> Errors => errors;

    /// <inheritdoc />
    public override bool IsExpandable => isExpandable;

    /// <summary>Gets the rows shown underneath this one when it is open.</summary>
    public IReadOnlyList<PropertyGridRow> Children => children;

    /// <summary>Puts the value back to the one the property declares as its default.</summary>
    public void ResetValue()
    {
        if (CanResetValue)
        {
            TryWrite(Description.DefaultValue);
        }
    }

    /// <summary>Reads the value from the object again, discarding any rejected edit.</summary>
    public void Refresh()
    {
        ReadFromTarget();
        RaiseValueChanged();
    }

    internal void ReadFromTarget()
    {
        if (Description.Accessor.TryGetValue(Target, out object? read, out Exception? failure))
        {
            value = read;
            errors.Clear();
        }
        else
        {
            // A getter that throws is one broken row, not a broken window. The row shows why and
            // refuses to be edited, and every other property of the object still works.
            value = null;
            errors.Clear();
            errors.Add(failure?.Message ?? string.Empty);
        }

        text = PropertyValueConverter.ToText(value, Source.Culture);
        RefreshChoices();
        RefreshExpandability();
        CollectTargetErrors();
    }

    internal void RefreshExpandability()
    {
        bool expandable = Source.CanExpand(this, value);
        if (isExpandable == expandable)
        {
            return;
        }

        isExpandable = expandable;
        RaisePropertyChanged(nameof(IsExpandable));

        if (!expandable)
        {
            SetExpandedQuietly(false);
            InvalidateChildren();
        }
    }

    internal void EnsureChildren()
    {
        if (childrenBuilt)
        {
            return;
        }

        childrenBuilt = true;
        children.Clear();
        if (value is not null)
        {
            children.AddRange(Source.BuildChildren(this, value));
        }
    }

    internal void InvalidateChildren()
    {
        childrenBuilt = false;
        children.Clear();
    }

    internal void CollectTargetErrors()
    {
        if (!Source.ValidationMode.HasFlag(PropertyGridValidationMode.DataErrorInfo)
            || Target is not INotifyDataErrorInfo reporter)
        {
            return;
        }

        // Whatever the object says wins: its setter has already run and may have applied a rule the
        // grid has no way to see.
        List<string> reported = [];
        System.Collections.IEnumerable? entries = reporter.GetErrors(Name);
        if (entries is not null)
        {
            foreach (object? entry in entries)
            {
                if (entry is not null)
                {
                    reported.Add(entry.ToString() ?? string.Empty);
                }
            }
        }

        if (reported.Count > 0 || errors.Count > 0)
        {
            SetErrors(reported);
        }
    }

    // Whether the property can hold nothing at all: a reference, or a Nullable<T>.
    private bool AcceptsNull =>
        !PropertyType.IsValueType || Nullable.GetUnderlyingType(PropertyType) is not null;

    // Whether "nothing" is one of the things the user was offered. A list with no empty entry means
    // a null coming out of the control cannot have been chosen.
    private static bool OffersNull(IEnumerable<object?> offered)
    {
        foreach (object? candidate in offered)
        {
            if (candidate is null)
            {
                return true;
            }
        }

        return false;
    }

    // A control that has just been created, or recycled onto another row, pushes its own empty state
    // through its two-way binding before it has been told what to show: a combo box whose items were
    // replaced resets its selection to null, and a fresh check box starts indeterminate. That is not
    // the user clearing anything, and writing it would either erase the value or - when the property
    // refuses null - leave the control showing something the model never held.
    //
    // So the write is dropped and the control is told to read again, which snaps it back to the
    // truth. Without the second half the control keeps showing its empty state for good, because a
    // rejected write changes nothing for it to notice.
    private void RejectEditorDefault([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null) =>
        RaisePropertyChanged(propertyName);

    private static double SafeToDouble(IConvertible convertible)
    {
        try
        {
            return convertible.ToDouble(null);
        }
        catch (Exception exception) when (exception is FormatException or InvalidCastException or OverflowException)
        {
            return double.NaN;
        }
    }

    private bool TryWrite(object? proposed)
    {
        // Editors write back while the grid is pushing a value into them, and a target that raises
        // its own change notification writes back again. Without this the round trip never settles.
        if (writing || IsReadOnly)
        {
            return false;
        }

        if (!PropertyValueConverter.TryCoerce(proposed, PropertyType, Source.Culture, out object? coerced, out string? conversionError))
        {
            SetErrors(conversionError is null ? [] : [conversionError]);
            return false;
        }

        if (Equals(coerced, value))
        {
            SetErrors([]);
            return true;
        }

        IReadOnlyList<string> rejections = PropertyValidationRunner.Validate(this, coerced);
        if (rejections.Count > 0)
        {
            SetErrors(rejections);
            return false;
        }

        object? previous = value;
        if (!Source.OnValueChanging(this, previous, coerced, out string? veto))
        {
            SetErrors(veto is null ? [] : [veto]);
            return false;
        }

        writing = true;
        try
        {
            if (!Description.Accessor.TrySetValue(Target, coerced, out Exception? failure))
            {
                SetErrors([failure?.Message ?? string.Empty]);
                return false;
            }

            // The setter is free to store something other than what it was handed - clamping a
            // number, normalising a path - and the row has to show what was actually kept.
            ReadFromTarget();
        }
        finally
        {
            writing = false;
        }

        RaiseValueChanged();
        Source.OnValueChanged(this, previous, value);
        return true;
    }

    private object? FromDate(DateTimeOffset chosen) => PropertyType switch
    {
        _ when KnownTypes.Unwrap(PropertyType) == typeof(DateOnly) => DateOnly.FromDateTime(chosen.LocalDateTime),
        _ when KnownTypes.Unwrap(PropertyType) == typeof(DateTime) => Combine(chosen.LocalDateTime),
        _ => chosen,
    };

    private object? FromTime(TimeSpan chosen) => KnownTypes.Unwrap(PropertyType) switch
    {
        Type type when type == typeof(TimeOnly) => TimeOnly.FromTimeSpan(chosen),
        Type type when type == typeof(TimeSpan) => chosen,
        Type type when type == typeof(DateTime) => (value as DateTime? ?? DateTime.Today).Date + chosen,
        Type type when type == typeof(DateTimeOffset) =>
            new DateTimeOffset((value as DateTimeOffset? ?? DateTimeOffset.Now).Date + chosen),
        _ => chosen,
    };

    // The calendar only picks a day, so the time already on the value has to survive being edited.
    private DateTime Combine(DateTime chosen) => value is DateTime existing
        ? chosen.Date + existing.TimeOfDay
        : chosen;

    private void RefreshChoices()
    {
        Type underlying = KnownTypes.Unwrap(PropertyType);

        // Values named by the description come first even for an enumeration: naming them is an
        // explicit statement, and an enum is only the default answer to the same question.
        if (!underlying.IsEnum || Description.StandardValues is { Count: > 0 })
        {
            RefreshStandardValues(underlying);
            return;
        }

        if (EnumMembers.Count == 0)
        {
            EnumMembers = EnumInfo.MembersOf(underlying);

            if (EnumInfo.IsFlags(underlying))
            {
                List<FlagMemberRow> flags = [];
                foreach (EnumMemberRow member in EnumMembers)
                {
                    ulong bits = EnumInfo.BitsOf(member.Value);

                    // The zero member of a flags enumeration is "none", not a flag: showing it as a
                    // tick box that can never be ticked off is worse than not showing it.
                    if (bits != 0)
                    {
                        FlagMemberRow flag = new(member, bits);
                        flag.Toggled += OnFlagToggled;
                        flags.Add(flag);
                    }
                }

                FlagMembers = flags;
            }
        }

        if (FlagMembers.Count > 0)
        {
            ulong current = value is null ? 0UL : EnumInfo.BitsOf(value);
            foreach (FlagMemberRow flag in FlagMembers)
            {
                flag.SetCheckedQuietly((current & flag.Bits) == flag.Bits);
            }
        }
    }

    private void RefreshStandardValues(Type underlying)
    {
        if (StandardValues.Count > 0)
        {
            return;
        }

        // What the description says wins. It knows things the type cannot: two coded fields of the
        // same table are both int and accept entirely different sets.
        if (Description.StandardValues is { Count: > 0 } declared)
        {
            StandardValues = declared;
            return;
        }

        System.ComponentModel.TypeConverter converter = System.ComponentModel.TypeDescriptor.GetConverter(underlying);
        if (!converter.GetStandardValuesSupported())
        {
            return;
        }

        List<PropertyStandardValue> values = [];
        foreach (object? standard in converter.GetStandardValues() ?? Array.Empty<object?>())
        {
            // A converter offers values and no labels, so the value has to speak for itself.
            values.Add(new PropertyStandardValue(standard, PropertyValueConverter.ToText(standard, Source.Culture)));
        }

        StandardValues = values;
    }

    private void OnFlagToggled(object? sender, EventArgs arguments)
    {
        if (writing)
        {
            return;
        }

        ulong bits = 0;
        foreach (FlagMemberRow flag in FlagMembers)
        {
            if (flag.IsChecked)
            {
                bits |= flag.Bits;
            }
        }

        Type underlying = KnownTypes.Unwrap(PropertyType);
        TryWrite(Enum.ToObject(underlying, unchecked((long)bits)));
    }

    private void SetErrors(IReadOnlyList<string> replacement)
    {
        if (errors.Count == replacement.Count && errors.SequenceEqual(replacement, StringComparer.Ordinal))
        {
            return;
        }

        errors.Clear();
        errors.AddRange(replacement);
        RaisePropertyChanged(nameof(Errors));
        RaisePropertyChanged(nameof(HasErrors));
        RaisePropertyChanged(nameof(ErrorMessage));
    }

    private void RaiseValueChanged()
    {
        RaisePropertyChanged(nameof(Value));
        RaisePropertyChanged(nameof(Text));
        RaisePropertyChanged(nameof(RuntimeType));
        RaisePropertyChanged(nameof(DoubleValue));
        RaisePropertyChanged(nameof(NullableBoolValue));
        RaisePropertyChanged(nameof(DateValue));
        RaisePropertyChanged(nameof(TimeValue));
        RaisePropertyChanged(nameof(SelectedEnumMember));
        RaisePropertyChanged(nameof(SelectedStandardValue));
        RaisePropertyChanged(nameof(IsModified));
        RaisePropertyChanged(nameof(CanResetValue));
        RaisePropertyChanged(nameof(Errors));
        RaisePropertyChanged(nameof(HasErrors));
        RaisePropertyChanged(nameof(ErrorMessage));
    }
}
