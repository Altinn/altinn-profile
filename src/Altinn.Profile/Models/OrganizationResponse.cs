using System.Collections.Generic;

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
        public List<NotificationAddressResponse> NotificationAddresses { get; set; }
    }
}
