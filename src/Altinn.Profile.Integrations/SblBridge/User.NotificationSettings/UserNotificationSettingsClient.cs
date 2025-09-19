using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.Profile.Integrations.SblBridge.User.NotificationSettings;

/// <summary>
/// Using SBLBridge to update the users professional notification settings in A2. Called ReporteeNotificationEndpoint in SBLBridge.
/// </summary>
public class UserNotificationSettingsClient : IUserNotificationSettingsClient
{
    private readonly ILogger<UserNotificationSettingsClient> _logger;
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserNotificationSettingsClient"/> class
    /// </summary>
    /// <param name="httpClient">HttpClient from default http client factory</param>
    /// <param name="logger">the logger</param>
    /// <param name="settings">the sbl bridge settings</param>
    public UserNotificationSettingsClient(
        HttpClient httpClient,
        ILogger<UserNotificationSettingsClient> logger,
        IOptions<SblBridgeSettings> settings)
    {
        _logger = logger;
        _client = httpClient;
        _client.BaseAddress = new Uri(settings.Value.ApiProfileEndpoint);
    }

    /// <inheritdoc />
    public async Task UpdateNotificationSettings(NotificationSettingsChangedRequest request)
    {
        string endpoint = $"users/reporteenotificationendpoint/update";

        StringContent requestBody = new(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        
        HttpResponseMessage response = await _client.PostAsync(endpoint, requestBody);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode is HttpStatusCode.InternalServerError or HttpStatusCode.ServiceUnavailable)
            {
                throw new InternalServerErrorException("Received error response while updating notification settings.");
            }

            _logger.LogError(
                "// UserNotificationSettingsClient // UpdateNotificationSettings // Unexpected response. Failed with {StatusCode} and message {Message}",
                response.StatusCode,
                await response.Content.ReadAsStringAsync());

            return;
        }
    }
}
