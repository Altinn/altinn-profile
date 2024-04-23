using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

using Altinn.Platform.Profile.Models;
using Altinn.Profile.Core;
using Altinn.Profile.Core.Integrations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.Profile.Integrations.SblBridge;

/// <summary>
/// Represents an implementation of <see cref="IUserProfiles"/> using SBLBridge to obtain profile information.
/// </summary>
public class UserProfileClient : IUserProfileClient
{
    private readonly ILogger<UserProfileClient> _logger;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _serializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserProfileClient"/> class
    /// </summary>
    /// <param name="httpClient">HttpClient from default http client factory</param>
    /// <param name="logger">the logger</param>
    /// <param name="settings">the sbl bridge settings</param>
    public UserProfileClient(
        HttpClient httpClient,
        ILogger<UserProfileClient> logger,
        IOptions<SblBridgeSettings> settings)
    {
        _logger = logger;
        _client = httpClient;
        _client.BaseAddress = new Uri(settings.Value.ApiProfileEndpoint);

        _serializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
    }

    /// <inheritdoc />
    public async Task<Result<UserProfile, bool>> GetUser(int userId)
    {
        string endpoint = $"users/{userId}";

        HttpResponseMessage response = await _client.GetAsync(endpoint);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Getting user {userId} failed with {statusCode}", userId, response.StatusCode);
            return false;
        }

        string content = await response.Content.ReadAsStringAsync();
        UserProfile user = JsonSerializer.Deserialize<UserProfile>(content, _serializerOptions)!;

        return user;
    }

    /// <inheritdoc />
    public async Task<Result<UserProfile, bool>> GetUser(string ssn)
    {
        string endpoint = "users";
        StringContent requestBody = new StringContent(JsonSerializer.Serialize(ssn), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _client.PostAsync(endpoint, requestBody);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Getting user by SSN failed with statuscode {statusCode}", response.StatusCode);
            return false;
        }

        string content = await response.Content.ReadAsStringAsync();
        UserProfile user = JsonSerializer.Deserialize<UserProfile>(content, _serializerOptions)!;

        return user;
    }

    /// <inheritdoc />
    public async Task<Result<UserProfile, bool>> GetUserByUuid(Guid userUuid)
    {
        string endpoint = $"users?useruuid={userUuid}";

        HttpResponseMessage response = await _client.GetAsync(endpoint);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Getting user {userUuid} failed with {statusCode}", userUuid, response.StatusCode);
            return false;
        }

        string content = await response.Content.ReadAsStringAsync();
        UserProfile user = JsonSerializer.Deserialize<UserProfile>(content, _serializerOptions)!;

        return user;
    }

    /// <inheritdoc />
    public async Task<Result<List<UserProfile>, bool>> GetUserListByUuid(List<Guid> userUuidList)
    {
        string endpoint = "users/byuuid";
        StringContent requestBody = new StringContent(JsonSerializer.Serialize(userUuidList), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _client.PostAsync(endpoint, requestBody);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Getting users failed with {statusCode}", response.StatusCode);
            return false;
        }

        string content = await response.Content.ReadAsStringAsync();
        List<UserProfile> users = JsonSerializer.Deserialize<List<UserProfile>>(content, _serializerOptions)!;

        return users;
    }

    /// <inheritdoc />
    public async Task<Result<UserProfile, bool>> GetUserByUsername(string username)
    {
        string endpoint = $"users/?username={username}";

        HttpResponseMessage response = await _client.GetAsync(endpoint);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Getting user {username} failed with {statusCode}", HttpUtility.HtmlEncode(username), response.StatusCode);
            return false;
        }

        string content = await response.Content.ReadAsStringAsync();
        UserProfile user = JsonSerializer.Deserialize<UserProfile>(content, _serializerOptions)!;

        return user;
    }
}
