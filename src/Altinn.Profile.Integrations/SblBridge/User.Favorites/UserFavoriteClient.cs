using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.Profile.Integrations.SblBridge.User.Favorites;

/// <summary>
/// Using SBLBridge to update favorites in A2
/// </summary>
public class UserFavoriteClient : IUserFavoriteClient
{
    private readonly ILogger<UserFavoriteClient> _logger;
    private readonly HttpClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserFavoriteClient"/> class
    /// </summary>
    /// <param name="httpClient">HttpClient from default http client factory</param>
    /// <param name="logger">the logger</param>
    /// <param name="settings">the sbl bridge settings</param>
    public UserFavoriteClient(
        HttpClient httpClient,
        ILogger<UserFavoriteClient> logger,
        IOptions<SblBridgeSettings> settings)
    {
        _logger = logger;
        _client = httpClient;
        _client.BaseAddress = new Uri(settings.Value.ApiProfileEndpoint);
    }

    /// <inheritdoc />
    public async Task UpdateFavorites(FavoriteChangedRequest request)
    {
        string endpoint = $"users/favorite/update";

        StringContent requestBody = new(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _client.PostAsync(endpoint, requestBody);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError(
                "// UserFavoriteClient // UpdateFavorites // Unexpected response. Failed with {StatusCode} and message {Message}",
                response.StatusCode,
                await response.Content.ReadAsStringAsync());

            return;
        }
    }
}
