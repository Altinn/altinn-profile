using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PhoneNumbers;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Represents a notification address
    /// </summary>
    public class NotificationAddressRequest : NotificationAddressModel, IValidatableObject
    {
        /// <inheritdoc/>
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            bool hasEmailValue = !string.IsNullOrWhiteSpace(Email);
            bool hasPhoneValue = !string.IsNullOrWhiteSpace(Phone);
            bool hasCountryCodeValue = !string.IsNullOrWhiteSpace(CountryCode);

            if (!hasEmailValue && !hasPhoneValue)
            {
                yield return new ValidationResult("Either Phone or Email must be specified.", [nameof(Phone), nameof(Email)]);
            }
            else if (hasEmailValue && hasPhoneValue)
            {
                yield return new ValidationResult("Cannot provide both Phone and Email for the same notification address.", [nameof(Phone), nameof(Email)]);
            }
            else if (hasPhoneValue)
            {
                if (!hasCountryCodeValue)
                {
                    yield return new ValidationResult("CountryCode is required with Phone.", [nameof(CountryCode)]);
                }
                else if (!IsValidPhoneNumber())
                {
                    yield return new ValidationResult("Phone number is not valid.", [nameof(Phone)]);
                }
            }
            else
            {
                if (hasCountryCodeValue)
                {
                    yield return new ValidationResult("CountryCode cannot be provided with Email.", [nameof(CountryCode)]);
                } 
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
