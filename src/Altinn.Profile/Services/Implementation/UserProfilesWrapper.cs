using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Altinn.Platform.Profile.Models;
using Altinn.Profile.Configuration;
using Altinn.Profile.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.Profile.Services.Implementation
{
    /// <summary>
    /// Represents an implementation of <see cref="IUserProfiles"/> using SBLBridge to obtain profile information.
    /// </summary>
    public class UserProfilesWrapper : IUserProfiles
    {
        private readonly ILogger _logger;
        private readonly GeneralSettings _generalSettings;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _serializerOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfilesWrapper"/> class
        /// </summary>
        /// <param name="httpClient">HttpClient from default http client factory</param>
        /// <param name="logger">the logger</param>
        /// <param name="generalSettings">the general settings</param>
        public UserProfilesWrapper(
            HttpClient httpClient,
            ILogger<UserProfilesWrapper> logger,
            IOptions<GeneralSettings> generalSettings)
        {
            _logger = logger;
            _generalSettings = generalSettings.Value;
            _client = httpClient;

            _serializerOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            };
        }

        /// <inheritdoc />
        public async Task<UserProfile> GetUser(int userId)
        {
            UserProfile user;

            Uri endpointUrl = new Uri($"{_generalSettings.BridgeApiEndpoint}users/{userId}");

            HttpResponseMessage response = await _client.GetAsync(endpointUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Getting user {userId} failed with {statusCode}", userId, response.StatusCode);
                return null;
            }

            string content = await response.Content.ReadAsStringAsync();
            user = JsonSerializer.Deserialize<UserProfile>(content, _serializerOptions);

            return user;
        }

        /// <inheritdoc />
        public async Task<UserProfile> GetUser(string ssn)
        {
            UserProfile user;
            Uri endpointUrl = new Uri($"{_generalSettings.BridgeApiEndpoint}users");
            StringContent requestBody = new StringContent(JsonSerializer.Serialize(ssn), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync(endpointUrl, requestBody);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Getting user by SSN failed with statuscode {statusCode}", response.StatusCode);
                return null;
            }

            string content = await response.Content.ReadAsStringAsync();
            user = JsonSerializer.Deserialize<UserProfile>(content, _serializerOptions);

            return user;
        }

        /// <inheritdoc />
        public async Task<UserProfile> GetUserByUuid(Guid userUuid)
        {
            Uri endpointUrl = new Uri($"{_generalSettings.BridgeApiEndpoint}users?useruuid={userUuid}");

            HttpResponseMessage response = await _client.GetAsync(endpointUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Getting user {userUuid} failed with {statusCode}", userUuid, response.StatusCode);
                return null;
            }

            string content = await response.Content.ReadAsStringAsync();
            UserProfile user = JsonSerializer.Deserialize<UserProfile>(content, _serializerOptions);

            return user;
        }

        /// <inheritdoc />
        public async Task<List<UserProfile>> GetUserListByUuid(List<Guid> userUuidList)
        {
            Uri endpointUrl = new Uri($"{_generalSettings.BridgeApiEndpoint}users/byuuid");
            StringContent requestBody = new StringContent(JsonSerializer.Serialize(userUuidList), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync(endpointUrl, requestBody);

            if (!response.IsSuccessStatusCode)
            {
                string userUuidListString = string.Join(", ", userUuidList);
                _logger.LogError("Getting users {userUuidListString} failed with {statusCode}", userUuidListString, response.StatusCode);
                return new List<UserProfile>();
            }

            string content = await response.Content.ReadAsStringAsync();
            List<UserProfile> users = JsonSerializer.Deserialize<List<UserProfile>>(content, _serializerOptions);

            return users;
        }

        /// <inheritdoc />
        public async Task<UserProfile> GetUserByUsername(string username)
        {
            UserProfile user;

            Uri endpointUrl = new Uri($"{_generalSettings.BridgeApiEndpoint}users/?username={username}");

            HttpResponseMessage response = await _client.GetAsync(endpointUrl);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Getting user {username} failed with {statusCode}", username, response.StatusCode);
                return null;
            }

            string content = await response.Content.ReadAsStringAsync();
            user = JsonSerializer.Deserialize<UserProfile>(content, _serializerOptions);

            return user;
        }
    }
}
