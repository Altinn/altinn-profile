using Altinn.Profile.Core.Unit.ContactPoints;

namespace Altinn.Profile.Core.Integrations
{
    /// <summary>
    /// Interface describing a client for the user profile service
    /// </summary>
    public interface IUnitProfileRepository
    {
        /// <summary>
        /// Provides a list of user registered contact points based on the lookup criteria
        /// </summary>
        /// <param name="lookup">Lookup object containing a list of organizations and a resource</param>
        /// <returns>A list of unit contact points</returns>
        Task<Result<UnitContactPointsList, bool>> GetUserRegisteredContactPoints(UnitContactPointLookup lookup);
    }
}
