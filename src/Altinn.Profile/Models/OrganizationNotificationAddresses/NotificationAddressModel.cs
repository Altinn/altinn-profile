using Altinn.Profile.Validators;

namespace Altinn.Profile.Models.OrganizationNotificationAddresses
{
    /// <summary>
    /// Represents a notification address
    /// </summary>
    public abstract class NotificationAddressModel
    {
        /// <summary>
        /// Country code for phone number
        /// </summary>
        [CustomRegexForNotificationAddresses(ValidationRule.OrganizationCountryCode)]
        public string CountryCode { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        [CustomRegexForNotificationAddresses(ValidationRule.OrganizationEmail)]
        public string Email { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        [CustomRegexForNotificationAddresses(ValidationRule.OrganizationPhone)]
        public string Phone { get; set; }
    }
}
