#nullable enable

using System;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Response model for the professional notification address for an organization, also called personal notification address.
    /// </summary>
    public class ProfessionalNotificationAddressResponse : ProfessionalNotificationAddress
    {
        /// <summary>
        /// The user id of logged-in user for whom the specific contact information belongs to.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Id of the party
        /// </summary>
        public Guid PartyUuid { get; set; }
    }
}
