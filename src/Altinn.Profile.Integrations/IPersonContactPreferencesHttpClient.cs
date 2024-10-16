using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations;

/// <summary>
/// Interface for HTTP client to handle contact details.
/// </summary>
public interface IPersonContactPreferencesHttpClient
{
    /// <summary>
    /// Retrieves contact details changes from the specified endpoint.
    /// </summary>
    /// <param name="endpointUrl">The URL of the endpoint to retrieve contact details changes from.</param>
    /// <param name="latestChangeNumber">The starting index for retrieving contact details changes.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
    Task<IPersonContactPreferencesChangesLog?> GetContactDetailsChangesAsync(string endpointUrl, long latestChangeNumber);
}
