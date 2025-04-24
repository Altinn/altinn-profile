using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Altinn.Profile.Validators;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Represents a notification address
    /// </summary>
    public class NotificationAddressModel : IValidatableObject
    {
        /// <summary>
        /// Country code for phone number
        /// </summary>
        [CustomRegexForNotificationAddresses("CountryCode")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        [CustomRegexForNotificationAddresses("Email")]
        public string Email { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        [CustomRegexForNotificationAddresses("Phone")]
        public string Phone { get; set; }

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Email == null && Phone == null)
            {
               yield return new ValidationResult("Either Phone or Email must be specified.", [nameof(Phone), nameof(Email)]);
            }

            if (Email != null && Phone != null)
            {
                yield return new ValidationResult("Cannot provide both Phone and Email for the same notification address.", [nameof(Phone), nameof(Email)]);
            }
        }
    }
}
