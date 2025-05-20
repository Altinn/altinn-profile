using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Altinn.Profile.Validators;
using PhoneNumbers;

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
            if (string.IsNullOrWhiteSpace(Email) && string.IsNullOrWhiteSpace(Phone))
            {
                yield return new ValidationResult("Either Phone or Email must be specified.", [nameof(Phone), nameof(Email)]);
            }

            if (!string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Phone))
            {
                yield return new ValidationResult("Cannot provide both Phone and Email for the same notification address.", [nameof(Phone), nameof(Email)]);
            }

            if (string.IsNullOrWhiteSpace(Email) && !IsValidPhoneNumber())
            {
                yield return new ValidationResult("Phone number is not valid.", [nameof(Phone)]);
            }
        }

        /// <summary>
        /// This is extra validation for phone numbers that cannot be validated with regex.
        /// </summary>
        private bool IsValidPhoneNumber()
        {
            var phoneNumberUtil = PhoneNumberUtil.GetInstance();
           
            bool isValidNumber;

            try
            {
                PhoneNumber phoneNumber = phoneNumberUtil.Parse(CountryCode + Phone, "NO");
                isValidNumber = phoneNumberUtil.IsValidNumber(phoneNumber);
            }
            catch (NumberParseException)
            {
                isValidNumber = false;
            }

            if (CountryCode == "+47")
            {
                if (Phone.Length != 8)
                {
                    isValidNumber = false;
                }

                if (!Phone.StartsWith('9') && !Phone.StartsWith('4'))
                {
                    isValidNumber = false;
                }
            }

            return isValidNumber;
        }
    }
}
