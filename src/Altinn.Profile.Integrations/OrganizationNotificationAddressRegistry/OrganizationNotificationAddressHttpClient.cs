using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Profile.Core.Extensions;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;

/// <summary>
/// An HTTP client to interact with a source registry for organizational notification addresses.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="OrganizationNotificationAddressHttpClient"/> class.
/// </remarks>
/// <param name="httpClient">The HTTP client to interact with KoFuVi.</param>
/// <param name="organizationNotificationAddressSettings">Settings for http client with base addresses</param>
public class OrganizationNotificationAddressHttpClient(HttpClient httpClient, OrganizationNotificationAddressSettings organizationNotificationAddressSettings) : IOrganizationNotificationAddressSyncClient, IOrganizationNotificationAddressUpdateClient
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly OrganizationNotificationAddressSettings _organizationNotificationAddressSettings = organizationNotificationAddressSettings;
    private readonly JsonSerializerOptions _options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">The URL is invalid. - endpointUrl</exception>
    public string GetInitialUrl(DateTime? lastUpdated)
    {
        // Time should be in iso8601 format. Example: 2018-02-15T11:07:12Z
        string? fullUrl = _organizationNotificationAddressSettings.ChangesLogEndpoint + $"?pageSize={_organizationNotificationAddressSettings.ChangesLogPageSize}";
        if (lastUpdated != null)
        {
            fullUrl += $"&since={lastUpdated:yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ}";
        }

        return fullUrl;
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
    public async Task<string> CreateNewNotificationAddress(NotificationAddress notificationAddress, string organizationNumber)
    {
        var request = DataMapper.MapToRegistryRequest(notificationAddress, organizationNumber);
        var json = JsonSerializer.Serialize(request, _options);
        string command = "/define";
        
        var responseObject = await PostAsync(json, command);

        return responseObject;
    }

    /// <inheritdoc/>
    public async Task<string> UpdateNotificationAddress(NotificationAddress notificationAddress, string organizationNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(notificationAddress.RegistryID);

        var request = DataMapper.MapToRegistryRequest(notificationAddress, organizationNumber);

        var json = JsonSerializer.Serialize(request, _options);
        string command = @"/replace/" + notificationAddress.RegistryID;

        var responseObject = await PostAsync(json, command);

        return responseObject;
    }

    /// <inheritdoc/>
    public async Task<string> DeleteNotificationAddress(string notificationAddressRegistryId)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(notificationAddressRegistryId);

        // Using /replace/ with empty payload as per API requirements for deletion
        string command = @"/replace/" + notificationAddressRegistryId;
        string json = string.Empty;

        var responseObject = await PostAsync(json, command);

        return responseObject;
    }

    private async Task<string> PostAsync(string request, string command)
    {
        var stringContent = new StringContent(request, Encoding.UTF8, "application/json");
        string endpoint = _organizationNotificationAddressSettings.UpdateEndpoint + command;

        var response = await _httpClient.PostAsync(endpoint, stringContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new OrganizationNotificationAddressChangesException($"Kof-reception connection error. StatusCode: {response.StatusCode}");
        }

        var responseData = await response.Content.ReadAsStringAsync();

        var responseObject = JsonSerializer.Deserialize<RegistryResponse>(responseData, _options);
        if (responseObject == null)
        {
            throw new OrganizationNotificationAddressChangesException("Failed to deserialize the response from external registry.");
        }

        if (responseObject.BoolResult != true || responseObject.AddressID == null)
        {
            throw new OrganizationNotificationAddressChangesException(responseObject.Status + ": " + responseObject.Details);
        }

        return responseObject.AddressID;
    }
}
