namespace Digi21.WinUI.PropertyGrid;

/// <summary>Which layers of validation a <see cref="PropertyGrid"/> runs when a value is edited.</summary>
[Flags]
public enum PropertyGridValidationMode
{
    /// <summary>Nothing beyond turning the typed text into a value, which can never be skipped.</summary>
    None = 0,

    /// <summary>The validators the grid was given, and the cancellable value-changing event.</summary>
    Validators = 1,

    /// <summary>
    /// The <c>System.ComponentModel.DataAnnotations</c> attributes on the property — <c>[Range]</c>,
    /// <c>[Required]</c>, <c>[StringLength]</c>, <c>[RegularExpression]</c> — checked before the
    /// value reaches the object.
    /// </summary>
    DataAnnotations = 2,

    /// <summary>
    /// The errors the object reports through <see cref="System.ComponentModel.INotifyDataErrorInfo"/>
    /// after the value has been written, which is what <c>ObservableValidator</c> implements.
    /// </summary>
    DataErrorInfo = 4,

    /// <summary>Every layer.</summary>
    All = Validators | DataAnnotations | DataErrorInfo,
}
