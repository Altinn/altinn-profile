using System.ComponentModel.DataAnnotations;

using Altinn.Profile.Validators;

namespace Altinn.Profile.Models.OrganizationNotificationAddresses
{
    /// <summary>
    /// Represents a notification address
    /// </summary>
    public abstract class NotificationAddressModel
    {
        /// <summary>
        /// Country code for phone number.
        /// </summary>
        /// <example>+47</example>
        [CustomRegexForNotificationAddresses(ValidationRule.OrganizationCountryCode)]
        public string CountryCode { get; set; }

        /// <summary>
        /// Email address.
        /// </summary>
        /// <example>user@example.com</example>
        [CustomRegexForNotificationAddresses(ValidationRule.OrganizationEmail)]
        [MaxLength(254)]
        public string Email { get; set; }

        /// <summary>
        /// Phone number.
        /// </summary>
        /// <example>98765432</example>        
        [CustomRegexForNotificationAddresses(ValidationRule.OrganizationPhone)]
        [MaxLength(14)] // Ref. the E.164 standard
        public string Phone { get; set; }
    }
}
