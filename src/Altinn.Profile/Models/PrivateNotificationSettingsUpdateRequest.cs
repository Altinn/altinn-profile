using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using Altinn.Profile.Validators;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Data model for the private notification address for a self-identified user, used when updating the notification address.
    /// </summary>
    public class PrivateNotificationSettingsUpdateRequest : IValidatableObject
    {
        /// <summary>
        /// The phone number. May be null if no phone number is set. 
        /// </summary>
        /// <example>+4798765432</example>
        [CustomRegexForNotificationAddresses(ValidationRule.InternationalPhoneNumber)]
        public string Value { get; set; }

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Value != null && !PhoneNumberValidator.IsValidPhoneNumber(Value))
            {
                yield return new ValidationResult("Phone number is not valid.", [nameof(Value)]);
            }
        }
    }
}
