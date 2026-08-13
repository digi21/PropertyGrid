using System.ComponentModel.DataAnnotations;

namespace Digi21.WinUI.PropertyGrid;

// Runs the validation layers that get a say before a value is written.
//
// The layer that runs after - INotifyDataErrorInfo on the object itself - is not here, because by
// then the value is already stored and the row simply mirrors what the object reports.
internal static class PropertyValidationRunner
{
    internal static IReadOnlyList<string> Validate(PropertyGridPropertyRow row, object? proposedValue)
    {
        PropertyGridValidationMode mode = row.Source.ValidationMode;
        List<string> rejections = [];

        if (mode.HasFlag(PropertyGridValidationMode.DataAnnotations))
        {
            AddDataAnnotationFailures(row, proposedValue, rejections);
        }

        if (mode.HasFlag(PropertyGridValidationMode.Validators))
        {
            foreach (IPropertyValidator validator in row.Source.Validators)
            {
                PropertyValidationResult verdict = validator.Validate(row, proposedValue);
                if (!verdict.IsValid)
                {
                    rejections.Add(verdict.ErrorMessage ?? string.Empty);
                }
            }
        }

        return rejections;
    }

    private static void AddDataAnnotationFailures(PropertyGridPropertyRow row, object? proposedValue, List<string> rejections)
    {
        List<ValidationAttribute> rules = [];
        foreach (Attribute attribute in row.Description.Attributes)
        {
            if (attribute is ValidationAttribute rule)
            {
                rules.Add(rule);
            }
        }

        if (rules.Count == 0)
        {
            return;
        }

        // The context carries the object and the member name so that a rule can look at its
        // siblings, which is what makes [Compare] and custom cross-field rules work.
        ValidationContext context = new(row.Target)
        {
            MemberName = row.Name,
            DisplayName = row.DisplayName,
        };

        List<ValidationResult> failures = [];
        if (Validator.TryValidateValue(proposedValue!, context, failures, rules))
        {
            return;
        }

        foreach (ValidationResult failure in failures)
        {
            rejections.Add(failure.ErrorMessage ?? string.Empty);
        }
    }
}
