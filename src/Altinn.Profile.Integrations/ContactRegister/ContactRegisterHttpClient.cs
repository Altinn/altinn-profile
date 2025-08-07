using System.Text;
using System.Text.Json;
using Altinn.Profile.Core.Extensions;
using Microsoft.Extensions.Logging;

namespace Altinn.Profile.Integrations.ContactRegister;

/// <summary>
/// An HTTP client to interact with the contact register.
/// </summary>
public class ContactRegisterHttpClient : IContactRegisterHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ContactRegisterHttpClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContactRegisterHttpClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to interact with the contact register.</param>
    /// <param name="logger">The logger</param>
    public ContactRegisterHttpClient(HttpClient httpClient, ILogger<ContactRegisterHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves the changes in persons' contact details from the specified endpoint.
    /// </summary>
    /// <param name="endpointUrl">The URL of the endpoint to retrieve contact details changes from.</param>
    /// <param name="startingIdentifier">The starting identifier for retrieving contact details changes.</param>
    /// <returns>
    /// A task that represents the asynchronous operation with the returned values.
    /// </returns>
    /// <exception cref="System.ArgumentException">The URL is invalid. - endpointUrl</exception>
    public async Task<ContactRegisterChangesLog?> GetContactDetailsChangesAsync(string endpointUrl, long startingIdentifier)
    {
        if (!endpointUrl.IsValidUrl())
        {
            throw new ArgumentException("The endpoint URL is invalid.", nameof(endpointUrl));
        }

        if (startingIdentifier < 0)
        {
            throw new ArgumentException("The starting position is invalid.", nameof(endpointUrl));
        }

        var request = new HttpRequestMessage(HttpMethod.Post, endpointUrl)
        {
            Content = new StringContent($"{{\"fraEndringsId\": {startingIdentifier}}}", Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to retrieve contact details changes. Received status code {StatusCode}.", response.StatusCode);
            return null;
        }

        var responseData = await response.Content.ReadAsStringAsync();

        var responseObject = JsonSerializer.Deserialize<ContactRegisterChangesLog>(responseData);

        if (responseObject == null || responseObject.ContactPreferencesSnapshots == null)
        {
            throw new ContactAndReservationChangesException("Failed to deserialize the response from the contact and reservation registry.");
        }

        return responseObject;
    }
}
