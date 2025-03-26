using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Validators;

namespace Altinn.Profile.Models
{    
    /// <summary>
     /// Represents a on organization with  notification addresses
     /// </summary>
    public class OrganizationResponse
    {
        /// <summary>
        /// The organizations organization number
        /// </summary>
        public string OrganizationNumber { get; set; }

        /// <summary>
        /// Represents a list of mandatory notification address
        /// </summary>
        public List<NotificationAddress> NotificationAddresses { get; set; }

        /// <summary>
        /// Represents a notification address
        /// </summary>
        public class NotificationAddress
        {
            /// <summary>
            /// <see cref="NotificationAddressID"/>
            /// </summary>
            public int NotificationAddressID { get; set; }

            /// <summary>
            /// Country code for phone number//TODO 
            /// </summary>
            [CustomRegexForNotificationAddresses("CountryCode", ErrorMessage = "AltinnII.SBL.UserProfile.Regex.CountryCode")]
            public string CountryCode { get; set; }

            /// <summary>
            /// Email address
            /// </summary>
            [CustomRegexForNotificationAddresses("Email", ErrorMessage = "AltinnII.SBL.UserProfile.Regex.Email")]
            public string Email { get; set; }

            /// <summary>
            /// Phone number
            /// </summary>
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
}
