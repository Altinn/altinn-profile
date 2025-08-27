using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true) }
        };
    }

    /// <inheritdoc/>
    public async Task<ChangeLog?> GetChangeLog(DateTime changeDate, DataType dataType, CancellationToken cancellationToken)
    {
        var utc = ConvertToLocal(changeDate);

        string endpoint = $"profilechangelog?fromTimestamp={utc:yyyy-MM-ddTHH\\:mm\\:ss.fffffffZ}&dataType={dataType}";
        using HttpResponseMessage response = await _client.GetAsync(endpoint, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode is HttpStatusCode.InternalServerError or HttpStatusCode.ServiceUnavailable)
            {
                throw new InternalServerErrorException("Received error response while fetching changeLog.");
            }

            _logger.LogError(
                "// ChangeLogClient // GetChangeLog // Unexpected response. Failed with {StatusCode}",
                response.StatusCode);

            return null;
        }

        string content = await response.Content.ReadAsStringAsync(cancellationToken);
        ChangeLog changeLog = JsonSerializer.Deserialize<ChangeLog>(content, _serializerOptions)!;

        return changeLog;
    }

    private static DateTime ConvertToLocal(DateTime changeDate)
    {
        return changeDate.Kind switch
        {
            DateTimeKind.Utc => changeDate.ToLocalTime(),
            DateTimeKind.Local => changeDate,
            _ => DateTime.SpecifyKind(changeDate, DateTimeKind.Local)
        };
    }
}
