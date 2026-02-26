using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Common.AccessTokenClient.Services;
using Altinn.Profile.Integrations.Notifications;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Moq;
using Moq.Protected;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Notifications
{
    public class AltinnNotificationsClientTests
    {
        private readonly Mock<IOptions<NotificationsSettings>> _settingsMock;
        private readonly Mock<IAccessTokenGenerator> _tokenGenMock;
        private readonly Mock<ILogger<AltinnNotificationsClient>> _loggerMock;
        private HttpClient _httpClient;
        private const string _testBaseUrl = "https://notifications.test/";

        public AltinnNotificationsClientTests()
        {
            _settingsMock = new Mock<IOptions<NotificationsSettings>>();
            _settingsMock.Setup(s => s.Value).Returns(new NotificationsSettings { ApiNotificationsEndpoint = _testBaseUrl });

            _tokenGenMock = new Mock<IAccessTokenGenerator>();
            _tokenGenMock.Setup(t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>()))
                         .Returns("token");

            _loggerMock = new Mock<ILogger<AltinnNotificationsClient>>();
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
        public async Task OrderSms_SendsCorrectRequest()
        {
            // Arrange
            HttpRequestMessage sentRequest = null;
            var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK), req => sentRequest = req);
            _httpClient = new HttpClient(handler.Object);
            var client = new AltinnNotificationsClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);
            var smsBody = "Test SMS body content";
            var sendersReference = Guid.NewGuid().ToString();

            // Act
            await client.OrderSms("+4799999999", smsBody, sendersReference, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(sentRequest);
            Assert.Equal(HttpMethod.Post, sentRequest.Method);
            Assert.Equal(new Uri(_testBaseUrl + "v1/future/orders/instant/sms"), sentRequest.RequestUri);
            Assert.True(sentRequest.Headers.Contains("PlatformAccessToken"));
            Assert.IsType<StringContent>(sentRequest.Content);
            var content = await sentRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Contains("4799999999", content);
            Assert.Contains(smsBody, content);
            Assert.Contains(sendersReference, content);
        }

        [Fact]
        public async Task OrderEmail_SendsCorrectRequest()
        {
            // Arrange
            HttpRequestMessage sentRequest = null;
            var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK), req => sentRequest = req);
            _httpClient = new HttpClient(handler.Object);
            var client = new AltinnNotificationsClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);
            var emailSubject = "Test subject";
            var emailBody = "Test email body content";
            var sendersReference = Guid.NewGuid().ToString();

            // Act
            await client.OrderEmail("test@example.com", emailSubject, emailBody, sendersReference, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(sentRequest);
            Assert.Equal(HttpMethod.Post, sentRequest.Method);
            Assert.Equal(new Uri(_testBaseUrl + "v1/future/orders/instant/email"), sentRequest.RequestUri);
            Assert.True(sentRequest.Headers.Contains("PlatformAccessToken"));
            Assert.IsType<StringContent>(sentRequest.Content);
            var content = await sentRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Contains("test@example.com", content);
            Assert.Contains(emailSubject, content);
            Assert.Contains(emailBody, content);
            Assert.Contains(sendersReference, content);
        }

        [Fact]
        public async Task OrderSms_InvalidAccessToken_LogsErrorAndDoesNotSend()
        {
            // Arrange
            var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK));
            _httpClient = new HttpClient(handler.Object);
            _tokenGenMock.Setup(t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>()))
                         .Returns(string.Empty);
            var client = new AltinnNotificationsClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            await client.OrderSms("+4799999999", "body", "ref", TestContext.Current.CancellationToken);

            // Assert
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid access token")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // No HTTP request should be sent
            handler.Protected().Verify("SendAsync", Times.Never(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task OrderSms_UnsuccessfulResponse_LogsError()
        {
            // Arrange
            var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.BadRequest));
            _httpClient = new HttpClient(handler.Object);
            var client = new AltinnNotificationsClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            await client.OrderSms("+4799999999", "body", "ref", TestContext.Current.CancellationToken);

            // Assert
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to send order request")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task OrderSms_NullSendersReference_SendsCorrectRequest()
        {
            // Arrange
            HttpRequestMessage sentRequest = null;
            var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK), req => sentRequest = req);
            _httpClient = new HttpClient(handler.Object);
            var client = new AltinnNotificationsClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            await client.OrderSms("+4799999999", "body", null, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(sentRequest);
            Assert.Equal(HttpMethod.Post, sentRequest.Method);
            var content = await sentRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Contains("4799999999", content);
            Assert.Contains("body", content);
        }

        [Fact]
        public async Task OrderEmail_InvalidAccessToken_LogsErrorAndDoesNotSend()
        {
            // Arrange
            var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK));
            _httpClient = new HttpClient(handler.Object);
            _tokenGenMock.Setup(t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>()))
                         .Returns(string.Empty);
            var client = new AltinnNotificationsClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            await client.OrderEmail("test@example.com", "subject", "body", "ref", TestContext.Current.CancellationToken);

            // Assert
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Invalid access token")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            handler.Protected().Verify("SendAsync", Times.Never(), ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task OrderEmail_UnsuccessfulResponse_LogsError()
        {
            // Arrange
            var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.BadRequest));
            _httpClient = new HttpClient(handler.Object);
            var client = new AltinnNotificationsClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            await client.OrderEmail("test@example.com", "subject", "body", "ref", TestContext.Current.CancellationToken);

            // Assert
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to send order request")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
