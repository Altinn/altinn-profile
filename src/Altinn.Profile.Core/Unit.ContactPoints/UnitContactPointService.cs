using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.User.ContactPoints;

namespace Altinn.Profile.Core.Unit.ContactPoints
{
    /// <summary>
    /// Implementation of the <see cref="IUnitContactPoints"/> interface using a REST client to retrieve profile data "/>
    /// </summary>
    public class UnitContactPointService : IUnitContactPoints
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
        public async Task<Result<UnitContactPointsList, bool>> GetUserRegisteredContactPoints(UnitContactPointLookup lookup)
        {
            Result<UnitContactPointsList, bool> result = await _unitClient.GetUserRegisteredContactPoints(lookup);
            return result;
        }
    }
}
