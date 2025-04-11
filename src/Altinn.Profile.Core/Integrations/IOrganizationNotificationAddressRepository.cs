using Altinn.Profile.Core.OrganizationNotificationAddresses;

namespace Altinn.Profile.Core.Integrations;

/// <summary>
/// Defines a repository for accessing  notification addresses of organizations.
/// </summary>
public interface IOrganizationNotificationAddressRepository
{
    /// <summary>
    /// Fetches organizations notification addresses
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> with a collection of organizations as value.</returns>
    Task<IEnumerable<Organization>> GetOrganizationsAsync(List<string> organizationNumbers, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an organizations notification addresses
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> with an organizationn as value.</returns>
    Task<int> UpdateNotificationAddressAsync(Organization organization, NotificationAddress notificationAddress);

    /// <summary>
    /// Creates a new notification address for an organization
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> with an organizationn as value.</returns>
    Task<int> CreateNotificationAddressAsync(Organization organization, NotificationAddress notificationAddress);
}
