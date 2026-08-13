namespace Digi21.WinUI.PropertyGrid;

/// <summary>Makes a property use a named editor instead of the one its type would resolve to.</summary>
/// <remarks>
/// The key names an entry of a <c>PropertyEditorTemplateMap</c>, so the property picks its editor
/// without the model having to reference any UI type. An <c>int</c> that means a percentage can ask
/// for a slider while every other <c>int</c> keeps the number box.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class PropertyEditorAttribute : Attribute
{
    /// <summary>Initializes a new instance of the <see cref="PropertyEditorAttribute"/> class.</summary>
    /// <param name="key">The name of the editor to use.</param>
    public PropertyEditorAttribute(string key) => Key = key;

    /// <summary>Gets the name of the editor to use.</summary>
    public string Key { get; }
}
