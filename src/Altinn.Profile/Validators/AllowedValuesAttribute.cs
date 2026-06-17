using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Altinn.Profile.Validators
{
    /// <summary>
    /// Validation attribute that restricts a property to a set of allowed string values.
    /// </summary>
    /// <param name="allowedValues">The array of allowed string values.</param>
    public class AllowedValuesAttribute(params string[] allowedValues) : ValidationAttribute
    {
        private readonly string[] _allowedValues = allowedValues;

        /// <summary>
        /// Validates whether the provided value is in the list of allowed values.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>A <see cref="ValidationResult"/> indicating success if the value is null or in the allowed list, otherwise a validation error.</returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            if (_allowedValues.Contains(value.ToString()))
            {
                return ValidationResult.Success;
            }

            return new ValidationResult($"Allowed values are: {string.Join(", ", _allowedValues)}");
        }
    }
}
