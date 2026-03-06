using System.Net;
using System.Text;
using System.Text.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.Profile.Integrations.SblBridge.User.PrivateConsent;

/// <summary>
/// Using SBLBridge to update user private consent profile in A2
/// </summary>
public class PrivateConsentProfileClient : IPrivateConsentProfileClient
{
    private readonly ILogger<PrivateConsentProfileClient> _logger;
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrivateConsentProfileClient"/> class
    /// </summary>
    /// <param name="httpClient">HttpClient from default http client factory</param>
    /// <param name="logger">the logger</param>
    /// <param name="settings">the sbl bridge settings</param>
    public PrivateConsentProfileClient(
        HttpClient httpClient,
        ILogger<PrivateConsentProfileClient> logger,
        IOptions<SblBridgeSettings> settings)
    {
        _logger = logger;
        _client = httpClient;
        _client.BaseAddress = new Uri(settings.Value.ApiProfileEndpoint);
    }

    /// <inheritdoc />
    public async Task UpdatePrivateConsent(PrivateConsentChangedRequest request)
    {
        string endpoint = $"users/privateconsentprofile/update";

        StringContent requestBody = new(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _client.PostAsync(endpoint, requestBody);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode is HttpStatusCode.InternalServerError or HttpStatusCode.ServiceUnavailable)
            {
                throw new InternalServerErrorException("Received error response while updating private consent profile.");
            }

            _logger.LogError(
                "// PrivateConsentProfileClient // UpdatePrivateConsent // Unexpected response. Failed with {StatusCode} and message {Message}",
                response.StatusCode,
                await response.Content.ReadAsStringAsync());

            return;
        }
    }
}
