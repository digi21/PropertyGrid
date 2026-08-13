namespace Digi21.WinUI.PropertyGrid;

/// <summary>What an <see cref="IPropertyValidator"/> made of a value.</summary>
/// <param name="IsValid">Whether the value is acceptable.</param>
/// <param name="ErrorMessage">What to tell the user when it is not.</param>
public readonly record struct PropertyValidationResult(bool IsValid, string? ErrorMessage)
{
    /// <summary>Gets a result accepting the value.</summary>
    public static PropertyValidationResult Success { get; } = new(true, null);

    /// <summary>Rejects a value.</summary>
    /// <param name="errorMessage">What to tell the user.</param>
    /// <returns>A result rejecting the value.</returns>
    public static PropertyValidationResult Error(string errorMessage) => new(false, errorMessage);
}
