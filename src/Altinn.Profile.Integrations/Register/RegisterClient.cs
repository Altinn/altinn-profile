using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Common.AccessTokenClient.Services;

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
    public RegisterClient(HttpClient httpClient, IOptions<RegisterSettings> settings, IAccessTokenGenerator accessTokenGenerator)
    {
        _httpClient = httpClient;
        _accessTokenGenerator = accessTokenGenerator;
        _httpClient.BaseAddress = new Uri(settings.Value.ApiRegisterEndpoint);

    }

    /// <inheritdoc/>
    public async Task<string?> GetMainUnit(string orgNumber, CancellationToken cancellationToken)
    {
        var request = new LookupMainUnitRequest(orgNumber);
        var json = JsonSerializer.Serialize(request, _options);
        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "v2/internal/parties/main-units")
        {
            Content = stringContent
        };

        var accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "profile");
        if (!string.IsNullOrEmpty(accessToken))
        {
            requestMessage.Headers.Add("PlatformAccessToken", accessToken);
        }

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new NotImplementedException($"Failed to retrieve main unit. StatusCode: {response.StatusCode}");
        }

        var responseData = await response.Content.ReadAsStringAsync(cancellationToken);

        var responseObject = JsonSerializer.Deserialize<LookupMainUnitResponse>(responseData);

        if (responseObject == null)
        {
            throw new NotImplementedException("Failed to deserialize response from Register API.");
        }

        if (!(responseObject.Data?.Count > 0))
        {
            return null;
        }

        // The response is a list, but assuming the list contains only one item in all cases
        var mainUnitOrgNumber = responseObject.Data[0].OrganizationIdentifier;
        return mainUnitOrgNumber;
    }
}
