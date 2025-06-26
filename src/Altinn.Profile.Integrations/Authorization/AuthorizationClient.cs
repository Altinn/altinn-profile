using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Altinn.Common.PEP.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Altinn.Profile.Integrations.Authorization;

    /// <summary>
    /// App implementation of the authorization service where the app uses the Altinn platform api.
    /// </summary>
public class AuthorizationClient
    : IAuthorizationClient
{
    private static readonly JsonSerializerOptions _options = JsonSerializerOptions.Web;
    private readonly HttpClient _authClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthorizationClient> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizationClient"/> class
    /// </summary>
    /// <param name="platformSettings">The platform settings from configuration.</param>
    /// <param name="httpClient">A Http client from the HttpClientFactory.</param>
    /// <param name="httpContextAccessor">The http context accessor.</param>
    /// <param name="logger">The logger</param>
    public AuthorizationClient(
        IOptions<PlatformSettings> platformSettings,
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthorizationClient> logger)
    {
        httpClient.BaseAddress = new Uri(platformSettings.Value.ApiAuthorizationEndpoint);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _authClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> ValidateSelectedParty(int userId, int partyId, CancellationToken cancellationToken = default)
    {
        string apiPath = $"parties/{partyId}/validate?userid={userId}";
        HttpRequestMessage requestMessage = new(HttpMethod.Get, apiPath);

        var authorizationToken = _httpContextAccessor.HttpContext?.Request.Headers[HeaderNames.Authorization].ToString();
        if (string.IsNullOrEmpty(authorizationToken))
        {
            // If the authorization token is not present, we cannot proceed with the request.
            _logger.LogWarning("Authorization token is missing in the request headers.");
            return false;
        }

        requestMessage.Headers.Add("Authorization", authorizationToken);

        HttpResponseMessage response = await _authClient.SendAsync(requestMessage, cancellationToken: cancellationToken);

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
