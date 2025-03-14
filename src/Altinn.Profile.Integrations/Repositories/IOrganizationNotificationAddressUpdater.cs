using Altinn.Profile.Core;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;

namespace Altinn.Profile.Integrations.Repositories;

/// <summary>
/// Defines operations for syncrhonizing changes to notification addresses for organizations
/// </summary>
public interface IOrganizationNotificationAddressUpdater
{
    /// <summary>
    /// Asynchronously synchronizes the changes in organizations notification addresses.
    /// </summary>
    /// <param name="organizationNotificationAddressChanges">The snapshots of notification addresses to be synchronized.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object with a <see cref="bool"/> indicating success or failure.
    /// </returns>
    Task<int> SyncNotificationAddressesAsync(NotificationAddressChangesLog organizationNotificationAddressChanges);
}
