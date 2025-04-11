using System.Collections.Generic;

namespace Altinn.Profile.Models
{    
    /// <summary>
     /// Represents a on organization with  notification addresses
     /// </summary>
    public class UpdateOrganizationRequest
    {
        /// <summary>
        /// Represents a list of mandatory notification address
        /// </summary>
        public List<NotificationAddressModel> NotificationAddresses { get; set; }
    }
}
