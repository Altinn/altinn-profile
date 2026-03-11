using Altinn.Profile.Core.Unit.ContactPoints;

namespace Altinn.Profile.Core.Integrations;

/// <summary>
/// Interface for accessing user profile services related to unit contact points.
/// </summary>
public interface IUnitProfileRepository
{
    /// <summary>
    /// Retrieves a list of user-registered contact points based on the specified lookup criteria.
    /// </summary>
    /// <param name="lookup">An object containing a list of organization numbers and a resource ID to filter the contact points.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object with a <see cref="UnitContactPointsList"/> on success, or a boolean indicating failure.</returns>
    Task<Result<UnitContactPointsList, bool>> GetUserRegisteredContactPoints(UnitContactPointLookup lookup);
}
