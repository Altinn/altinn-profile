namespace Altinn.Profile.Integrations.Entities
{
    /// <summary>
    /// Data model for the personal notification address for an organization
    /// </summary>
    public class UserPartyContactInfo
    {
        /// <summary>
        /// Id of the user party contact info
        /// </summary>
        public long UserPartyContactInfoId { get; set; }

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
        /// Date of last change
        /// </summary>
        public DateTime LastChanged { get; set; }

        /// <summary>
        /// Gets or sets notification options chosen for specific services by the user for the contact info
        /// </summary>
        public List<UserPartyContactInfoResource>? UserPartyContactInfoResources { get; set; }
    }
}
