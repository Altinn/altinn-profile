using Altinn.Profile.Core.Unit.ContactPoints;

namespace Altinn.Profile.Integrations.SblBridge.Unit.Profile
{
    /// <summary>
    /// Model describing a container for a list of contact points.
    /// </summary>
    public class PartyNotificationContactPoints
    {
        /// <summary>
        /// Gets or sets the party id of the organization.
        /// </summary>
        public Guid? PartyId { get; set; }

        /// <summary>
        /// Gets or sets the legacy id of the organization.
        /// </summary>
        public int LegacyPartyId { get; set; }

        /// <summary>
        /// Gets or sets the organization number of the organization.
        /// </summary>
        public string OrganizationNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a list of multiple contact points associated with the organization.
        /// </summary>
        public List<SblUserRegisteredContactPoint> ContactPoints { get; set; } = [];

        /// <summary>
        /// Maps a list of <see cref="PartyNotificationContactPoints"/> to a list of <see cref="UnitContactPoints"/>.
        /// </summary>
        public static UnitContactPointsList MapToUnitContactPoints(List<PartyNotificationContactPoints> source)
        {
            List<UnitContactPoints> contactPoints = source.Select(partyNotificationEndpoint => new UnitContactPoints
            {
                OrganizationNumber = partyNotificationEndpoint.OrganizationNumber,
                PartyId = partyNotificationEndpoint.LegacyPartyId,
                UserContactPoints = partyNotificationEndpoint.ContactPoints.Select(contactPoint => new UserRegisteredContactPoint
                {
                    UserId = contactPoint.LegacyUserId,
                    Email = contactPoint.Email,
                    MobileNumber = contactPoint.MobileNumber
                }).ToList()
            }).ToList();

            return new UnitContactPointsList() { ContactPointsList = contactPoints };
        }
    }
}
