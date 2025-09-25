using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.User.NotificationSettings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.SblBridge.User.NotificationSettings
{
    public class UserNotificationSettingsClientTests
    {
        private readonly Mock<IOptions<SblBridgeSettings>> _settingsMock;
        private readonly Mock<ILogger<UserNotificationSettingsClient>> _loggerMock;
        private HttpClient _httpClient;
        private const string _testBaseUrl = "https://api.test.local/";

        public UserNotificationSettingsClientTests()
        {
            _settingsMock = new Mock<IOptions<SblBridgeSettings>>();
            _settingsMock.Setup(s => s.Value).Returns(new SblBridgeSettings { ApiProfileEndpoint = _testBaseUrl });

            _loggerMock = new Mock<ILogger<UserNotificationSettingsClient>>();
        }

        private static Mock<HttpMessageHandler> CreateHandler(
            HttpResponseMessage response,
            Action<HttpRequestMessage> requestCallback = null,
            Action<CancellationToken> cancelCallback = null)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) =>
                {
                    requestCallback?.Invoke(req);
                    cancelCallback?.Invoke(ct);
                    return response;
                });
            return handlerMock;
        }

        [Fact]
        public void Constructor_BaseAddressIsSetFromSettings()
        {
            var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK));
            _httpClient = new HttpClient(handler.Object);
            var client = new UserNotificationSettingsClient(_httpClient, _loggerMock.Object, _settingsMock.Object);
            Assert.Equal(new Uri(_testBaseUrl), _httpClient.BaseAddress);
        }

        [Fact]
        public async Task UpdateNotificationSettings_SuccessfulRequest_DoesNotLogError()
        {
            // Arrange
            var request = new NotificationSettingsChangedRequest
            {
                ChangeType = "insert",
                ChangeDateTime = DateTime.UtcNow,
                UserId = 123,
                PartyUuid = Guid.NewGuid(),
                PhoneNumber = "+4712345678",
                Email = "user@example.com",
                ServiceNotificationOptions = new[] { "option1", "option2" }
            };
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            HttpRequestMessage sentRequest = null;
            var handler = CreateHandler(response, req => sentRequest = req);
            _httpClient = new HttpClient(handler.Object);
            var client = new UserNotificationSettingsClient(_httpClient, _loggerMock.Object, _settingsMock.Object);

            // Act
            await client.UpdateNotificationSettings(request);

            // Assert
            Assert.NotNull(sentRequest);
            Assert.Equal(HttpMethod.Post, sentRequest.Method);
            Assert.Equal(new Uri(_testBaseUrl + "users/reporteenotificationendpoint/update"), sentRequest.RequestUri);
            Assert.IsType<StringContent>(sentRequest.Content);
            var requestContent = await sentRequest.Content.ReadAsStringAsync();
            var sentPayload = JsonSerializer.Deserialize<NotificationSettingsChangedRequest>(requestContent);
            Assert.Equal(request.ChangeType, sentPayload.ChangeType);
            Assert.Equal(request.ChangeDateTime, sentPayload.ChangeDateTime, TimeSpan.FromSeconds(1));
            Assert.Equal(request.UserId, sentPayload.UserId);
            Assert.Equal(request.PartyUuid, sentPayload.PartyUuid);
            Assert.Equal(request.PhoneNumber, sentPayload.PhoneNumber);
            Assert.Equal(request.Email, sentPayload.Email);
            Assert.Equal(request.ServiceNotificationOptions, sentPayload.ServiceNotificationOptions);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateNotificationSettings_WhenResponseIsNotSuccess_LogsError()
        {
            // Arrange
            var request = new NotificationSettingsChangedRequest
            {
                ChangeType = "delete",
                ChangeDateTime = DateTime.UtcNow,
                UserId = 456,
                PartyUuid = Guid.NewGuid(),
                PhoneNumber = "+4798765432",
                Email = "other@example.com",
                ServiceNotificationOptions = new[] { "optionA" }
            };
            var errorMessage = "Something went wrong";
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(errorMessage, Encoding.UTF8, "text/plain")
            };
            var handler = CreateHandler(response);
            _httpClient = new HttpClient(handler.Object);
            var client = new UserNotificationSettingsClient(_httpClient, _loggerMock.Object, _settingsMock.Object);

            // Act
            await client.UpdateNotificationSettings(request);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unexpected response")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateNotificationSettings_WhenResponseIsInternalServerError_ThrowsException()
        {
            // Arrange
            var request = new NotificationSettingsChangedRequest
            {
                ChangeType = "insert",
                ChangeDateTime = DateTime.UtcNow,
                UserId = 789,
                PartyUuid = Guid.NewGuid(),
                PhoneNumber = null,
                Email = null,
                ServiceNotificationOptions = null
            };
            var errorMessage = "Internal server error";
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(errorMessage, Encoding.UTF8, "text/plain")
            };
            var handler = CreateHandler(response);
            _httpClient = new HttpClient(handler.Object);
            var client = new UserNotificationSettingsClient(_httpClient, _loggerMock.Object, _settingsMock.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InternalServerErrorException>(() => client.UpdateNotificationSettings(request));

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unexpected response")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }
    }
}
