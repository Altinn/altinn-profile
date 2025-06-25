#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using Altinn.Profile.Validators;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Data model for the professional notification address for an organization, also called personal notification address.
    /// </summary>
    public abstract partial class ProfessionalNotificationAddress :IValidatableObject
    {
        private const string _resourceIdRegex = "^urn:altinn:resource:[a-z0-9_-]{4,}$";

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
            if (ResourceIncludeList.Any(r => string.IsNullOrWhiteSpace(r) || !ResourceIdRegex().IsMatch(r)))
            {
                yield return new ValidationResult("ResourceIncludeList must contain valid URN values of the format 'urn:altinn:resource:{resourceId}' where resourceId has 4 or more characters of lowercase letter, number, underscore or hyphen", [nameof(ResourceIncludeList)]);
            }

            if (ResourceIncludeList.Count > ResourceIncludeList.Distinct().Count())
            {
                yield return new ValidationResult("ResourceIncludeList cannot contain duplicates", [nameof(ResourceIncludeList)]);
            }

            if (string.IsNullOrWhiteSpace(EmailAddress) && string.IsNullOrWhiteSpace(PhoneNumber))
            {
                yield return new ValidationResult("The notification setting for a party must include either EmailAddress, PhoneNumber, or both.", [nameof(EmailAddress), nameof(PhoneNumber)]);
            }
        }

        [GeneratedRegex(_resourceIdRegex)]
        private static partial Regex ResourceIdRegex();
    }
}
