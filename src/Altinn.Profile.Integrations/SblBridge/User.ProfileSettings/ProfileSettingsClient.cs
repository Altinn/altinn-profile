using System.Net;
using System.Text;
using System.Text.Json;

using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.User.ProfileSettings;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Using SBLBridge to update user portal settings in A2
/// </summary>
public class ProfileSettingsClient : IProfileSettingsClient
{
    private readonly ILogger<ProfileSettingsClient> _logger;
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProfileSettingsClient"/> class
    /// </summary>
    /// <param name="httpClient">HttpClient from default http client factory</param>
    /// <param name="logger">the logger</param>
    /// <param name="settings">the sbl bridge settings</param>
    public ProfileSettingsClient(
        HttpClient httpClient,
        ILogger<ProfileSettingsClient> logger,
        IOptions<SblBridgeSettings> settings)
    {
        _logger = logger;
        _client = httpClient;
        _client.BaseAddress = new Uri(settings.Value.ApiProfileEndpoint);
    }

    /// <inheritdoc />
    public async Task UpdatePortalSettings(ProfileSettingsChangedRequest request)
    {
        string endpoint = $"users/portalsettings/update";

        StringContent requestBody = new(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _client.PostAsync(endpoint, requestBody);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode is HttpStatusCode.InternalServerError or HttpStatusCode.ServiceUnavailable)
            {
                throw new InternalServerErrorException("Received error response while updating portal settings.");
            }

            _logger.LogError(
                "// ProfileSettingsClient // UpdatePortalSettings // Unexpected response. Failed with {StatusCode} and message {Message}",
                response.StatusCode,
                await response.Content.ReadAsStringAsync());

            return;
        }
    }
}
