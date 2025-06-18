#nullable enable

using System;
using System.Collections.Generic;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Data model for the personal notification address for an organization
    /// </summary>
    public class ProfessionalNotificationAddresses
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
        public string? EmailAddress { get; set; }

        /// <summary>
        /// The phone number. May be null if no phone number is set. 
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// A list of resources that the user has registered to receive notifications for. The format is in URN. This is used to determine which resources the user can receive notifications for.
        /// </summary>
        public List<string> ResourceIncludeList { get; set; } = [];
    }
}
