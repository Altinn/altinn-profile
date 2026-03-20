#nullable enable

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

using Altinn.Profile.Core.Utils;
using Altinn.Profile.Validators;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Data model for the professional notification address for an organization, also called personal notification address.
    /// </summary>
    public partial class NotificationSettingsPatchRequest : IValidatableObject
    {
        private const string _resourceIdRegex = "^urn:altinn:resource:[a-z0-9_-]{4,}$";

        /// <summary>
        /// A feature flag to indicate whether a verification code should be generated and sent to the provided email address or phone number. This is used to verify that the user has access to the provided contact information before it is saved as a notification address. If set to true, a verification code will be generated and sent, and the user will need to verify the code before the notification address is considered valid.
        /// </summary>
        public bool? GenerateVerificationCode { get; init; } = false;

        /// <summary>
        /// The email address. May be null if no email address is set.
        /// </summary>
        [CustomRegexForNotificationAddresses("ProfessionalEmail")]
        public Optional<string?> EmailAddress { get; set; }

        /// <summary>
        /// The phone number. May be null if no phone number is set. 
        /// </summary>
        [CustomRegexForNotificationAddresses("ProfessionalPhone")]
        public Optional<string?> PhoneNumber { get; set; }

        /// <summary>
        /// A list of resources that the user has registered to receive notifications for. The format is in URN. This is used to determine which resources the user can receive notifications for.
        /// </summary>
        public Optional<List<string>> ResourceIncludeList { get; set; }

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ResourceIncludeList.HasValue && ResourceIncludeList.Value?.Any(r => string.IsNullOrWhiteSpace(r) || !ResourceIdRegex().IsMatch(r)) == true)
            {
                yield return new ValidationResult("ResourceIncludeList must contain valid URN values of the format 'urn:altinn:resource:{resourceId}' where resourceId has 4 or more characters of lowercase letter, number, underscore or hyphen", [nameof(ResourceIncludeList)]);
            }

            if (ResourceIncludeList.HasValue && ResourceIncludeList.Value != null && ResourceIncludeList.Value.Count > ResourceIncludeList.Value.Distinct().Count())
            {
                yield return new ValidationResult("ResourceIncludeList cannot contain duplicates", [nameof(ResourceIncludeList)]);
            }

            if ((!EmailAddress.HasValue || string.IsNullOrWhiteSpace(EmailAddress.Value)) && (!PhoneNumber.HasValue || string.IsNullOrWhiteSpace(PhoneNumber.Value)))
            {
                yield return new ValidationResult("The notification setting for a party must include either EmailAddress, PhoneNumber, or both.", [nameof(EmailAddress), nameof(PhoneNumber)]);
            }
        }

        [GeneratedRegex(_resourceIdRegex)]
        private static partial Regex ResourceIdRegex();
    }
}
