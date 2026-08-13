namespace Digi21.WinUI.PropertyGrid;

/// <summary>Describes one property of a type in a <see cref="PropertyGridMetadata"/> store.</summary>
/// <remarks>
/// Every method sets one field and returns the builder, so a property is described in a single
/// chained expression. Fields left unmentioned keep whatever the attributes on the property said.
/// </remarks>
public sealed class PropertyMetadataBuilder
{
    private readonly PropertyGridMetadata metadata;
    private readonly PropertyMetadataOverride target;

    internal PropertyMetadataBuilder(PropertyGridMetadata metadata, PropertyMetadataOverride target)
    {
        this.metadata = metadata;
        this.target = target;
    }

    /// <summary>Sets the label shown for the property.</summary>
    /// <param name="displayName">The label.</param>
    /// <returns>This builder.</returns>
    public PropertyMetadataBuilder DisplayName(string displayName) => Set(() => target.DisplayName = displayName);

    /// <summary>Sets the sentence shown in the description pane when the property is selected.</summary>
    /// <param name="helpText">The sentence.</param>
    /// <returns>This builder.</returns>
    public PropertyMetadataBuilder Description(string helpText) => Set(() => target.HelpText = helpText);

    /// <summary>Puts the property in a category.</summary>
    /// <param name="categoryName">The name of the category.</param>
    /// <returns>This builder.</returns>
    public PropertyMetadataBuilder Category(string categoryName) => Set(() => target.CategoryName = categoryName);

    /// <summary>Places the property at a position, lowest first.</summary>
    /// <param name="order">The position.</param>
    /// <returns>This builder.</returns>
    public PropertyMetadataBuilder Order(int order) => Set(() => target.Order = order);

    /// <summary>Stops the grid from writing the property.</summary>
    /// <param name="isReadOnly">Whether the property is read-only. Defaults to <see langword="true"/>.</param>
    /// <returns>This builder.</returns>
    public PropertyMetadataBuilder ReadOnly(bool isReadOnly = true) => Set(() => target.IsReadOnly = isReadOnly);

    /// <summary>Shows or hides the property.</summary>
    /// <param name="isBrowsable">Whether the property is shown.</param>
    /// <returns>This builder.</returns>
    /// <remarks>
    /// Passing <see langword="true"/> brings back a property hidden by <c>[Browsable(false)]</c>,
    /// which is the point of having the store able to say so.
    /// </remarks>
    public PropertyMetadataBuilder Browsable(bool isBrowsable = true) => Set(() => target.IsBrowsable = isBrowsable);

    /// <summary>Says whether the property survives into the merged list when several objects are shown.</summary>
    /// <param name="isMergable">Whether the property can be merged.</param>
    /// <returns>This builder.</returns>
    public PropertyMetadataBuilder Mergable(bool isMergable = true) => Set(() => target.IsMergable = isMergable);

    /// <summary>Makes the property use a named editor instead of the one its type would resolve to.</summary>
    /// <param name="editorKey">The name of the editor.</param>
    /// <returns>This builder.</returns>
    public PropertyMetadataBuilder Editor(string editorKey) => Set(() => target.EditorKey = editorKey);

    /// <summary>Says whether the property can be opened into child rows.</summary>
    /// <param name="isExpandable">Whether the value can be opened. Defaults to <see langword="true"/>.</param>
    /// <returns>This builder.</returns>
    public PropertyMetadataBuilder Expandable(bool isExpandable = true) => Set(() => target.IsExpandable = isExpandable);

    /// <summary>Declares the value the property counts as untouched at, so the grid can mark it when it differs.</summary>
    /// <param name="defaultValue">The value.</param>
    /// <returns>This builder.</returns>
    public PropertyMetadataBuilder DefaultValue(object? defaultValue) => Set(() =>
    {
        target.HasDefaultValue = true;
        target.DefaultValue = defaultValue;
    });

    private PropertyMetadataBuilder Set(Action apply)
    {
        apply();
        metadata.Touch();
        return this;
    }
}
