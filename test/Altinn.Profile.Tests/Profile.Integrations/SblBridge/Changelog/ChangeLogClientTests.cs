using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.Changelog;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.SblBridge.Changelog
{
    public class ChangeLogClientTests
    {
        private readonly Mock<IOptions<SblBridgeSettings>> _settingsMock;
        private readonly Mock<ILogger<ChangeLogClient>> _loggerMock;
        private HttpClient _httpClient;
        private const string _testBaseUrl = "https://api.test.local/";

        public ChangeLogClientTests()
        {
            _settingsMock = new Mock<IOptions<SblBridgeSettings>>();
            _settingsMock.Setup(s => s.Value).Returns(new SblBridgeSettings { ApiProfileEndpoint = _testBaseUrl });

            _loggerMock = new Mock<ILogger<ChangeLogClient>>();
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
            var client = new ChangeLogClient(_httpClient, _loggerMock.Object, _settingsMock.Object);

            Assert.Equal(new Uri(_testBaseUrl), _httpClient.BaseAddress);
        }

        [Fact]
        public async Task GetChangeLog_SuccessfulRequest_DoesNotLogError()
        {
            // Arrange
            var response = new HttpResponseMessage() { Content = JsonContent.Create(new ChangeLog()), StatusCode = HttpStatusCode.OK };
            HttpRequestMessage sentRequest = null;
            var handler = CreateHandler(response, req => sentRequest = req);
            _httpClient = new HttpClient(handler.Object);
            var client = new ChangeLogClient(_httpClient, _loggerMock.Object, _settingsMock.Object);

            // Act
            await client.GetChangeLog(DateTime.MinValue, DataType.Favorites, CancellationToken.None);

            // Assert
            Assert.NotNull(sentRequest);
            Assert.Equal(HttpMethod.Get, sentRequest.Method);
            Assert.Equal(new Uri(_testBaseUrl + "profilechangelog?fromTimestamp=0001-01-01T00:00:00.0000000Z&dataType=Favorites"), sentRequest.RequestUri);
            Assert.Equal(DateTime.MinValue, DateTime.Parse("0001-01-01T00:00:00.0000000Z").ToUniversalTime());

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
        public async Task GetChangeLog_WhenResponseIsNotSuccess_LogsError()
        {
            // Arrange
            var errorMessage = "Something went wrong";
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(errorMessage, Encoding.UTF8, "text/plain")
            };
            var handler = CreateHandler(response);
            _httpClient = new HttpClient(handler.Object);
            var client = new ChangeLogClient(_httpClient, _loggerMock.Object, _settingsMock.Object);

            // Act
            await client.GetChangeLog(DateTime.MinValue, DataType.Favorites, CancellationToken.None);

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
        public async Task GetChangeLog_WhenResponseIsInternalServerError_ThrowsException()
        {
            // Arrange
            var errorMessage = "Something went wrong";
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(errorMessage, Encoding.UTF8, "text/plain")
            };
            var handler = CreateHandler(response);
            _httpClient = new HttpClient(handler.Object);
            var client = new ChangeLogClient(_httpClient, _loggerMock.Object, _settingsMock.Object);

            // Act
            await Assert.ThrowsAsync<InternalServerErrorException>(() => client.GetChangeLog(DateTime.MinValue, DataType.Favorites, CancellationToken.None));

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
