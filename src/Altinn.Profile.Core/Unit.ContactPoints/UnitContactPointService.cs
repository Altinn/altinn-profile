using Altinn.Profile.Core.Integrations;

namespace Altinn.Profile.Core.Unit.ContactPoints
{
    /// <summary>
    /// Implementation of the <see cref="IUnitContactPointsService"/> interface using an <see cref="IUnitProfileRepository"/> retrieve profile data "/>
    /// </summary>
    public class UnitContactPointService : IUnitContactPointsService
    {
        private readonly IUnitProfileRepository _unitRepository;
        private readonly IProfessionalNotificationsRepository _professionalNotificationsRepository;
        private readonly IRegisterClient _registerClient

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
        public async Task<UnitContactPointsList> GetUserRegisteredContactPoints(string[] orgNumbers, string resourceId)
        {
            var partyUuids = await _registerClient.GetPartyUuids(orgNumbers, CancellationToken.None);

            var result = new UnitContactPointsList
            {
                ContactPointsList = new List<UnitContactPoints>()
            };
            foreach (var orgNumber in orgNumbers)
            {
                if (partyUuids != null && partyUuids.TryGetValue(orgNumber, out Guid partyUuid))
                {
                    var contactPoints = await _professionalNotificationsRepository.GetProfessionalNotificationContactPoints(partyUuid, resourceId);
                    result.ContactPointsList.Add(new UnitContactPoints
                    {
                        OrganizationNumber = orgNumber,
                        UserContactPoints = new List<User.ContactPoints.UserContactPoints>(),
                    });
                }
            }

            return result;
        }
    }
}
