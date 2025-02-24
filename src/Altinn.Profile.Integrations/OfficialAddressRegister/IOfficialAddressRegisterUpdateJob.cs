namespace Altinn.Profile.Integrations.OfficialAddressRegister;

/// <summary>
/// Defines a component that can perform synchronization of contact information for organizations.
/// </summary>
public interface IOfficialAddressRegisterUpdateJob
{
    /// <summary>
    /// Retrieves all changes from the source registry and updates the local contact information.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SyncContactInformationAsync();
}
