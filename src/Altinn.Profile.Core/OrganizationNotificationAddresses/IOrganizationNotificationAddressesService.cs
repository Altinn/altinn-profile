namespace Altinn.Profile.Core.OrganizationNotificationAddresses;

/// <summary>
/// Defines a service which can retrieve notification addresses for organizations
/// </summary>
public interface IOrganizationNotificationAddressesService
{
    /// <summary>
    /// Method for retrieving notification addresses for an organization to use by notifications api
    /// </summary>
    /// <param name="organizationNumbers">A list of organization numbers to lookup contact points for</param>
    /// <param name="cancellationToken">To cancel the request before it is finished</param>
    /// <returns>The notification addresses or a boolean if failure.</returns>
    Task<IEnumerable<Organization>> GetOrganizationNotificationAddresses(List<string> organizationNumbers, CancellationToken cancellationToken);
}
