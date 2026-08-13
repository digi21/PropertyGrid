using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

namespace Digi21.WinUI.PropertyGrid;

/// <summary>A set of editors, declared in markup and handed to a grid, or to the whole application.</summary>
/// <remarks>
/// <para>
/// Registering an editor here replaces the value cell for the types it names. To replace a whole
/// category of editors instead — every boolean, every enumeration — declare a
/// <c>DataTemplate</c> under the matching <see cref="PropertyEditorKeys"/> name in the application's
/// resources, and the grid picks it up without any registration at all.
/// </para>
/// <para>
/// A grid searches its own map first and <see cref="Default"/> second, and within a map the last
/// entry registered wins, so an application can always override what a library set up.
/// </para>
/// </remarks>
/// <example>
/// <code language="xml">
/// &lt;pg:PropertyGrid SelectedObject="{x:Bind Layer}"&gt;
///     &lt;pg:PropertyGrid.EditorTemplates&gt;
///         &lt;pg:PropertyEditorTemplateMap&gt;
///             &lt;pg:PropertyEditorTemplate TargetType="local:Rating"&gt;
///                 &lt;DataTemplate&gt;
///                     &lt;RatingControl MaxRating="5" Value="{Binding DoubleValue, Mode=TwoWay}" /&gt;
///                 &lt;/DataTemplate&gt;
///             &lt;/pg:PropertyEditorTemplate&gt;
///             &lt;pg:PropertyEditorTemplate Key="Percent"&gt;
///                 &lt;DataTemplate&gt;
///                     &lt;Slider Maximum="100" Minimum="0" Value="{Binding DoubleValue, Mode=TwoWay}" /&gt;
///                 &lt;/DataTemplate&gt;
///             &lt;/pg:PropertyEditorTemplate&gt;
///         &lt;/pg:PropertyEditorTemplateMap&gt;
///     &lt;/pg:PropertyGrid.EditorTemplates&gt;
/// &lt;/pg:PropertyGrid&gt;
/// </code>
/// </example>
[ContentProperty(Name = nameof(Entries))]
public partial class PropertyEditorTemplateMap : DependencyObject
{
    private readonly List<EditorCriteria> criteria = [];

    /// <summary>Gets the map every grid falls back to, shared by the whole application.</summary>
    public static PropertyEditorTemplateMap Default { get; } = new();

    /// <summary>Gets the editors, in the order they were registered.</summary>
    public IList<PropertyEditorTemplate> Entries { get; } = [];

    /// <summary>Registers an editor.</summary>
    /// <param name="editor">The editor to register.</param>
    /// <returns>This map, so registrations can be chained.</returns>
    public PropertyEditorTemplateMap Add(PropertyEditorTemplate editor)
    {
        ArgumentNullException.ThrowIfNull(editor);

        Entries.Add(editor);
        return this;
    }

    /// <summary>Finds the editor to use for a property.</summary>
    /// <param name="declaredType">The declared type of the property.</param>
    /// <param name="runtimeType">The type of the value it currently holds, if any.</param>
    /// <param name="explicitKey">The name the property asked for, if it asked for one.</param>
    /// <returns>The template to render the value cell with, or <see langword="null"/> if this map has nothing to say.</returns>
    public DataTemplate? Resolve(Type declaredType, Type? runtimeType, string? explicitKey)
    {
        ArgumentNullException.ThrowIfNull(declaredType);

        // Rebuilt on every lookup rather than kept in step with the list: markup fills the list once
        // at parse time, lookups are memoized by the selector, and a snapshot that can go stale is
        // a worse trade than rebuilding a handful of structs.
        criteria.Clear();
        foreach (PropertyEditorTemplate entry in Entries)
        {
            criteria.Add(entry.ToCriteria());
        }

        int match = PropertyEditorMatching.Resolve(criteria, declaredType, runtimeType, explicitKey);
        return match == PropertyEditorMatching.NoMatch ? null : Entries[match].ValueTemplate;
    }
}

