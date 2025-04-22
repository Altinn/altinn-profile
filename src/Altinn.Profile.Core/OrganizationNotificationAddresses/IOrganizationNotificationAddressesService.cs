namespace Altinn.Profile.Core.OrganizationNotificationAddresses;

/// <summary>
/// Defines a service which can retrieve notification addresses for organizations
/// </summary>
public interface IOrganizationNotificationAddressesService
{
    /// <summary>
    /// Method for creating a notification addresses for an organization
    /// </summary>
    /// <param name="organizationNumber">An organization number to indicate which organization to update addresses for</param>
    /// <param name="notificationAddresses">The new notification address</param>
    /// <param name="cancellationToken">To cancel the request before it is finished</param>
    Task<Organization> CreateNotificationAddress(string organizationNumber, NotificationAddress notificationAddresses, CancellationToken cancellationToken);

    /// <summary>
    /// Method for retrieving notification addresses for an organization
    /// </summary>
    /// <param name="organizationNumbers">A list of organization numbers to lookup contact points for</param>
    /// <param name="cancellationToken">To cancel the request before it is finished</param>
    /// <returns>The notification addresses or a boolean if failure.</returns>
    Task<IEnumerable<Organization>> GetOrganizationNotificationAddresses(List<string> organizationNumbers, CancellationToken cancellationToken);
}
