namespace Altinn.Profile.Core.Unit.ContactPoints;

/// <summary>
/// Class describing the methods required for user contact point service
/// </summary>
public interface IUnitContactPointsService
{
    /// <summary>
    /// Method for retrieving user registered contact points for a unit
    /// </summary>
    /// <param name="orgNumbers">Array of organization numbers to lookup contact points for</param>
    /// <param name="resourceId">The resource ID to filter the contact points by</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A list of contact points for the specified organizations and resource</returns>
    Task<UnitContactPointsList> GetUserRegisteredContactPoints(string[] orgNumbers, string resourceId, CancellationToken cancellationToken);
}
