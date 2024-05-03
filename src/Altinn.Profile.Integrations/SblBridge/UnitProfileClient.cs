using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Altinn.Profile.Core;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.Unit.ContactPoints;
using Altinn.Profile.Core.User.ContactPoints;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.Profile.Integrations.SblBridge;

/// <summary>
/// Represents an implementation of <see cref="IUnitContactPointClient"/> using SBLBridge to obtain unit profile information.
/// </summary>
public class UnitProfileClient : IUnitProfileClient
{
    private readonly ILogger<UnitProfileClient> _logger;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _serializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="UnitProfileClient"/> class
    /// </summary>
    /// <param name="httpClient">HttpClient from default http client factory</param>
    /// <param name="logger">the logger</param>
    /// <param name="settings">the sbl bridge settings</param>
    public UnitProfileClient(
        HttpClient httpClient,
        ILogger<UnitProfileClient> logger,
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
    public async Task<Result<UnitContactPointsList, bool>> GetUserRegisteredContactPoints(UnitContactPointLookup lookup)
    {
        string endpoint = $"units/contactpointslookup";

        StringContent requestBody = new StringContent(JsonSerializer.Serialize(lookup), Encoding.UTF8, "application/json");

        HttpResponseMessage response = await _client.PostAsync(endpoint, requestBody);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("// UnitClient // GetUserRegisteredContactPoints // An error occured when retrieving unit contact points. Failed with {statusCode}", response.StatusCode);
            return false;
        }

        string content = await response.Content.ReadAsStringAsync();
        List<PartyNotificationContactPoints> partyNotificationEndpoints = JsonSerializer.Deserialize<List<PartyNotificationContactPoints>>(content, _serializerOptions)!;

        List<UnitContactPoints> contactPoints = partyNotificationEndpoints.Select(partyNotificationEndpoint => new UnitContactPoints
        {
            OrganizationNumber = partyNotificationEndpoint.OrganizationNumber,
            PartyId = partyNotificationEndpoint.LegacyPartyId,
            UserContactPoints = partyNotificationEndpoint.ContactPoints.Select(contactPoint => new UserContactPoints
            {
                UserId = contactPoint.LegacyUserId,
                Email = contactPoint.Email,
                MobileNumber = contactPoint.MobileNumber
            }).ToList()
        }).ToList();

        return new UnitContactPointsList() { ContactPointsList = contactPoints };
    }
}
