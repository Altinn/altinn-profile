using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Altinn.Profile.Validators;

using JasperFx.CodeGeneration;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Data model for the professional notification address for an organization, also called personal notification address.
    /// </summary>
    public class PrivateNotificationSettingsRequest : IValidatableObject
    {
        /// <summary>
        /// The phone number. May be null if no phone number is set. 
        /// </summary>
        [CustomRegexForNotificationAddresses(ValidationRule.InternationalPhoneNumber)]
        public string Value { get; set; }

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!PhoneNumberValidator.IsValidPhoneNumber(Value))
            {
                yield return new ValidationResult("Phone number is not valid.", [nameof(Value)]);
            }
        }
    }
}
