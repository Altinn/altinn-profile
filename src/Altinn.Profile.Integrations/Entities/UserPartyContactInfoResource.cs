namespace Altinn.Profile.Integrations.Entities
{
    /// <summary>
    /// Data model for the personal notification address for an organization
    /// </summary>
    public class UserPartyContactInfoResource
    {
        /// <summary>
        /// Id of the association of resource to the user party contact info
        /// </summary>
        public long UserPartyContactInfoResourceId { get; set; }

        /// <summary>
        /// Foreign key to the user party contact info
        /// </summary>
        public long UserPartyContactInfoId { get; set; }

        /// <summary>
        /// Id of the resource that this contact info is associated with
        /// </summary>
        public required string ResourceId { get; set; }

        /// <summary>
        /// The contact info the resource is associated with
        /// </summary>
        public required virtual UserPartyContactInfo UserPartyContactInfo { get; set; }
    }
}
