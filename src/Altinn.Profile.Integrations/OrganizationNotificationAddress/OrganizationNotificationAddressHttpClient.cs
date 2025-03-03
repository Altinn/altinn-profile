using System.Text.Json;
using Altinn.Profile.Core.Extensions;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddress;

/// <summary>
/// An HTTP client to interact with the contact register.
/// </summary>
public class OrganizationNotificationAddressHttpClient : IOrganizationNotificationAddressHttpClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrganizationNotificationAddressHttpClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to interact with KoFuVi.</param>
    public OrganizationNotificationAddressHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Retrieves the changes in persons' contact details from the specified endpoint.
    /// </summary>
    /// <param name="endpointUrl">The URL of the endpoint to retrieve contact details changes from.</param>
    /// <returns>
    /// A task that represents the asynchronous operation with the returned values.
    /// </returns>
    /// <exception cref="System.ArgumentException">The URL is invalid. - endpointUrl</exception>
    public async Task<NotificationAddressChangesLog> GetAddressChangesAsync(string endpointUrl)
    {
        if (!endpointUrl.IsValidUrl())
        {
            throw new ArgumentException("The endpoint URL is invalid.", nameof(endpointUrl));
        }

        var request = new HttpRequestMessage(HttpMethod.Get, endpointUrl);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            throw new OrganizationNotificationAddressChangesException($"Failed to retrieve contact details changes. StatusCode: {response.StatusCode}");
        }

        var responseData = await response.Content.ReadAsStringAsync();

        var responseObject = JsonSerializer.Deserialize<NotificationAddressChangesLog>(responseData);

        if (responseObject == null || responseObject.OrganizationNotificationAddressList == null)
        {
            throw new OrganizationNotificationAddressChangesException("Failed to deserialize the response from the organization notification address sync.");
        }

        return responseObject;
    }
}
