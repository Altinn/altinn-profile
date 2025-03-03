namespace Altinn.Profile.Integrations.OrganizationNotificationAddress
{
    /// <summary>
    /// Defines a component that can perform synchronization of notification addresses for organizations.
    /// </summary>
    public interface IOrganizationNotificationAddressHttpClient
    {
        /// <summary>
        /// Retrieves all changes from the source registry(KoFuVi) and updates the local contact information.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task<NotificationAddressChangesLog> GetAddressChangesAsync(string endpointUrl);
    }
}
