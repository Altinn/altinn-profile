using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations;

/// <summary>
/// Interface for HTTP client to handle contact details.
/// </summary>
public interface IContactDetailsHttpClient
{
    /// <summary>
    /// Retrieves contact details changes from the specified endpoint.
    /// </summary>
    /// <param name="endpointUrl">The URL of the endpoint to retrieve contact details changes from.</param>
    /// <param name="startIndex">The starting index for retrieving contact details changes.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
    Task<IEnumerable<IPersonChangeLog>?> GetContactDetailsChangesAsync(string endpointUrl, string startIndex);
}
