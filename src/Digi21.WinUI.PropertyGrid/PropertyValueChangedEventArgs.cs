namespace Digi21.WinUI.PropertyGrid;

/// <summary>Carries a value that has just been written.</summary>
public sealed class PropertyValueChangedEventArgs : EventArgs
{
    internal PropertyValueChangedEventArgs(PropertyGridPropertyRow row, object? oldValue, object? newValue)
    {
        Row = row;
        OldValue = oldValue;
        NewValue = newValue;
    }

    /// <summary>Gets the row that was edited.</summary>
    public PropertyGridPropertyRow Row { get; }

    /// <summary>Gets the value the property held before.</summary>
    public object? OldValue { get; }

    /// <summary>Gets the value it holds now, which is what the setter kept rather than what was typed.</summary>
    public object? NewValue { get; }
}
