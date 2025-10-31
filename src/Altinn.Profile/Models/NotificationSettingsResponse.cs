#nullable enable

using System;

namespace Altinn.Profile.Models
{
    /// <summary>
    /// Response model for the professional notification address for an organization, also called personal notification address.
    /// </summary>
    public class NotificationSettingsResponse : ProfessionalNotificationAddress
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
        /// An indication of whether the notification address needs confirmation from a user
        /// </summary>
        public bool NeedsConfirmation { get; set; }
    }
}
