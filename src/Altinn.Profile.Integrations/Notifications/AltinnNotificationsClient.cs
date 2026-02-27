using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Altinn.Common.AccessTokenClient.Services;
using Altinn.Profile.Core.Integrations;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.Profile.Integrations.Notifications;

/// <summary>
/// A content-agnostic HTTP client for interacting with the Altinn notifications service.
/// Responsible only for HTTP transport; callers build message content.
/// </summary>
public class AltinnNotificationsClient : INotificationsClient
{
    private readonly HttpClient _httpClient;
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly ILogger<AltinnNotificationsClient> _logger;
    private const string _notificationTypeSms = "sms";
    private const string _notificationTypeEmail = "email";

    private static readonly JsonSerializerOptions _options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnNotificationsClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to make requests to the Notifications service.</param>
    /// <param name="settings">The Notifications settings containing the API endpoint.</param>
    /// <param name="accessTokenGenerator">The access token generator.</param>
    /// <param name="logger">The logger</param>
    public AltinnNotificationsClient(HttpClient httpClient, IOptions<NotificationsSettings> settings, IAccessTokenGenerator accessTokenGenerator, ILogger<AltinnNotificationsClient> logger)
    {
        _httpClient = httpClient;
        _accessTokenGenerator = accessTokenGenerator;
        _httpClient.BaseAddress = new Uri(settings.Value.ApiNotificationsEndpoint);
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task OrderSmsAsync(string phoneNumber, string body, string? sendersReference, CancellationToken cancellationToken)
    {
        var request = new SmsOrderRequest
        {
            IdempotencyId = Guid.NewGuid().ToString(),
            SendersReference = sendersReference,
            RecipientSms = new RecipientSms
            {
                PhoneNumber = phoneNumber,
                SmsSettings = new SmsSettings
                {
                    Body = body,
                }
            }
        };

        var json = JsonSerializer.Serialize(request, _options);
        await SendOrder(json, _notificationTypeSms, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task OrderEmailAsync(string emailAddress, string subject, string body, string? sendersReference, CancellationToken cancellationToken)
    {
        var request = new EmailOrderRequest
        {
            IdempotencyId = Guid.NewGuid().ToString(),
            SendersReference = sendersReference,
            RecipientEmail = new RecipientEmail
            {
                EmailAddress = emailAddress,
                EmailSettings = new EmailSettings
                {
                    Subject = subject,
                    Body = body,
                }
            }
        };

        var json = JsonSerializer.Serialize(request, _options);
        await SendOrder(json, _notificationTypeEmail, cancellationToken);
    }

    private async Task SendOrder(string jsonString, string type, CancellationToken cancellationToken)
    {
        var accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "profile");
        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogError("Invalid access token generated for notification order.");
            return;
        }

        var stringContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"v1/future/orders/instant/{type}")
        {
            Content = stringContent
        };

        requestMessage.Headers.Add("PlatformAccessToken", accessToken);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("Failed to send order request. Status code: {StatusCode}, Response: {ResponseContent}", response.StatusCode, responseContent);
        }
    }
}
