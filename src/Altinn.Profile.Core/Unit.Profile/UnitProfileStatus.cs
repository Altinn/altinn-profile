namespace Altinn.Profile.Core.Unit.ContactPoints
{
    /// <summary>
    /// Class describing the unit profile for an organization. This describes maintainance of the notification settings for an organization.
    /// </summary>
    public class UnitProfileStatus
    {
        /// <summary>
        /// Gets or sets the party id of the organization.
        /// </summary>
        public int PartyId { get; set; }

        /// <summary>
        /// Gets or sets the party uuid of the organization.
        /// </summary>
        public Guid PartyUuid { get; set; }

        /// <summary>
        /// Gets or sets the user id that last modified the unit profile.
        /// </summary>
        public int? LastModifiedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the user uuid that last modified the unit profile.
        /// </summary>
        public Guid? LastModifiedByUserUuid { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the unit profile was last modified.
        /// </summary>
        public DateTime LastModifiedDate { get; set; }

        /// <summary>
        /// Gets or sets the user id that last confirmed the unit profile.
        /// </summary>
        public int? LastConfirmedByUserId { get; set; }

        /// <summary>
        /// Gets or sets the user uuid that last confirmed the unit profile.
        /// </summary>
        public Guid? LastConfirmedByUserUuid { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the unit profile was last confirmed.
        /// </summary>
        public DateTime? LastConfirmationDate { get; set; }
    }
}
