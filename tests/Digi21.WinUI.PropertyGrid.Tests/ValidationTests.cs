using Xunit;

namespace Digi21.WinUI.PropertyGrid.Tests;

public class ValidationTests
{
    private sealed class EvenNumbersOnly : IPropertyValidator
    {
        public PropertyValidationResult Validate(PropertyGridPropertyRow row, object? proposedValue) =>
            proposedValue is int number && number % 2 != 0
                ? PropertyValidationResult.Error("Only even numbers, please.")
                : PropertyValidationResult.Success;
    }

    private static (PropertyGridSource Source, AnnotatedValidationSubject Subject) NewAnnotatedGrid()
    {
        AnnotatedValidationSubject subject = new();
        PropertyGridSource source = new();
        source.SetTarget(subject);
        return (source, subject);
    }

    [Fact]
    public void RejectsAValueOutsideTheDeclaredRange()
    {
        (PropertyGridSource source, AnnotatedValidationSubject subject) = NewAnnotatedGrid();
        PropertyGridPropertyRow row = source.FindRow("Rating")!;

        row.Value = 99;

        Assert.True(row.HasErrors);
        Assert.Equal(5, subject.Rating);
    }

    [Fact]
    public void AcceptsAValueInsideTheDeclaredRange()
    {
        (PropertyGridSource source, AnnotatedValidationSubject subject) = NewAnnotatedGrid();

        source.FindRow("Rating")!.Value = 8;

        Assert.Equal(8, subject.Rating);
    }

    [Fact]
    public void RejectsAStringOutsideTheDeclaredLength()
    {
        (PropertyGridSource source, AnnotatedValidationSubject subject) = NewAnnotatedGrid();

        source.FindRow("Code")!.Text = "far too long";

        Assert.Equal("abc", subject.Code);
    }

    [Fact]
    public void RejectsAnEmptyValueForARequiredProperty()
    {
        (PropertyGridSource source, AnnotatedValidationSubject subject) = NewAnnotatedGrid();
        PropertyGridPropertyRow row = source.FindRow("Code")!;

        row.Text = string.Empty;

        Assert.True(row.HasErrors);
        Assert.Equal("abc", subject.Code);
    }

    [Fact]
    public void RunsAValidatorItWasGiven()
    {
        (PropertyGridSource source, AnnotatedValidationSubject subject) = NewAnnotatedGrid();
        source.Validators.Add(new EvenNumbersOnly());
        PropertyGridPropertyRow row = source.FindRow("Rating")!;

        row.Value = 7;

        Assert.Equal("Only even numbers, please.", row.ErrorMessage);
        Assert.Equal(5, subject.Rating);
    }

    [Fact]
    public void LetsAValidatorAcceptAValue()
    {
        (PropertyGridSource source, AnnotatedValidationSubject subject) = NewAnnotatedGrid();
        source.Validators.Add(new EvenNumbersOnly());

        source.FindRow("Rating")!.Value = 6;

        Assert.Equal(6, subject.Rating);
    }

    [Fact]
    public void SkipsTheLayersItWasToldNotToRun()
    {
        (PropertyGridSource source, AnnotatedValidationSubject subject) = NewAnnotatedGrid();
        source.ValidationMode = PropertyGridValidationMode.None;

        source.FindRow("Rating")!.Value = 99;

        Assert.Equal(99, subject.Rating);
    }

    [Fact]
    public void ShowsWhatTheObjectItselfReportsAfterTheValueIsStored()
    {
        // The setter has already run and applied a rule the grid could not have seen from outside.
        ValidatingSubject subject = new();
        PropertyGridSource source = new();
        source.SetTarget(subject);
        PropertyGridPropertyRow row = source.FindRow("Identifier")!;

        row.Text = "lowercase";

        Assert.Equal("The identifier must be uppercase.", row.ErrorMessage);
        Assert.Equal("lowercase", subject.Identifier);
    }

    [Fact]
    public void ClearsTheReportedErrorWhenTheObjectStopsComplaining()
    {
        ValidatingSubject subject = new();
        PropertyGridSource source = new();
        source.SetTarget(subject);
        PropertyGridPropertyRow row = source.FindRow("Identifier")!;
        row.Text = "lowercase";

        row.Text = "UPPERCASE";

        Assert.False(row.HasErrors);
    }

    [Fact]
    public void IgnoresWhatTheObjectReportsWhenThatLayerIsTurnedOff()
    {
        ValidatingSubject subject = new();
        PropertyGridSource source = new()
        {
            ValidationMode = PropertyGridValidationMode.Validators | PropertyGridValidationMode.DataAnnotations,
        };
        source.SetTarget(subject);
        PropertyGridPropertyRow row = source.FindRow("Identifier")!;

        row.Text = "lowercase";

        Assert.False(row.HasErrors);
    }

    [Fact]
    public void ReportsASetterThatRefusedTheValue()
    {
        ThrowingSubject subject = new();
        PropertyGridSource source = new();
        source.SetTarget(subject);
        PropertyGridPropertyRow row = source.FindRow("Rejecting")!;

        row.Value = "anything";

        Assert.True(row.HasErrors);
        Assert.Contains("the setter said no", row.ErrorMessage!, StringComparison.Ordinal);
    }

    [Fact]
    public void ShowsAGetterThatThrewInsteadOfBringingTheGridDown()
    {
        ThrowingSubject subject = new();
        PropertyGridSource source = new();
        source.SetTarget(subject);

        Assert.Equal("the getter said no", source.FindRow("Broken")!.ErrorMessage);
        Assert.Equal("fine", source.FindRow("Fine")!.Value);
    }
}
