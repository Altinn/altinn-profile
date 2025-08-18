using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Altinn.Profile.Integrations.SblBridge.Unit.Profile;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npgsql.Internal;

namespace Altinn.Profile.Integrations.SblBridge.Changelog;

/// <summary>
/// Using SBLBridge to get changes in A2
/// </summary>
public class ChangeLogClient : IChangeLogClient
{
    private readonly ILogger<ChangeLogClient> _logger;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _serializerOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeLogClient"/> class
    /// </summary>
    /// <param name="httpClient">HttpClient from default http client factory</param>
    /// <param name="logger">the logger</param>
    /// <param name="settings">the sbl bridge settings</param>
    public ChangeLogClient(
        HttpClient httpClient,
        ILogger<ChangeLogClient> logger,
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

    /// <inheritdoc/>
    public async Task<ChangeLog?> GetChangeLog(int changeId, DataType dataType)
    {
        string endpoint = $"profilechangelog?fromChangeId={changeId}&dataType={dataType}";

        HttpResponseMessage response = await _client.GetAsync(endpoint);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode is HttpStatusCode.InternalServerError or HttpStatusCode.ServiceUnavailable)
            {
                throw new InternalServerErrorException("Received error response while fetching changeLog.");
            }

            _logger.LogError(
                "// ChangeLogClient // GetChangeLog // Unexpected response. Failed with {StatusCode} and message {Message}",
                response.StatusCode,
                await response.Content.ReadAsStringAsync());

            return null;
        }

        string content = await response.Content.ReadAsStringAsync();
        ChangeLog changeLog = JsonSerializer.Deserialize<ChangeLog>(content, _serializerOptions)!;

        return changeLog;
    }
}
