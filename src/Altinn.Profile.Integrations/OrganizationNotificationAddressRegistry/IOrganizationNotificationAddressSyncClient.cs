namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;

/// <summary>
/// Defines an HTTP client to sync with a source registry for organizational notification addresses.
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
    Task<NotificationAddressChangesLog?> GetAddressChangesAsync(string endpointUrl);

    /// <summary>
    /// Formats the url to get the initial dataload - either from the last changed timestamp or from the beginning.
    /// </summary>
    /// <param name="lastUpdated">The timestamp to get changes since.</param>
    string GetInitialUrl(DateTime? lastUpdated);
}
