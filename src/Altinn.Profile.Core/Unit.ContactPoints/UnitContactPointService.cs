using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.User.ContactPoints;

namespace Altinn.Profile.Core.Unit.ContactPoints
{
    /// <summary>
    /// Implementation of the <see cref="IUnitContactPointsService"/> interface using a REST client to retrieve profile data "/>
    /// </summary>
    public class UnitContactPointService : IUnitContactPointsService
    {
        private readonly IUnitProfileClient _unitClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnitContactPointService"/> class.
        /// </summary>
        public UnitContactPointService(IUnitProfileClient unitClient)
        {
            _unitClient = unitClient;
        }

        /// <inheritdoc/>
        public async Task<Result<UnitContactPointsList, bool>> GetUserRegisteredContactPoints(UnitContactPointLookup lookup) => await _unitClient.GetUserRegisteredContactPoints(lookup);
    }
}
