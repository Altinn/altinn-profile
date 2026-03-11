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
    /// <param name="orgNumbers">Array of organization numbers to lookup contact points for</param>
    /// <param name="resourceId">The resource ID to filter the contact points by</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A list of contact points for the specified organizations and resource</returns>
    Task<UnitContactPointsList> GetUserRegisteredContactPoints(string[] orgNumbers, string resourceId, CancellationToken cancellationToken);
}
