using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.Profile.Core.Integrations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Altinn.Profile.Integrations.Notifications;

/// <summary>
/// An HTTP client to interact with the Altinn notifications service.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="NotificationsClient"/> class.
/// </remarks>
public class NotificationsClient : INotificationsClient
{
    private readonly HttpClient _httpClient;
    private readonly IAccessTokenGenerator _accessTokenGenerator;
    private readonly ILogger<NotificationsClient> _logger;

    private readonly JsonSerializerOptions _options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationsClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to make requests to the Notifications service.</param>
    /// <param name="settings">The Notifications settings containing the API endpoint.</param>
    /// <param name="accessTokenGenerator">The access token generator.</param>
    /// <param name="logger">The logger</param>
    public NotificationsClient(HttpClient httpClient, IOptions<NotificationsSettings> settings, IAccessTokenGenerator accessTokenGenerator, ILogger<NotificationsClient> logger)
    {
        _httpClient = httpClient;
        _accessTokenGenerator = accessTokenGenerator;
        _httpClient.BaseAddress = new Uri(settings.Value.ApiNotificationsEndpoint);
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task OrderSms(string phoneNumber, string languageCode, CancellationToken cancellationToken)
    {
        var request = new SmsOrderRequest
        {
            IdempotencyId = Guid.NewGuid().ToString(),
            Recipient = new Recipient
            {
                RecipientSms = new RecipientSms
                {
                    PhoneNumber = phoneNumber,
                    SmsSettings = new SmsSettings
                    {
                        Body = OrderContent.GetSmsContent(languageCode),
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(request, _options);

        await SendOrder(json, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task OrderEmail(string emailAddress, string languageCode, CancellationToken cancellationToken)
    {
        var request = new EmailOrderRequest
        {
            IdempotencyId = Guid.NewGuid().ToString(),
            Recipient = new EmailRecipient
            {
                RecipientEmail = new RecipientEmail
                {
                    EmailAddress = emailAddress,
                    EmailSettings = new EmailSettings
                    {
                        Subject = OrderContent.GetEmailSubject(languageCode),
                        Body = OrderContent.GetTmpEmailBody(languageCode),
                    }
                }
            },
        };

        var json = JsonSerializer.Serialize(request, _options);
        await SendOrder(json, cancellationToken);
    }

    private async Task SendOrder(string jsonString, CancellationToken cancellationToken)
    {
        var accessToken = _accessTokenGenerator.GenerateAccessToken("platform", "profile");
        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogError("Invalid access token generated for notification order.");
            return;
        }

        var stringContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "v1/future/orders")
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
