namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;

/// <summary>
/// Defines a component that can perform synchronization of contact information for organizations.
/// </summary>
public interface IOrganizationNotificationAddressSyncJob
{
    /// <summary>
    /// Retrieves all changes from the source registry and updates the local contact information.
    /// </summary>
    /// <param name="cancellationToken">A token used to stop the synchronization, for instance when the lease that guards the job is lost.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SyncNotificationAddressesAsync(CancellationToken cancellationToken = default);
}
