using System.Text;
using System.Text.Json;

using Altinn.Profile.Core;
using Altinn.Profile.Core.ContactRegister;
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
    /// <param name="logger">The logger instance used for logging.</param>
    /// <param name="httpClient">The HTTP client to interact with the contact register.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="logger"/> or <paramref name="httpClient"/> is null.</exception>
    public ContactRegisterHttpClient(ILogger<ContactRegisterHttpClient> logger, HttpClient httpClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <summary>
    /// Retrieves the changes in persons' contact details from the specified endpoint.
    /// </summary>
    /// <param name="endpointUrl">The URL of the endpoint to retrieve contact details changes from.</param>
    /// <param name="startingIdentifier">The starting identifier for retrieving contact details changes.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object, where <see cref="IContactRegisterChangesLog"/> represents the successful result and <see cref="bool"/> indicates a failure.
    /// </returns>
    /// <exception cref="System.ArgumentException">The URL is invalid. - endpointUrl</exception>
    public async Task<Result<IContactRegisterChangesLog, bool>> GetContactDetailsChangesAsync(string endpointUrl, long startingIdentifier)
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

        try
        {
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "// ContactRegisterHttpClient // GetContactDetailsChangesAsync // Unexpected response. Failed with {StatusCode} and message {Message}",
                    response.StatusCode,
                    await response.Content.ReadAsStringAsync());

                return false;
            }

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
            _logger.LogError(ex, "An error occurred while retrieving changes from the contact register.");

            throw;
        }
    }
}
