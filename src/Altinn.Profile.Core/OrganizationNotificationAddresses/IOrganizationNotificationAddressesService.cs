namespace Altinn.Profile.Core.OrganizationNotificationAddresses;

/// <summary>
/// Defines a service which can retrieve notification addresses for organizations
/// </summary>
public interface IOrganizationNotificationAddressesService
{
    /// <summary>
    /// Method for retrieving notification addresses for an organization to use by notifications api
    /// </summary>
    /// <param name="lookup">Wraps  a list of organization numbers to lookup contact points for</param>
    /// <param name="cancellationToken">To cancel the request before it is finished</param>
    /// <returns>The notification addresses or a boolean if failure.</returns>
    Task<OrgContactPointsList> GetNotificationContactPoints(OrgContactPointLookup lookup, CancellationToken cancellationToken);
}
