using System.Threading;

using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.ProfessionalNotificationAddresses;

namespace Altinn.Profile.Core.Unit.ContactPoints
{
    /// <summary>
    /// Implementation of the <see cref="IUnitContactPointsService"/> interface using an <see cref="IUnitProfileRepository"/> retrieve profile data "/>
    /// </summary>
    public class UnitContactPointService : IUnitContactPointsService
    {
        private readonly IUnitProfileRepository _unitRepository;
        private readonly IProfessionalNotificationsRepository _professionalNotificationsRepository;
        private readonly IRegisterClient _registerClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitContactPointService"/> class.
        /// </summary>
        public UnitContactPointService(IUnitProfileRepository unitRepository, IProfessionalNotificationsRepository professionalNotificationsRepository, IRegisterClient registerClient)
        {
            _unitRepository = unitRepository;
            _professionalNotificationsRepository = professionalNotificationsRepository;
            _registerClient = registerClient;
        }

        /// <inheritdoc/>
        public async Task<Result<UnitContactPointsList, bool>> GetUserRegisteredContactPoints(UnitContactPointLookup lookup) => await _unitRepository.GetUserRegisteredContactPoints(lookup);

        /// <inheritdoc/>
        public async Task<UnitContactPointsList> GetUserRegisteredContactPoints(string[] orgNumbers, string resourceId, CancellationToken cancellationToken)
        {
            var partyList = await _registerClient.GetPartyUuids(orgNumbers, cancellationToken);

            if (partyList == null)
            {
                throw new InvalidOperationException("Something went wrong when getting partyUuids from register");
            }

            var result = new UnitContactPointsList
            {
                ContactPointsList = []
            };

            if (partyList.Count == 0)
            {
                return result;
            }

            foreach (var party in partyList)
            {
                var validContactPoints = await GetValidNotificationAddressesForParty(party.PartyUuid, resourceId, cancellationToken);
                if (!validContactPoints.Any())
                {
                    continue;
                }

                result.ContactPointsList.Add(new UnitContactPoints
                {
                    OrganizationNumber = party.OrganizationIdentifier,
                    PartyId = party.PartyId,
                    UserContactPoints = [.. validContactPoints.Select(c => new UserRegisteredContactPoint { Email = c.EmailAddress, MobileNumber = c.PhoneNumber, UserId = c.UserId })],
                });
            }

            return result;
        }

        private async Task<IEnumerable<UserPartyContactInfo>> GetValidNotificationAddressesForParty(Guid partyUuid, string resourceId, CancellationToken cancellationToken)
        {
            var contactPoints = await _professionalNotificationsRepository.GetAllNotificationAddressesForPartyAsync(partyUuid, cancellationToken);

            var validContactPoits = contactPoints.Where(c => IsValidEndpoint(c, resourceId));

            return validContactPoits;
        }

        private static bool IsValidEndpoint(UserPartyContactInfo contactInfo, string resourceId)
        {
            // This method is inverted compared to the one in SblBridge, and check for null/empty is moved to db query level
            if (contactInfo.UserPartyContactInfoResources == null || contactInfo.UserPartyContactInfoResources.Count == 0)
            {
                return true;
            }

            // Check if any of the resourceIds match the resourceId of the request
            if (contactInfo.UserPartyContactInfoResources.Any(x => x.ResourceId == resourceId))
            {
                return true;
            }

            return false;
        }
    }
}
