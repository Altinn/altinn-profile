using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;

/// <summary>
/// Defines an HTTP client to interact with a source registry for updates to organizational notification addresses.
/// </summary>
public interface IOrganizationNotificationAddressUpdateClient
{
    /// <summary>
    /// Updates the registry with a new notification address
    /// </summary>
    /// <param name="notificationAddress">The notification address to create.</param>
    /// <param name="organization">The organization the notification address belongs to</param>
    /// <returns>
    /// A task that represents the asynchronous operation with the returned values.
    /// </returns>
    Task<RegistryResponse> CreateNewNotificationAddress(NotificationAddress notificationAddress, Organization organization);

    /// <summary>
    /// Updates an existing notification address in the registry
    /// </summary>
    /// <param name="notificationAddress">The notification address to be updated.</param>
    /// <param name="organization">The organization the notification address belongs to</param>
    /// <returns>
    /// A task that represents the asynchronous operation with the returned values.
    /// </returns>
    Task<RegistryResponse> UpdateNotificationAddress(NotificationAddress notificationAddress, Organization organization);

    /// <summary>
    /// Deletes a notification address from the registry
    /// </summary>
    /// <param name="notificationAddressRegistryId">The id of the notification address in the regsirty.</param>
    /// <returns>
    /// A task that represents the asynchronous operation with the returned values.
    /// </returns>
    Task<RegistryResponse> DeleteNotificationAddress(string notificationAddressRegistryId);
}
