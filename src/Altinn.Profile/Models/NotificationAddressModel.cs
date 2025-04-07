using System.ComponentModel.DataAnnotations;
using Altinn.Profile.Validators;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Represents a notification address
    /// </summary>
    public class NotificationAddressModel
    {
        /// <summary>
        /// <see cref="NotificationAddressID"/>
        /// </summary>
        public int NotificationAddressID { get; set; }

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

        /// <summary>
        /// A value indicating whether the entity is deleted in Altinn.
        /// </summary>
        public bool? IsDeleted { get; set; }

        /// <summary>
        /// Id from the registry
        /// </summary>
        [Required]
        [StringLength(32)]
        public string RegistryID { get; set; }
    }
}
