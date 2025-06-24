using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text.Json;
using Altinn.Authorization.ProblemDetails;
using Altinn.Register.Configuration;
using Altinn.Register.Core.Errors;
using Altinn.Register.Extensions;
using Altinn.Register.Services.Interfaces;
using AltinnCore.Authentication.Utils;
using Microsoft.Extensions.Options;

namespace Altinn.Profile.Integrations.Authorization;

    /// <summary>
    /// App implementation of the authorization service where the app uses the Altinn platform api.
    /// </summary>
    internal sealed partial class AuthorizationClient
        : IAuthorizationClient
    {
        private static readonly JsonSerializerOptions _options = JsonSerializerOptions.Web;

        private readonly GeneralSettings _generalSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _authClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationClient"/> class
        /// </summary>
        /// <param name="generalSettings">the general settings</param>
        /// <param name="platformSettings">The platform settings from configuration.</param>
        /// <param name="httpContextAccessor">the http context accessor.</param>
        /// <param name="httpClient">A Http client from the HttpClientFactory.</param>
        /// <param name="logger">the handler for logger service</param>
        public AuthorizationClient(
            IOptions<GeneralSettings> generalSettings,
            IOptions<PlatformSettings> platformSettings,
            IHttpContextAccessor httpContextAccessor,
            HttpClient httpClient)
        {
            _generalSettings = generalSettings.Value;
            _httpContextAccessor = httpContextAccessor;
            httpClient.BaseAddress = new Uri(platformSettings.Value.ApiAuthorizationEndpoint);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _authClient = httpClient;
        }

        /// <inheritdoc />
        public async Task<Result<bool>> ValidateSelectedParty(int userId, int partyId, CancellationToken cancellationToken = default)
        {
            string apiUrl = $"parties/{partyId}/validate?userid={userId}";
            string token = JwtTokenUtil.GetTokenFromContext(_httpContextAccessor.HttpContext, _generalSettings.JwtCookieName);

            HttpResponseMessage response = await _authClient.GetAsync(token, apiUrl, cancellationToken: cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                bool result;
                try
                {
                    result = await response.Content.ReadFromJsonAsync<bool>(_options, cancellationToken);
                }
                catch (JsonException)
                {
                    Log.ValidatingSelectedPartyFailed(_logger, partyId, userId, response.StatusCode);
                    return Problems.PartyAuthorizeFailed.Create([
                        new("reason", "Bad JSON response from A2"),
                ]);
                }

                Log.ValidatingSelectedPartySucceeded(_logger, partyId, userId, result);
                return result;
            }
            else
            {
                Log.ValidatingSelectedPartyFailed(_logger, partyId, userId, response.StatusCode);
                return Problems.PartyAuthorizeFailed.Create([
                    new("reason", $"Http status code {response.StatusCode} from A2"),
            ]);
            }
        }
    }
}
