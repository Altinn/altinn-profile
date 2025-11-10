using System;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Represents a notification address
    /// </summary>
    public class DashboardNotificationAddressResponse
    {
        /// <summary>
        /// <see cref="NotificationAddressId"/>
        /// </summary>
        public int NotificationAddressId { get; set; }
        
        /// <summary>
        /// Country code for phone number
        /// </summary>        
        public string CountryCode { get; set; }

        /// <summary>
        /// Email address
        /// </summary>        
        public string Email { get; set; }

        /// <summary>
        /// Phone number
        /// </summary>        
        public string Phone { get; set; }

        /// <summary>
        /// Source organization number
        /// </summary>
        public string SourceOrgNumber { get; set; }

        /// <summary>
        /// Requested organization number
        /// </summary>        
        public string RequestedOrgNumber { get; set; }

        /// <summary>
        /// Last changed timestamp
        /// </summary>        
        public DateTime? LastChanged { get; set; }
    }
}
