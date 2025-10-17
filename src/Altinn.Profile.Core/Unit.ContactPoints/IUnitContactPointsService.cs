namespace Altinn.Profile.Core.Unit.ContactPoints;

/// <summary>
/// Class describing the methods required for user contact point service
/// </summary>
public interface IUnitContactPointsService
{
    /// <summary>
    /// Method for retrieving user registered contact points for a unit
    /// </summary>
    /// <param name="lookup">A lookup object containing a list of organization numbers and the resource to lookup contact points for</param>
    /// <returns>The users' contact points and reservation status or a boolean if failure.</returns>
    Task<Result<UnitContactPointsList, bool>> GetUserRegisteredContactPoints(UnitContactPointLookup lookup);

    /// <summary>
    /// Method for retrieving user registered contact points for a unit
    /// </summary>
    Task<UnitContactPointsList> GetUserRegisteredContactPoints(string[] orgNumbers, string resourceId, CancellationToken cancellationToken);
}
