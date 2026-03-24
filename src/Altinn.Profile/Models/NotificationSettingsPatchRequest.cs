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
        /// The email address. May be null if no email address is set.
        /// </summary>
        public Optional<string?> EmailAddress { get; set; } = new();

        /// <summary>
        /// The phone number. May be null if no phone number is set. 
        /// </summary>
        public Optional<string?> PhoneNumber { get; set; } = new();

        /// <summary>
        /// A list of resources that the user has registered to receive notifications for. The format is in URN. This is used to determine which resources the user can receive notifications for.
        /// </summary>
        public Optional<List<string>> ResourceIncludeList { get; set; } = new();

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            ValidationResult? emailValidationError = GetAddressValidationError(this, EmailAddress, "ProfessionalEmail", nameof(EmailAddress));
            if (emailValidationError is not null)
            {
                yield return emailValidationError;
            }

            ValidationResult? phoneValidationError = GetAddressValidationError(this, PhoneNumber, "ProfessionalPhone", nameof(PhoneNumber));
            if (phoneValidationError is not null)
            {
                yield return phoneValidationError;
            }

            bool hasResourceIncludeList = ResourceIncludeList.HasValue;
            List<string>? resourceIncludeList = ResourceIncludeList.Value;

            if (hasResourceIncludeList && resourceIncludeList?.Any(r => string.IsNullOrWhiteSpace(r) || !ResourceIdRegex().IsMatch(r)) == true)
            {
                yield return new ValidationResult("ResourceIncludeList must contain valid URN values of the format 'urn:altinn:resource:{resourceId}' where resourceId has 4 or more characters of lowercase letter, number, underscore or hyphen", [nameof(ResourceIncludeList)]);
            }

            if (hasResourceIncludeList && resourceIncludeList != null && resourceIncludeList.Count > resourceIncludeList.Distinct().Count())
            {
                yield return new ValidationResult("ResourceIncludeList cannot contain duplicates", [nameof(ResourceIncludeList)]);
            }

            bool hasNoEmail = !EmailAddress.HasValue || string.IsNullOrWhiteSpace(EmailAddress.Value);
            bool hasNoPhone = !PhoneNumber.HasValue || string.IsNullOrWhiteSpace(PhoneNumber.Value);
            if (hasNoEmail && hasNoPhone)
            {
                yield return new ValidationResult("The notification setting for a party must include either EmailAddress, PhoneNumber, or both.", [nameof(EmailAddress), nameof(PhoneNumber)]);
            }

            static ValidationResult? GetAddressValidationError(
                NotificationSettingsPatchRequest request,
                Optional<string?> value,
                string addressType,
                string memberName)
            {
                if (!value.HasValue)
                {
                    return null;
                }

                ValidationResult? validationError = new CustomRegexForNotificationAddressesAttribute(addressType)
                    .GetValidationResult(value.Value, new ValidationContext(request) { MemberName = memberName });

                return validationError is null || validationError == ValidationResult.Success ? null : validationError;
            }
        }

        [GeneratedRegex(_resourceIdRegex)]
        private static partial Regex ResourceIdRegex();
    }
}
