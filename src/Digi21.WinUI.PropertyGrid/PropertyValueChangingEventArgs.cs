namespace Digi21.WinUI.PropertyGrid;

/// <summary>Carries a value about to be written, with a chance to refuse it.</summary>
public sealed class PropertyValueChangingEventArgs : EventArgs
{
    internal PropertyValueChangingEventArgs(PropertyGridPropertyRow row, object? oldValue, object? newValue)
    {
        Row = row;
        OldValue = oldValue;
        NewValue = newValue;
    }

    /// <summary>Gets the row being edited.</summary>
    public PropertyGridPropertyRow Row { get; }

    /// <summary>Gets the value the property holds now.</summary>
    public object? OldValue { get; }

    /// <summary>Gets the value about to replace it, already converted to the property's type.</summary>
    public object? NewValue { get; }

    /// <summary>Gets or sets a value indicating whether the write is refused.</summary>
    public bool Cancel { get; set; }

    /// <summary>Gets or sets what to tell the user when the write is refused.</summary>
    public string? ErrorMessage { get; set; }
}
