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
    /// Creates a new notification address for an organization
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> with an organization as value.</returns>
    Task<Organization> CreateNotificationAddressAsync(string organizationNumber, NotificationAddress notificationAddress);
}
