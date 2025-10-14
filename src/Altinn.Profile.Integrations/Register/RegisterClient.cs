using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.Profile.Core.Integrations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.Profile.Integrations.Register;

/// <summary>
/// An HTTP client to interact with a source registry for organizational notification addresses.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="IRegisterClient"/> class.
/// </remarks>
public class RegisterClient : IRegisterClient
{
    private readonly HttpClient _httpClient;
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly ILogger<RegisterClient> _logger;

    private readonly JsonSerializerOptions _options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to make requests to the register service.</param>
    /// <param name="settings">The register settings containing the API endpoint.</param>
    /// <param name="accessTokenGenerator">The access token generator.</param>
    /// <param name="logger">The logger</param>
    public RegisterClient(HttpClient httpClient, IOptions<RegisterSettings> settings, IAccessTokenGenerator accessTokenGenerator, ILogger<RegisterClient> logger)
    {
        _httpClient = httpClient;
        _accessTokenGenerator = accessTokenGenerator;
        _httpClient.BaseAddress = new Uri(settings.Value.ApiRegisterEndpoint);
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<string?> GetPartyUuids(string[] orgNumbers, CancellationToken cancellationToken)
    {
        var accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "profile");
        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogError("Invalid access token generated for org main unit lookup.");
            return null;
        }

        var request = new QueryPartiesRequest(orgNumbers);
        var json = JsonSerializer.Serialize(request, _options);
        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "v2/internal/parties/query")
        {
            Content = stringContent
        };

        requestMessage.Headers.Add("PlatformAccessToken", accessToken);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to get main unit for organization. Status code: {StatusCode}", response.StatusCode);
            return null;
        }

        var responseData = await response.Content.ReadAsStringAsync(cancellationToken);

        var responseObject = JsonSerializer.Deserialize<QueryPartiesResponse>(responseData);
        if (!(responseObject?.Data?.Count > 0))
        {
            return null;
        }

        // The response is a list, but assuming the list contains only one item in all cases
        if (responseObject.Data.Count > 1)
        {
            _logger.LogWarning("Get main units for organization returned multiple results. Using the first one.");
        }

        var mainUnitOrgNumber = responseObject.Data[0].OrganizationIdentifier;
        return mainUnitOrgNumber;
    }

    /// <inheritdoc/>
    public async Task<string?> GetMainUnit(string orgNumber, CancellationToken cancellationToken)
    {
        var accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "profile");
        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogError("Invalid access token generated for org main unit lookup.");
            return null;
        }

        var request = new LookupMainUnitRequest(orgNumber);
        var json = JsonSerializer.Serialize(request, _options);
        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "v2/internal/parties/main-units")
        {
            Content = stringContent
        };

        requestMessage.Headers.Add("PlatformAccessToken", accessToken);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to get main unit for organization. Status code: {StatusCode}", response.StatusCode);
            return null;
        }

        var responseData = await response.Content.ReadAsStringAsync(cancellationToken);

        var responseObject = JsonSerializer.Deserialize<LookupMainUnitResponse>(responseData);
        if (!(responseObject?.Data?.Count > 0))
        {
            return null;
        }

        // The response is a list, but assuming the list contains only one item in all cases
        if (responseObject.Data.Count > 1)
        {
            _logger.LogWarning("Get main units for organization returned multiple results. Using the first one.");
        }

        var mainUnitOrgNumber = responseObject.Data[0].OrganizationIdentifier;
        return mainUnitOrgNumber;
    }

    /// <inheritdoc/>
    public async Task<int?> GetPartyId(Guid partyUuid, CancellationToken cancellationToken)
    {
        var accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "profile");
        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogError("Invalid access token generated for party ID lookup.");
            return null;
        }

        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"v1/parties/identifiers?uuids={partyUuid}");

        requestMessage.Headers.Add("PlatformAccessToken", accessToken);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to get partyId for party. Status code: {StatusCode}", response.StatusCode);
            return null;
        }

        var responseData = await response.Content.ReadFromJsonAsync<List<PartyIdentifiersResponse>>(cancellationToken);

        if (responseData is null or { Count: 0 })
        {
            return null;
        }

        // The response is a list, but assuming the list contains only one item in all cases
        if (responseData.Count > 1)
        {
            _logger.LogWarning("Get partyId for party returned multiple results. Using the first one.");
        }

        return responseData[0].PartyId;
    }
}
