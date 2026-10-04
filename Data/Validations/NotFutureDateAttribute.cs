using System.ComponentModel.DataAnnotations;

namespace CustomerManagementPractiseCS.Data.Validations
{
    // Stops a date from being in the future
    public class NotFutureDateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            // If the value is a date AND it is later than today, it is wrong
            if (value is DateOnly date && date > DateOnly.FromDateTime(DateTime.Today))
            {
                return new ValidationResult(ErrorMessage ?? "Date cannot be in the future.");
            }

            // Anything else is fine
            return ValidationResult.Success;
        }
    }
}