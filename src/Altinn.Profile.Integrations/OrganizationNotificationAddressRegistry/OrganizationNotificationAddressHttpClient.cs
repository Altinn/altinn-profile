using System.Text;
using System.Text.Json;
using Altinn.Profile.Core.Extensions;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;

/// <summary>
/// An HTTP client to interact with the contact register.
/// </summary>
public class OrganizationNotificationAddressHttpClient : IOrganizationNotificationAddressSyncClient, IOrganizationNotificationAddressUpdateClient
{
    private readonly HttpClient _httpClient;
    private readonly OrganizationNotificationAddressSettings _organizationNotificationAddressSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="OrganizationNotificationAddressHttpClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to interact with KoFuVi.</param>
    /// <param name="organizationNotificationAddressSettings">Settings for http client with base addresses</param>
    public OrganizationNotificationAddressHttpClient(HttpClient httpClient, OrganizationNotificationAddressSettings organizationNotificationAddressSettings)
    {
        _httpClient = httpClient;
        _organizationNotificationAddressSettings = organizationNotificationAddressSettings;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">The URL is invalid. - endpointUrl</exception>
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

    /// <inheritdoc/>
    public async Task<RegistryResponse> CreateNewNotificationAddress(NotificationAddress notificationAddress, Organization organization)
    {
        var request = DataMapper.MapToRegistryRequest(notificationAddress, organization);
        var json = JsonSerializer.Serialize(request);
        string command = "/define";
        
        var responseObject = await PostAsync(json, command);

        return responseObject;
    }

    /// <inheritdoc/>
    public async Task<RegistryResponse> UpdateNotificationAddress(NotificationAddress notificationAddress, Organization organization)
    {
        if (notificationAddress.RegistryID == null)
        {
            throw new ArgumentException("RegistryID cannot be null when updating a notification address");
        }

        var request = DataMapper.MapToRegistryRequest(notificationAddress, organization);

        var json = JsonSerializer.Serialize(request);
        string command = @"/replace/" + notificationAddress.RegistryID;

        var responseObject = await PostAsync(json, command);

        return responseObject;
    }

    /// <inheritdoc/>
    public async Task<RegistryResponse> DeleteNotificationAddress(NotificationAddress notificationAddress)
    {
        if (notificationAddress.RegistryID == null)
        {
            throw new ArgumentException("RegistryID cannot be null when deleting a notification address");
        }

        // Using /replace/ with empty payload as per API requirements for deletion
        string command = @"/replace/" + notificationAddress.RegistryID;
        string json = string.Empty;

        var responseObject = await PostAsync(json, command);

        return responseObject;
    }

    private async Task<RegistryResponse> PostAsync(string request, string command)
    {
        var stringContent = new StringContent(request, Encoding.UTF8, "application/json");
        string endpoint = _organizationNotificationAddressSettings.UpdateEndpoint + command;

        var response = await _httpClient.PostAsync(endpoint, stringContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new OrganizationNotificationAddressChangesException($"Kof-reception connection error. StatusCode: {response.StatusCode}");
        }

        var responseData = await response.Content.ReadAsStringAsync();

        var responseObject = JsonSerializer.Deserialize<RegistryResponse>(responseData);
        if (responseObject == null)
        {
            throw new OrganizationNotificationAddressChangesException("Failed to deserialize the response from external registry.");
        }

        return responseObject;
    }
}
