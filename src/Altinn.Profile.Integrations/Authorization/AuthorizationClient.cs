using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Altinn.Common.PEP.Configuration;
using Microsoft.Extensions.Options;

namespace Altinn.Profile.Integrations.Authorization;

    /// <summary>
    /// App implementation of the authorization service where the app uses the Altinn platform api.
    /// </summary>
    public class AuthorizationClient
        : IAuthorizationClient
    {
        private static readonly JsonSerializerOptions _options = JsonSerializerOptions.Web;

        private readonly HttpClient _authClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationClient"/> class
        /// </summary>
        /// <param name="platformSettings">The platform settings from configuration.</param>
        /// <param name="httpClient">A Http client from the HttpClientFactory.</param>
        public AuthorizationClient(
            IOptions<PlatformSettings> platformSettings,
            HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri(platformSettings.Value.ApiAuthorizationEndpoint);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _authClient = httpClient;
        }

        /// <inheritdoc />
        public async Task<bool> ValidateSelectedParty(int userId, int partyId, CancellationToken cancellationToken = default)
        {
            string apiUrl = $"parties/{partyId}/validate?userid={userId}";

            HttpResponseMessage response = await _authClient.GetAsync(apiUrl, cancellationToken: cancellationToken);

            if (!response.IsSuccessStatusCode)
            { 
                return false; 
            }

            bool result;
            try
            {
                result = await response.Content.ReadFromJsonAsync<bool>(_options, cancellationToken);
            }
            catch (JsonException)
            {
                return false;
            }

            return result;
        }
    }
