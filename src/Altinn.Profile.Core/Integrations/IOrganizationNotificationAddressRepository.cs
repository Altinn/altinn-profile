using Altinn.Profile.Core.OrganizationNotificationAddresses;

namespace Altinn.Profile.Core.Integrations;

/// <summary>
/// Defines a repository for interacting with notification addresses of organizations.
/// </summary>
public interface IOrganizationNotificationAddressRepository
{
    /// <summary>
    /// Fetches a single organization's notification addresses
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> with an organization as value.</returns>
    Task<Organization?> GetOrganizationAsync(string organizationNumber, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches organizations notification addresses
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> with a collection of organizations as value.</returns>
    Task<IEnumerable<Organization>> GetOrganizationsAsync(List<string> organizationNumbers, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches notification addresses filtered by email address
    /// </summary>
    /// <param name="fullAddress">The full address to search for in notification addresses</param>
    /// <param name="addressType">The type of address to search for (e.g., Email, Phone)</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
    /// <returns>A <see cref="Task{TResult}"/> with a collection of organization notification addresses as value.</returns>
    Task<IEnumerable<Organization>> GetOrganizationNotificationAddressesByFullAddressAsync(string fullAddress, AddressType addressType, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a new notification address for an organization
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> with the notification address as value.</returns>
    Task<NotificationAddress> CreateNotificationAddressAsync(string organizationNumber, NotificationAddress notificationAddress, string registryId);

    /// <summary>
    /// Delete a notification address for an organization
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> with the notification address as value.</returns>
    Task<NotificationAddress> DeleteNotificationAddressAsync(int notificationAddressId);

    /// <summary>
    /// Updates an existing notification address for an organization
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> with the notification address as value.</returns>
    Task<NotificationAddress> UpdateNotificationAddressAsync(NotificationAddress notificationAddress, string registryId);

    /// <summary>
    /// Restores a previously soft deleted notification address for an organization.
    /// </summary>
    /// <param name="notificationAddressId">The ID of the notification address to restore.</param>
    /// <param name="registryId">The registry identifier associated with the notification address.</param>
    /// <returns>The restored <see cref="NotificationAddress"/>.</returns>
    Task<NotificationAddress> RestoreNotificationAddress(int notificationAddressId, string registryId);
}
