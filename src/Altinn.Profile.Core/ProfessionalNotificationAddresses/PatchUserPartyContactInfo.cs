using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Altinn.Profile.Core.Utils;

namespace Altinn.Profile.Core.ProfessionalNotificationAddresses
{
    /// <summary>
    /// Data model for the personal notification address for an organization
    /// </summary>
    public class PatchUserPartyContactInfo
    {
        /// <summary>
        /// The user id of logged-in user for whom the specific contact information belongs to.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Id of the party
        /// </summary>
        public Guid PartyUuid { get; set; }

        /// <summary>
        /// The email address. May be null if no email address is set.
        /// </summary>
        public Optional<string?> EmailAddress { get; set; }

        /// <summary>
        /// The phone number. May be null if no phone number is set. 
        /// </summary>
        public Optional<string?> PhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets notification options chosen for specific services by the user for the contact info
        /// </summary>
        public Optional<List<UserPartyContactInfoResource>> UserPartyContactInfoResources { get; set; }
    }
}
