using System.Text;
using System.Text.Json;

using Altinn.Profile.Core;
using Altinn.Profile.Core.ContactRegister;
using Altinn.Profile.Core.Extensions;

using Microsoft.Extensions.Logging;

namespace Altinn.Profile.Integrations.ContactRegister;

/// <summary>
/// Implementation of <see cref="IContactRegisterHttpClient"/> to handle contact details via HTTP.
/// </summary>
public class ContactRegisterHttpClient : IContactRegisterHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ContactRegisterHttpClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContactRegisterHttpClient"/> class.
    /// </summary>
    /// <param name="logger">The logger instance used for logging.</param>
    /// <param name="httpClient">The service for retrieving the contact details.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the <paramref name="logger"/> or <paramref name="httpClient"/> is null.
    /// </exception>
    public ContactRegisterHttpClient(ILogger<ContactRegisterHttpClient> logger, HttpClient httpClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Retrieves contact details changes from the specified endpoint.
    /// </summary>
    /// <param name="endpointUrl">The URL of the endpoint to retrieve contact details changes from.</param>
    /// <param name="latestChangeNumber">The starting index for retrieving contact details changes.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the HTTP response message.</returns>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="margin"/> is less than zero.</exception>
    public async Task<Result<IContactRegisterChangesLog, bool>> GetContactDetailsChangesAsync(string endpointUrl, long latestChangeNumber)
    {
        if (!endpointUrl.IsValidUrl())
        {
            throw new ArgumentException("The URL is invalid.", nameof(endpointUrl));
        }

        var request = new HttpRequestMessage(HttpMethod.Post, endpointUrl)
        {
            Content = new StringContent($"{{\"fraEndringsId\": {latestChangeNumber}}}", Encoding.UTF8, "application/json")
        };

        try
        {
            var response = await _httpClient.SendAsync(request);

            var responseData = await response.Content.ReadAsStringAsync();

            var responseObject = JsonSerializer.Deserialize<ContactRegisterChangesLog>(responseData);
            if (responseObject == null || responseObject.ContactPreferencesSnapshots == null)
            {
                return false;
            }

            return responseObject;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving contact details changes.");

            throw;
        }
    }
}
