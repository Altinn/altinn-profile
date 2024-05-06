using Altinn.Profile.Core.Unit.ContactPoints;
using Altinn.Profile.Core.User.ContactPoints;

namespace Altinn.Profile.Integrations.SblBridge
{
    /// <summary>
    /// Model describing a container for a list of contact points.
    /// </summary>
    public class PartyNotificationContactPoints
    {
        /// <summary>
        /// Gets or sets the party id of the organisation.
        /// </summary>
        public Guid? PartyId { get; set; }

        /// <summary>
        /// Gets or sets the legacy id of the organisation.
        /// </summary>
        public int LegacyPartyId { get; set; }

        /// <summary>
        /// Gets or sets the organization number of the organisation.
        /// </summary>
        public string OrganizationNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a list of multiple contanct points associated with the organisation.
        /// </summary>
        public List<UserRegisteredContactPoint> ContactPoints { get; set; } = new List<UserRegisteredContactPoint>();

        /// <summary>
        /// Maps a list of <see cref="PartyNotificationContactPoints"/> to a list of <see cref="UnitContactPoints"/>.
        /// </summary>
        public static UnitContactPointsList MapToUnitContactPoints(List<PartyNotificationContactPoints> source)
        {
            var contactPoints = source.Select(partyNotificationEndpoint => new UnitContactPoints
            {
                OrganizationNumber = partyNotificationEndpoint.OrganizationNumber,
                PartyId = partyNotificationEndpoint.LegacyPartyId,
                UserContactPoints = partyNotificationEndpoint.ContactPoints.Select(contactPoint => new UserContactPoints
                {
                    UserId = contactPoint.LegacyUserId,
                    Email = contactPoint.Email,
                    MobileNumber = contactPoint.MobileNumber
                }).ToList()
            }).ToList();

            return new UnitContactPointsList() { ContactPointsList = contactPoints };
        }
    }

    /// <summary>
    /// Model describing the contact information that a user has associated with a party they can represent.
    /// </summary>
    public class UserRegisteredContactPoint
    {
        /// <summary>
        /// Gets or sets the legacy user id for the owner of this party notification endpoint.
        /// </summary>
        /// <remarks>
        /// This was named as legacy for consistency. Property for UUID will probably never be added.
        /// </remarks>
        public int LegacyUserId { get; set; }

        /// <summary>
        /// Gets or sets the email address for this contact point.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the mobile number for this contact point.
        /// </summary>
        public string MobileNumber { get; set; } = string.Empty;
    }
}
