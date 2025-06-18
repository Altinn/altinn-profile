#nullable enable

using System;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Data model for the personal notification address for an organization
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
