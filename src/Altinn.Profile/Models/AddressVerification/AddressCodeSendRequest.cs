using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

using Altinn.Profile.Core.AddressVerifications.Models;
using Altinn.Profile.Validators;

using PhoneNumbers;

namespace Altinn.Profile.Models.AddressVerification
{
    /// <summary>
    /// Represents a request to send a verification code for a given address (email or phone number)
    /// </summary>
    public class AddressCodeSendRequest : IValidatableObject
    {
        /// <summary>
        /// Gets or sets the address to verify, either an email or a phone number.
        /// </summary>
        [Required]
        [JsonRequired]
        [StringLength(320)] // max email length per RFC 5321
        [MinLength(5)] // No valid email or phone can be shorter than this
        public required string Value { get; init; }

        /// <summary>
        /// Gets or sets the type of the address, either "email" or "sms".
        /// </summary>
        [Required]
        [JsonRequired]
        public required AddressType? Type { get; init; }

        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Type == AddressType.Email)
            {
                ValidationResult validationError = new CustomRegexForNotificationAddressesAttribute(ValidationRule.EmailAddress).GetValidationResult(Value, new ValidationContext(this) { MemberName = nameof(Value) });
                if (validationError is not null)
                {
                    yield return validationError;
                }
            }
            else if (Type == AddressType.Sms)
            {
                ValidationResult phoneValidationError = new CustomRegexForNotificationAddressesAttribute(ValidationRule.InternationalPhoneNumber).GetValidationResult(Value, new ValidationContext(this) { MemberName = nameof(Value) });
                if (phoneValidationError is not null)
                {
                    yield return phoneValidationError;
                }

                if (!PhoneNumberValidator.IsValidPhoneNumber(Value))
                {
                    yield return new ValidationResult("Phone number is not valid.", [nameof(Value)]);
                }
            }
        }
    }
}
