namespace Digi21.WinUI.PropertyGrid;

/// <summary>What a <see cref="FilePathAttribute"/> property is a path to, and what will be done with it.</summary>
public enum FilePathKind
{
    /// <summary>A file that has to exist.</summary>
    OpenFile,

    /// <summary>A file that will be written, and does not have to exist yet.</summary>
    SaveFile,

    /// <summary>A folder.</summary>
    Folder,
}

/// <summary>Marks a property as a path, so the grid offers a box with a browse button beside it.</summary>
/// <remarks>
/// <para>
/// The grid does not open the dialog itself. Which dialog to open, where it starts and how it is
/// filtered are the application's business, and in WinUI 3 a picker needs the window handle, which a
/// control has no dependable way to reach. Pressing the button raises
/// <see cref="PropertyGrid.BrowseRequested"/> instead, carrying what this attribute says.
/// </para>
/// <para>
/// A property of type <see cref="System.IO.FileInfo"/> or <see cref="System.IO.DirectoryInfo"/> gets
/// the same editor without the attribute — those types can only mean one thing.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// [FilePath(FilePathKind.OpenFile, ".gpkg", ".las")]
/// public string SourceFile { get; set; } = string.Empty;
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class FilePathAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="FilePathAttribute"/> class.</summary>
    /// <param name="kind">What the path is to, and what will be done with it.</param>
    /// <param name="extensions">
    /// The file extensions the property accepts, each with its leading dot. Leave empty for any file,
    /// which is also what a folder means.
    /// </param>
    public FilePathAttribute(FilePathKind kind = FilePathKind.OpenFile, params string[] extensions)
    {
        Kind = kind;
        Extensions = extensions ?? [];
    }

    /// <summary>Gets what the path is to, and what will be done with it.</summary>
    public FilePathKind Kind { get; }

    /// <summary>Gets the file extensions the property accepts, each with its leading dot.</summary>
    public IReadOnlyList<string> Extensions { get; }
}
