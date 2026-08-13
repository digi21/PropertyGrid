using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Digi21.WinUI.PropertyGrid;

/// <summary>One line of a <see cref="PropertyGrid"/>: either a category header or a property.</summary>
/// <remarks>
/// <para>
/// Rows are ordinary observable objects, not <see cref="Microsoft.UI.Xaml.DependencyObject"/>s.
/// They are data rather than elements — nothing styles them, animates them or reads an attached
/// property off them — and being plain objects is what lets them be built off the interface thread
/// and tested without a XAML runtime, which a <c>DependencyObject</c> cannot even be constructed
/// without.
/// </para>
/// <para>
/// Both category headers and properties live in the one flat list the grid renders, so that a single
/// virtualizing repeater covers the whole grid: nesting repeaters inside a vertical stack would
/// measure them with infinite height and realize every row at once.
/// </para>
/// </remarks>
public abstract class PropertyGridRow : INotifyPropertyChanged
{
    private bool isExpanded;

    private protected PropertyGridRow(PropertyGridSource source, string key, int depth)
    {
        Source = source;
        Key = key;
        Depth = depth;
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Gets a name that identifies the row within its grid, such as <c>Address.City</c>.</summary>
    /// <remarks>
    /// Keys survive a rebuild, which is what lets the grid put back what was expanded, what was
    /// selected and where the list was scrolled after the arrangement changes.
    /// </remarks>
    public string Key { get; }

    /// <summary>Gets how deeply the row is nested. Rows at the top of the grid are at zero.</summary>
    public int Depth { get; }

    /// <summary>Gets the label shown in the name column.</summary>
    public abstract string DisplayName { get; }

    /// <summary>Gets a value indicating whether the row can be opened to show rows underneath it.</summary>
    public abstract bool IsExpandable { get; }

    /// <summary>Gets or sets a value indicating whether the row is open.</summary>
    public bool IsExpanded
    {
        get => isExpanded;
        set
        {
            if (isExpanded == value || (value && !IsExpandable))
            {
                return;
            }

            isExpanded = value;
            RaisePropertyChanged();
            Source.OnExpansionChanged(this);
        }
    }

    internal PropertyGridSource Source { get; }

    // Opens or closes the row without asking the grid to rebuild its list, for the rebuild itself:
    // restoring what was open before must not schedule another rebuild for every row it restores.
    internal void SetExpandedQuietly(bool value)
    {
        if (isExpanded == value)
        {
            return;
        }

        isExpanded = value;
        RaisePropertyChanged(nameof(IsExpanded));
    }

    /// <summary>Announces that a property of the row changed.</summary>
    /// <param name="propertyName">The name of the property, filled in by the compiler.</param>
    protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
