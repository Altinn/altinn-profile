namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;

/// <summary>
/// Defines an HTTP client to interact with a source registry for organizational notification addresses.
/// </summary>
public interface IOrganizationNotificationAddressSyncClient
{
    /// <summary>
    /// Retrieves changes to organizational notification addresses
    /// </summary>
    /// <param name="endpointUrl">The URL of the endpoint to retrieve contact details changes from.</param>
    /// <returns>
    /// A task that represents the asynchronous operation with the returned values.
    /// </returns>
    Task<NotificationAddressChangesLog> GetAddressChangesAsync(string endpointUrl);
}
