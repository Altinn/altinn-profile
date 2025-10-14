namespace Altinn.Profile.Core.Unit.ContactPoints
{
    /// <summary>
    /// The party object
    /// </summary>
    public class Party
    {
        /// <summary>
        /// The party id.
        /// </summary>
        public int PartyId { get; init; }

        /// <summary>
        /// The party uuid.
        /// </summary>
        public Guid PartyUuid { get; init; }

        /// <summary>
        /// The organization identifier (org number).
        /// </summary>
        public required string OrganizationIdentifier { get; init; }
    }
}
