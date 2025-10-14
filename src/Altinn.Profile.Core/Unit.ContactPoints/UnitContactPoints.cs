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
        public List<UserRegisteredContactPoint> UserContactPoints { get; set; } = [];
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

    /// <summary>
    /// Class describing the contact points for a user
    /// </summary>
    public class UserRegisteredContactPoint
    {
        /// <summary>
        /// Gets or sets the mobile number
        /// </summary>
        public string? MobileNumber { get; set; }

        /// <summary>
        /// Gets or sets the email address
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// The user id of user for whom the specific contact information belongs to.
        /// </summary>
        public int UserId { get; set; }
    }
}
