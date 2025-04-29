namespace Altinn.Profile.Core.OrganizationNotificationAddresses;

/// <summary>
/// Defines a service which handles notification addresses for organizations
/// </summary>
public interface IOrganizationNotificationAddressesService
{
    /// <summary>
    /// Method for creating a notification addresses for an organization. Data is written primarily to an <see cref="IOrganizationNotificationAddressUpdateClient"/> and lastly to the <see cref="IOrganizationNotificationAddressRepository"/>.
    /// </summary>
    /// <param name="organizationNumber">An organization number to indicate which organization to update addresses for</param>
    /// <param name="notificationAddress">The new notification address</param>
    /// <param name="cancellationToken">To cancel the request before it is finished</param>
    Task<NotificationAddress> CreateNotificationAddress(string organizationNumber, NotificationAddress notificationAddress, CancellationToken cancellationToken);

    /// <summary>
    /// Method for retrieving notification addresses for an organization
    /// </summary>
    /// <param name="organizationNumbers">A list of organization numbers to lookup contact points for</param>
    /// <param name="cancellationToken">To cancel the request before it is finished</param>
    /// <returns>The notification addresses or a boolean if failure.</returns>
    Task<IEnumerable<Organization>> GetOrganizationNotificationAddresses(List<string> organizationNumbers, CancellationToken cancellationToken);
}
