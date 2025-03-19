namespace Altinn.Profile.Core.OrganizationNotificationAddresses;

/// <summary>
/// Class describing the methods required for user contact point service
/// </summary>
public interface IOrganizationNotificationAddressesService
{
    /// <summary>
    /// Method for retrieving notification addresses for an organization to use by notifications api
    /// </summary>
    /// <param name="lookup">A lookup object containing a list of organization numbers to lookup contact points for</param>
    /// <param name="cancellationToken">To cancel the request before it is finished</param>
    /// <returns>The notification addresses or a boolean if failure.</returns>
    Task<Result<OrgContactPointsList, bool>> GetNotificationContactPoints(OrgContactPointLookup lookup, CancellationToken cancellationToken);
}
