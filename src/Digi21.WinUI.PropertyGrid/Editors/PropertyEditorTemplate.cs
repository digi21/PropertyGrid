using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;

namespace Digi21.WinUI.PropertyGrid;

/// <summary>
/// One registered editor: the template that renders the value cell of a property, and what that
/// template is for.
/// </summary>
/// <remarks>
/// This is a <see cref="DependencyObject"/> only because markup has to be able to put a
/// <see cref="DataTemplate"/> on it. The matching itself is plain data.
/// </remarks>
/// <example>
/// <code language="xml">
/// &lt;pg:PropertyEditorTemplate TargetType="local:Rating"&gt;
///     &lt;DataTemplate&gt;
///         &lt;RatingControl MaxRating="5" Value="{Binding DoubleValue, Mode=TwoWay}" /&gt;
///     &lt;/DataTemplate&gt;
/// &lt;/pg:PropertyEditorTemplate&gt;
/// </code>
/// </example>
[ContentProperty(Name = nameof(ValueTemplate))]
public partial class PropertyEditorTemplate : DependencyObject
{
    /// <summary>Identifies the <see cref="TargetType"/> dependency property.</summary>
    public static readonly DependencyProperty TargetTypeProperty = DependencyProperty.Register(
        nameof(TargetType),
        typeof(Type),
        typeof(PropertyEditorTemplate),
        new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="Key"/> dependency property.</summary>
    public static readonly DependencyProperty KeyProperty = DependencyProperty.Register(
        nameof(Key),
        typeof(string),
        typeof(PropertyEditorTemplate),
        new PropertyMetadata(null));

    /// <summary>Identifies the <see cref="MatchDerivedTypes"/> dependency property.</summary>
    public static readonly DependencyProperty MatchDerivedTypesProperty = DependencyProperty.Register(
        nameof(MatchDerivedTypes),
        typeof(bool),
        typeof(PropertyEditorTemplate),
        new PropertyMetadata(false));

    /// <summary>Identifies the <see cref="ValueTemplate"/> dependency property.</summary>
    public static readonly DependencyProperty ValueTemplateProperty = DependencyProperty.Register(
        nameof(ValueTemplate),
        typeof(DataTemplate),
        typeof(PropertyEditorTemplate),
        new PropertyMetadata(null));

    /// <summary>Gets or sets the type of property this editor is for.</summary>
    public Type? TargetType
    {
        get => (Type?)GetValue(TargetTypeProperty);
        set => SetValue(TargetTypeProperty, value);
    }

    /// <summary>Gets or sets the name a property refers to this editor by with <c>[PropertyEditor]</c>.</summary>
    public string? Key
    {
        get => (string?)GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the editor also covers types derived from
    /// <see cref="TargetType"/>, or implementing it when it is an interface.
    /// </summary>
    /// <remarks>
    /// Off by default. An exact match is what most editors mean, and a base-type match firing by
    /// accident on some unrelated property is very hard to notice.
    /// </remarks>
    public bool MatchDerivedTypes
    {
        get => (bool)GetValue(MatchDerivedTypesProperty);
        set => SetValue(MatchDerivedTypesProperty, value);
    }

    /// <summary>
    /// Gets or sets the template that renders the value cell. Its data context is the
    /// <see cref="PropertyGridPropertyRow"/> being edited.
    /// </summary>
    /// <remarks>
    /// Named <c>ValueTemplate</c> rather than the more obvious <c>Template</c> on purpose. A
    /// dependency property called <c>Template</c> on a plain <see cref="DependencyObject"/> is
    /// silently dropped by the XAML compiler — the object is created, its other properties are set,
    /// and that one is left null with no warning and no error. Setting it from code works, which
    /// makes the difference maddening to track down. Presumably it collides with the well-known
    /// <c>Control.Template</c>.
    /// </remarks>
    public DataTemplate? ValueTemplate
    {
        get => (DataTemplate?)GetValue(ValueTemplateProperty);
        set => SetValue(ValueTemplateProperty, value);
    }

    internal EditorCriteria ToCriteria() => new(TargetType, Key, MatchDerivedTypes);
}

