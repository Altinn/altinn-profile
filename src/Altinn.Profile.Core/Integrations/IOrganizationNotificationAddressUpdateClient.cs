using Altinn.Profile.Core.OrganizationNotificationAddresses;

namespace Altinn.Profile.Core.Integrations;

/// <summary>
/// Defines an HTTP client to interact with a source registry for organizational notification addresses.
/// </summary>
public interface IOrganizationNotificationAddressUpdateClient
{
    /// <summary>
    /// Updates the registry with a new notification address
    /// </summary>
    /// <param name="notificationAddress">The notification address.</param>
    /// <param name="organizationNumber">The organization number the notification address belongs to</param>
    /// <returns>
    /// A task that represents the asynchronous operation with the returned values.
    /// </returns>
    Task<string> CreateNewNotificationAddress(NotificationAddress notificationAddress, string organizationNumber);

    /// <summary>
    /// Updates the notification address in the registry
    /// </summary>
    /// <param name="notificationAddress">The notification address to be updated.</param>
    /// <param name="organizationNumber">The organization number the notification address belongs to</param>
    /// <returns>
    /// A task that represents the asynchronous operation with the returned values.
    /// </returns>
    Task<string> UpdateNotificationAddress(NotificationAddress notificationAddress, string organizationNumber);

    /// <summary>
    /// Deletes a notification address from the registry
    /// </summary>
    /// <param name="notificationAddressRegistryId">The id of the notification address to be deleted.</param>
    /// <returns>
    /// A task that represents the asynchronous operation with the returned values.
    /// </returns>
    Task<string> DeleteNotificationAddress(string notificationAddressRegistryId);
}
