namespace Altinn.Profile.Integrations.ContactRegister;

/// <summary>
/// Defines a component that can perform synchronization of contact information for individuals.
/// </summary>
public interface IContactRegisterUpdateJob
{
    /// <summary>
    /// Retrieves all changes from the source registry and updates the local contact information.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SyncContactInformationAsync();
}
