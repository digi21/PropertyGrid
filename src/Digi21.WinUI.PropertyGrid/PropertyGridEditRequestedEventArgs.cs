namespace Digi21.WinUI.PropertyGrid;

/// <summary>Says that the user pressed the button beside a value that needs more room than a row.</summary>
/// <remarks>
/// <para>
/// This is the grid's equivalent of the modal editors a desktop property grid has always had: a
/// list, a complex object, a type only the application knows how to ask for. The grid shows the
/// value and offers the button; what opens is entirely up to the handler.
/// </para>
/// <para>
/// The handler is free to take as long as it likes. There is no deferral and no result property:
/// the way to report what was chosen is to write <c>Row.Value</c> whenever the answer arrives, and
/// the row is observable, so a write from a continuation reaches the grid on its own. For a list
/// edited in place, mutating the list and calling <c>Row.Refresh()</c> works just as well.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// grid.EditRequested += async (_, arguments) =>
/// {
///     if (arguments.Row.Value is IList&lt;string&gt; items)
///     {
///         ListDialog dialog = new(items) { XamlRoot = grid.XamlRoot };
///         if (await dialog.ShowAsync() == ContentDialogResult.Primary)
///         {
///             arguments.Row.Value = dialog.Result;
///         }
///     }
/// };
/// </code>
/// </example>
public sealed class PropertyGridEditRequestedEventArgs : EventArgs
{
    internal PropertyGridEditRequestedEventArgs(PropertyGridPropertyRow row) => Row = row;

    /// <summary>Gets the row being edited. Write its <see cref="PropertyGridPropertyRow.Value"/> to report a new value.</summary>
    public PropertyGridPropertyRow Row { get; }
}
