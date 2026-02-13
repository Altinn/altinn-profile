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
    public class NotificationsClientTests
    {
        private readonly Mock<IOptions<NotificationsSettings>> _settingsMock;
        private readonly Mock<IAccessTokenGenerator> _tokenGenMock;
        private readonly Mock<ILogger<NotificationsClient>> _loggerMock;
        private HttpClient _httpClient;
        private const string _testBaseUrl = "https://notifications.test/";

        public NotificationsClientTests()
        {
            _settingsMock = new Mock<IOptions<NotificationsSettings>>();
            _settingsMock.Setup(s => s.Value).Returns(new NotificationsSettings { ApiNotificationsEndpoint = _testBaseUrl });

            _tokenGenMock = new Mock<IAccessTokenGenerator>();
            _tokenGenMock.Setup(t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>()))
                         .Returns("token");

            _loggerMock = new Mock<ILogger<NotificationsClient>>();
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
        public async Task SendSmsOrder_WhenLanguageNb_SendsCorrectRequest()
        {
            // Arrange
            HttpRequestMessage sentRequest = null;
            var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK), req => sentRequest = req);
            _httpClient = new HttpClient(handler.Object);
            var client = new NotificationsClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            await client.OrderSms("12345678", Guid.NewGuid(), "nb", TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(sentRequest);
            Assert.Equal(HttpMethod.Post, sentRequest.Method);
            Assert.Equal(new Uri(_testBaseUrl + "v1/future/orders/instant/sms"), sentRequest.RequestUri);
            Assert.True(sentRequest.Headers.Contains("PlatformAccessToken"));
            Assert.IsType<StringContent>(sentRequest.Content);
            var content = await sentRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Contains("12345678", content);
            Assert.Contains("sms", content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("oppdatert", content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SendEmailOrder_WhenLanguageEn_SendsCorrectRequest()
        {
            // Arrange
            HttpRequestMessage sentRequest = null;
            var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK), req => sentRequest = req);
            _httpClient = new HttpClient(handler.Object);
            var client = new NotificationsClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            await client.OrderEmail("test@example.com", Guid.NewGuid(), "en", TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(sentRequest);
            Assert.Equal(HttpMethod.Post, sentRequest.Method);
            Assert.Equal(new Uri(_testBaseUrl + "v1/future/orders/instant/email"), sentRequest.RequestUri);
            Assert.True(sentRequest.Headers.Contains("PlatformAccessToken"));
            Assert.IsType<StringContent>(sentRequest.Content);
            var content = await sentRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Contains("test@example.com", content);
            Assert.Contains("email", content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("changed", content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SendOrder_InvalidAccessToken_LogsErrorAndDoesNotSend()
        {
            // Arrange
            var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK));
            _httpClient = new HttpClient(handler.Object);
            _tokenGenMock.Setup(t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>()))
                         .Returns(string.Empty);
            var client = new NotificationsClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            await client.OrderSms("12345678", Guid.NewGuid(), "nb", TestContext.Current.CancellationToken);

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
        public async Task SendOrder_UnsuccessfulResponse_LogsError()
        {
            // Arrange
            var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.BadRequest));
            _httpClient = new HttpClient(handler.Object);
            var client = new NotificationsClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            await client.OrderSms("12345678", Guid.NewGuid(), "nb", TestContext.Current.CancellationToken);

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

        // New tests for methods with verification code
        [Fact]
        public async Task SendSmsOrderWithCode_WhenLanguageNb_SendsCorrectRequest()
        {
            // Arrange
            HttpRequestMessage sentRequest = null;
            var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK), req => sentRequest = req);
            _httpClient = new HttpClient(handler.Object);
            var client = new NotificationsClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);
            var verificationCode = "9999";

            // Act
            await client.OrderSmsWithCode("12345678", Guid.NewGuid(), "nb", verificationCode, CancellationToken.None);

            // Assert
            Assert.NotNull(sentRequest);
            Assert.Equal(HttpMethod.Post, sentRequest.Method);
            Assert.Equal(new Uri(_testBaseUrl + "v1/future/orders/instant/sms"), sentRequest.RequestUri);
            Assert.True(sentRequest.Headers.Contains("PlatformAccessToken"));
            Assert.IsType<StringContent>(sentRequest.Content);
            var content = await sentRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Contains("12345678", content);
            Assert.Contains("sms", content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(verificationCode, content);
            Assert.Contains("bekrefte", content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SendEmailOrderWithCode_WhenLanguageEn_SendsCorrectRequest()
        {
            // Arrange
            HttpRequestMessage sentRequest = null;
            var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK), req => sentRequest = req);
            _httpClient = new HttpClient(handler.Object);
            var client = new NotificationsClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);
            var verificationCode = "abcd";

            // Act
            await client.OrderEmailWithCode("test@example.com", Guid.NewGuid(), "en", verificationCode, CancellationToken.None);

            // Assert
            Assert.NotNull(sentRequest);
            Assert.Equal(HttpMethod.Post, sentRequest.Method);
            Assert.Equal(new Uri(_testBaseUrl + "v1/future/orders/instant/email"), sentRequest.RequestUri);
            Assert.True(sentRequest.Headers.Contains("PlatformAccessToken"));
            Assert.IsType<StringContent>(sentRequest.Content);
            var content = await sentRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Contains("test@example.com", content);
            Assert.Contains("email", content, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(verificationCode, content);
            Assert.Contains("verify", content, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task SendOrderWithCode_InvalidAccessToken_LogsErrorAndDoesNotSend()
        {
            // Arrange
            var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK));
            _httpClient = new HttpClient(handler.Object);
            _tokenGenMock.Setup(t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>()))
                         .Returns(string.Empty);
            var client = new NotificationsClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            await client.OrderSmsWithCode("12345678", Guid.NewGuid(), "nb", "0000", CancellationToken.None);

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
        public async Task SendOrderWithCode_UnsuccessfulResponse_LogsError()
        {
            // Arrange
            var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.BadRequest));
            _httpClient = new HttpClient(handler.Object);
            var client = new NotificationsClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            await client.OrderSmsWithCode("12345678", Guid.NewGuid(), "nb", "0000", CancellationToken.None);

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
