namespace Altinn.Profile.Integrations.Entities
{
    /// <summary>
    /// An association between a party and a group
    /// </summary>
    public class PartyGroupAssociation
    {
        /// <summary>
        /// The id of the association
        /// </summary>
        public int AssociationId { get; set; }

        /// <summary>
        /// The group id
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// The Party id
        /// </summary>
        public int PartyId { get; set; }

        /// <summary>
        /// The datetime when the association was created
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// The group the party is associated with
        /// </summary>
        public virtual Group Group { get; set; }
    }
}
