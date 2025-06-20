#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Altinn.Profile.Validators;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Data model for the professional notification address for an organization, also called personal notification address.
    /// </summary>
    public abstract class ProfessionalNotificationAddress :IValidatableObject
    {
        /// <summary>
        /// The email address. May be null if no email address is set.
        /// </summary>
        [CustomRegexForNotificationAddresses("ProfessionalEmail")]
        public string? EmailAddress { get; set; }

        /// <summary>
        /// The phone number. May be null if no phone number is set. 
        /// </summary>
        [CustomRegexForNotificationAddresses("ProfessionalPhone")]
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// A list of resources that the user has registered to receive notifications for. The format is in URN. This is used to determine which resources the user can receive notifications for.
        /// </summary>
        public List<string> ResourceIncludeList { get; set; } = [];

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ResourceIncludeList.Any(r => !r.StartsWith("urn:altinn:resource")))
            {
                yield return new ValidationResult("ResourceIncludeList must contain valid URN values starting with 'urn:altinn:resource'", [nameof(ResourceIncludeList)]);
            }
        }
    }
}
