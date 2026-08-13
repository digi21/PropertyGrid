namespace Digi21.WinUI.PropertyGrid;

/// <summary>Says that the user pressed the browse button beside a path, and what the path accepts.</summary>
/// <remarks>
/// <para>
/// The handler is free to take as long as it likes. Nothing here has to be filled in before it
/// returns: a picker is asynchronous, and the way to report what was chosen is to write
/// <c>Row.Value</c> whenever the answer arrives. The row is observable, so a write from a
/// continuation shows up in the grid on its own.
/// </para>
/// <para>
/// That is why there is no deferral and no result property. Both would exist only to make an
/// asynchronous answer look synchronous, and neither is needed.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// grid.BrowseRequested += async (_, arguments) =>
/// {
///     FileOpenPicker picker = new();
///     InitializeWithWindow.Initialize(picker, windowHandle);
///     foreach (string extension in arguments.Extensions)
///     {
///         picker.FileTypeFilter.Add(extension);
///     }
///
///     if (await picker.PickSingleFileAsync() is { } file)
///     {
///         arguments.Row.Value = file.Path;
///     }
/// };
/// </code>
/// </example>
public sealed class PropertyGridBrowseRequestedEventArgs : EventArgs
{
    internal PropertyGridBrowseRequestedEventArgs(PropertyGridPropertyRow row, FilePathKind kind, IReadOnlyList<string> extensions)
    {
        Row = row;
        Kind = kind;
        Extensions = extensions;
    }

    /// <summary>Gets the row being edited. Write its <see cref="PropertyGridPropertyRow.Value"/> to report what was chosen.</summary>
    public PropertyGridPropertyRow Row { get; }

    /// <summary>Gets what the property is a path to, and what will be done with it.</summary>
    public FilePathKind Kind { get; }

    /// <summary>Gets the extensions the property accepts, each with its leading dot, or an empty list for any.</summary>
    public IReadOnlyList<string> Extensions { get; }

    /// <summary>Gets the path the property holds now, for a picker to start from.</summary>
    public string? CurrentPath => Row.Text;
}
