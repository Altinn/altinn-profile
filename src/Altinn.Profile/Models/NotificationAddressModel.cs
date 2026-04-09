using Altinn.Profile.Validators;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Represents a notification address
    /// </summary>
    public abstract class NotificationAddressModel
    {
        /// <summary>
        /// Country code for phone number
        /// </summary>
        [CustomRegexForNotificationAddresses(ValidationRule.CountryCode)]
        public string CountryCode { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        [CustomRegexForNotificationAddresses(ValidationRule.Email)]
        public string Email { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>
        [CustomRegexForNotificationAddresses(ValidationRule.Phone)]
        public string Phone { get; set; }
    }
}
