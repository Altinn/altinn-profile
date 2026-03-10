using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.Unit.ContactPoints;
using Altinn.Register.Contracts;

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
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
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
    public async Task<IReadOnlyList<Organization>?> GetPartyUuids(string[] orgNumbers, CancellationToken cancellationToken)
    {
        string[] identifiers = [.. orgNumbers.Where(o => !string.IsNullOrWhiteSpace(o)).Select(o => $"urn:altinn:organization:identifier-no:{o}")];
        var request = new QueryPartiesRequest(identifiers);

        var response = await QueryParties(request, "fields=id,uuid,org-id", cancellationToken: cancellationToken);
        if (response == null)
        {
            return null;
        }

        // We use another response type here since the contract for this method only requires party id, party uuid and organization identifier, and we want to avoid deserializing unnecessary data
        var responseObject = await response.Content.ReadFromJsonAsync<QueryPartiesResponse>(cancellationToken);

        return responseObject?.Data?
            .Where(p => p.Type == PartyType.Organization)
            .OfType<Organization>()
            .ToList();
    }

    /// <inheritdoc/>
    public async Task<string?> GetMainUnit(string orgNumber, CancellationToken cancellationToken)
    {
        var request = new LookupMainUnitRequest(orgNumber);

        var response = await SendRequest(HttpMethod.Post, "v2/internal/parties/main-units", request, cancellationToken);

        if (response == null)
        {
            return null;
        }

        var responseObject = await response.Content.ReadFromJsonAsync<LookupMainUnitResponse>(cancellationToken);
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
        var responseData = await GetPartyIdentifiers(partyUuid, cancellationToken);

        if (responseData is null or { Count: 0 })
        {
            return null;
        }

        if (responseData.Count > 1)
        {
            _logger.LogError("Get party identifiers returned multiple results. Using the first one.");
        }

        return responseData[0].PartyId;
    }

    /// <inheritdoc/>
    public async Task<string?> GetOrganizationNumberByPartyUuid(Guid partyUuid, CancellationToken cancellationToken)
    {
        var responseData = await GetPartyIdentifiers(partyUuid, cancellationToken);
        
        if (responseData is null or { Count: 0 })
        {
            return null;
        }

        if (responseData.Count > 1)
        {
            _logger.LogError("Get party identifiers returned multiple results. Using the first one.");
        }

        return responseData[0].OrgNumber;
    }

    /// <inheritdoc/>
    public async Task<Party?> GetUserParty(Guid userUuid, CancellationToken cancellationToken)
    {
        var identifiers = new[] { $"urn:altinn:party:uuid:{userUuid}" };
        var parties = await GetUserParties(identifiers, cancellationToken);
        return parties.FirstOrDefault();
    }

    /// <inheritdoc/>
    public async Task<Party?> GetUserParty(int userId, CancellationToken cancellationToken)
    {
        var identifiers = new[] { $"urn:altinn:user:id:{userId}" };
        var parties = await GetUserParties(identifiers, cancellationToken);
        return parties.FirstOrDefault();
    }

    /// <inheritdoc/>
    public async Task<Party?> GetUserPartyByUsername(string username, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(username);

        var identifiers = new[] { $"urn:altinn:party:username:{username}" };
        var parties = await GetUserParties(identifiers, cancellationToken);
        return parties.FirstOrDefault();
    }

    /// <inheritdoc/>
    public async Task<Party?> GetUserPartyBySsn(string ssn, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(ssn);

        var identifiers = new[] { $"urn:altinn:person:identifier-no:{ssn}" };
        var parties = await GetUserParties(identifiers, cancellationToken);
        return parties.FirstOrDefault();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Party>> GetUserParties(List<Guid> userUuids, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(userUuids);
        if (userUuids.Count == 0)
        {
            throw new ArgumentException("The list of user UUIDs cannot be empty.", nameof(userUuids));
        }

        var identifiers = userUuids.Select(uuid => $"urn:altinn:party:uuid:{uuid}").ToArray();
        return [.. await GetUserParties(identifiers, cancellationToken)];
    }

    private async Task<IEnumerable<Party>> GetUserParties(string[] urns, CancellationToken cancellationToken)
    {
        var request = new QueryPartiesRequest(urns);
        var response = await QueryParties(request, "fields=person,party,user,si", cancellationToken: cancellationToken);

        if (response == null)
        {
            throw new PartyNotFoundException("No response from Register when looking up parties for user(s)");
        }

        var responseObject = await response.Content.ReadFromJsonAsync<QueryPartiesResponse>(cancellationToken);
        var data = responseObject?.Data;

        if (data is null or { Count: 0 })
        {
            throw new PartyNotFoundException("Empty response from Register when looking up parties for user(s)");
        }

        return data.Where(p => p.Type == PartyType.Person || p.Type == PartyType.SelfIdentifiedUser);
    }

    private async Task<HttpResponseMessage?> QueryParties(QueryPartiesRequest request, string queryParams = "", CancellationToken cancellationToken = default)
    {
        var requestUri = "v2/internal/parties/query";
        if (!string.IsNullOrEmpty(queryParams))
        {
            requestUri += $"?{queryParams}";
        }

        var response = await SendRequest(HttpMethod.Post, requestUri, request, cancellationToken);
        return response;
    }

    private async Task<HttpResponseMessage?> SendRequest(HttpMethod method, string path, object request, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(request, _options);
        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage(method, path)
        {
            Content = stringContent
        };

        var success = TryAddPlatformAccessTokenHeader(requestMessage);
        if (!success)
        {
            return null;
        }

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to call register. Status code: {StatusCode}", response.StatusCode);
            return null;
        }

        return response;
    }

    private bool TryAddPlatformAccessTokenHeader(HttpRequestMessage requestMessage)
    {
        var accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "profile");
        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogError("Invalid platform access token generated.");
            return false;
        }

        requestMessage.Headers.Add("PlatformAccessToken", accessToken);

        return true;
    }

    private async Task<List<PartyIdentifiersResponse>?> GetPartyIdentifiers(Guid partyUuid, CancellationToken cancellationToken)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Get, $"v1/parties/identifiers?uuids={partyUuid}");

        var success = TryAddPlatformAccessTokenHeader(requestMessage);
        if (!success)
        {
            return null;
        }

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to get party identifiers for party. Status code: {StatusCode}", response.StatusCode);
            return null;
        }

        var responseData = await response.Content.ReadFromJsonAsync<List<PartyIdentifiersResponse>>(cancellationToken);        

        return responseData;        
    }
}
