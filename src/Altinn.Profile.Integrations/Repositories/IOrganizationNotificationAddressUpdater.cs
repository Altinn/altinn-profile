using Altinn.Profile.Core;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models;

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
    /// <param name="cancellationToken">A token used to stop the synchronization.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an integer value giving the number of writes to the database.
    /// </returns>
    Task<int> SyncNotificationAddressesAsync(NotificationAddressChangesLog organizationNotificationAddressChanges, CancellationToken cancellationToken = default);
}
