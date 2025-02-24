namespace Altinn.Profile.Integrations.OfficialAddressRegister
{
    /// <summary>
    /// Defines a component that can perform synchronization of official contact information for organizations.
    /// </summary>
    public interface IOfficialAddressHttpClient
    {
        /// <summary>
        /// Retrieves all changes from the source registry(KoFuVi) and updates the local contact information.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<OfficialAddressRegisterChangesLog> GetAddressChangesAsync(string endpointUrl);
    }
}
