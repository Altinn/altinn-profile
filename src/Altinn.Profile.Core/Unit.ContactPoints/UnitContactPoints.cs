using Altinn.Profile.Core.User.ContactPoints;

namespace Altinn.Profile.Core.Unit.ContactPoints
{
    /// <summary>
    /// Class describing the user registered contact points for a unit
    /// </summary>
    public class UnitContactPoints
    {
        /// <summary>
        /// Gets or sets the organization number of the organization.
        /// </summary>
        public string OrganizationNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the party id of the organization.
        /// </summary>
        public int PartyId { get; set; }

        /// <summary>
        /// Gets or sets a list of multiple contanct points associated with the organisation.
        /// </summary>
        public List<UserContactPoints> UserContactPoints { get; set; } = [];
    }

    /// <summary>
    /// A list representation of <see cref="UnitContactPoints"/>
    /// </summary>
    public class UnitContactPointsList
    {
        /// <summary>
        /// A list containing contact points for users
        /// </summary>
        public List<UnitContactPoints> ContactPointsList { get; set; } = [];
    }
}
