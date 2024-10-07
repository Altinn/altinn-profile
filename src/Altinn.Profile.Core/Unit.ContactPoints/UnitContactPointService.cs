using Altinn.Profile.Core.Integrations;

namespace Altinn.Profile.Core.Unit.ContactPoints
{
    /// <summary>
    /// Implementation of the <see cref="IUnitContactPointsService"/> interface using an <see cref="IUnitProfileRepository"/> retrieve profile data "/>
    /// </summary>
    public class UnitContactPointService : IUnitContactPointsService
    {
        private readonly IUnitProfileRepository _unitRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitContactPointService"/> class.
        /// </summary>
        public UnitContactPointService(IUnitProfileRepository unitRepository)
        {
            _unitRepository = unitRepository;
        }

        /// <inheritdoc/>
        public async Task<Result<UnitContactPointsList, bool>> GetUserRegisteredContactPoints(UnitContactPointLookup lookup) => await _unitRepository.GetUserRegisteredContactPoints(lookup);
    }
}
