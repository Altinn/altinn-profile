namespace Altinn.Profile.Core.OrganizationNotificationAddresses;

/// <summary>
/// Defines a service which handles notification addresses for organizations
/// </summary>
public interface IOrganizationNotificationAddressesService
{
    /// <summary>
    /// Method for creating a notification address for an organization.
    /// </summary>
    /// <param name="organizationNumber">An organization number to indicate which organization to add address for</param>
    /// <param name="notificationAddress">The new notification address</param>
    /// <param name="cancellationToken">To cancel the request before it is finished</param>
    Task<NotificationAddress> CreateNotificationAddress(string organizationNumber, NotificationAddress notificationAddress, CancellationToken cancellationToken);

    /// <summary>
    /// Method for updating a notification address for an organization. 
    /// </summary>
    /// <param name="organizationNumber">An organization number to indicate which organization to update address for</param>
    /// <param name="notificationAddress">The notification address with updated data</param>
    /// <param name="cancellationToken">To cancel the request before it is finished</param>
    Task<NotificationAddress?> UpdateNotificationAddress(string organizationNumber, NotificationAddress notificationAddress, CancellationToken cancellationToken);

    /// <summary>
    /// Method for retrieving notification addresses for an organization.
    /// </summary>
    /// <param name="organizationNumbers">A list of organization numbers to lookup contact points for</param>
    /// <param name="cancellationToken">To cancel the request before it is finished</param>
    /// <returns>The notification addresses or a boolean if failure.</returns>
    Task<IEnumerable<Organization>> GetOrganizationNotificationAddresses(List<string> organizationNumbers, CancellationToken cancellationToken);

    /// <summary>
    /// Method for deleting a notification addresses for an organization
    /// </summary>
    /// <param name="organizationNumber">An organization number to indicate which organization to update addresses for</param>
    /// <param name="notificationAddressId">The new notification address</param>
    /// <param name="cancellationToken">To cancel the request before it is finished</param>
    Task<NotificationAddress?> DeleteNotificationAddress(string organizationNumber, int notificationAddressId, CancellationToken cancellationToken);
}
