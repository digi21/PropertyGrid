namespace Digi21.WinUI.PropertyGrid;

/// <summary>Checks a value before a <see cref="PropertyGrid"/> writes it to the object.</summary>
/// <remarks>
/// This is for rules that live outside the model — a value that has to agree with another control,
/// a limit read from configuration. Rules the model itself owns are better expressed as
/// <c>DataAnnotations</c> attributes, or by the setter refusing the value, both of which the grid
/// already reports.
/// </remarks>
public interface IPropertyValidator
{
    /// <summary>Checks a value.</summary>
    /// <param name="row">The row being edited.</param>
    /// <param name="proposedValue">The value about to be written, already converted to the property's type.</param>
    /// <returns>The verdict.</returns>
    PropertyValidationResult Validate(PropertyGridPropertyRow row, object? proposedValue);
}
