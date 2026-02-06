using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Integrations.SblBridge.User.Favorites;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.SblBridge.User.Favorites
{
    public class UserFavoriteClientTests
    {
        private readonly Mock<IOptions<SblBridgeSettings>> _settingsMock;
        private readonly Mock<ILogger<UserFavoriteClient>> _loggerMock;
        private HttpClient _httpClient;
        private const string _testBaseUrl = "https://api.test.local/";

        public UserFavoriteClientTests()
        {
            _settingsMock = new Mock<IOptions<SblBridgeSettings>>();
            _settingsMock.Setup(s => s.Value).Returns(new SblBridgeSettings { ApiProfileEndpoint = _testBaseUrl });

            _loggerMock = new Mock<ILogger<UserFavoriteClient>>();
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
            var client = new UserFavoriteClient(_httpClient, _loggerMock.Object, _settingsMock.Object);
            Assert.Equal(new Uri(_testBaseUrl), _httpClient.BaseAddress);
        }

        [Fact]
        public async Task UpdateFavorites_SuccessfulRequest_DoesNotLogError()
        {
            // Arrange
            var request = new FavoriteChangedRequest
            {
                ChangeType = "insert",
                UserId = 123,
                PartyUuid = Guid.NewGuid(),
                ChangeDateTime = DateTime.UtcNow
            };
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            HttpRequestMessage sentRequest = null;
            var handler = CreateHandler(response, req => sentRequest = req);
            _httpClient = new HttpClient(handler.Object);
            var client = new UserFavoriteClient(_httpClient, _loggerMock.Object, _settingsMock.Object);

            // Act
            await client.UpdateFavorites(request);

            // Assert
            Assert.NotNull(sentRequest);
            Assert.Equal(HttpMethod.Post, sentRequest.Method);
            Assert.Equal(new Uri(_testBaseUrl + "users/favorite/update"), sentRequest.RequestUri);
            Assert.IsType<StringContent>(sentRequest.Content);
            var requestContent = await sentRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var sentPayload = JsonSerializer.Deserialize<FavoriteChangedRequest>(requestContent);
            Assert.Equal(request.ChangeType, sentPayload.ChangeType);
            Assert.Equal(request.UserId, sentPayload.UserId);
            Assert.Equal(request.PartyUuid, sentPayload.PartyUuid);
            Assert.Equal(request.ChangeDateTime, sentPayload.ChangeDateTime, TimeSpan.FromSeconds(1));
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
        public async Task UpdateFavorites_WhenResponseIsNotSuccess_LogsError()
        {
            // Arrange
            var request = new FavoriteChangedRequest
            {
                ChangeType = "delete",
                UserId = 456,
                PartyUuid = Guid.NewGuid(),
                ChangeDateTime = DateTime.UtcNow
            };
            var errorMessage = "Something went wrong";
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(errorMessage, Encoding.UTF8, "text/plain")
            };
            var handler = CreateHandler(response);
            _httpClient = new HttpClient(handler.Object);
            var client = new UserFavoriteClient(_httpClient, _loggerMock.Object, _settingsMock.Object);

            // Act
            await client.UpdateFavorites(request);

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
        public async Task UpdateFavorites_WhenResponseIsInternalServerError_ThrowsException()
        {
            // Arrange
            var request = new FavoriteChangedRequest
            {
                ChangeType = "delete",
                UserId = 456,
                PartyUuid = Guid.NewGuid(),
                ChangeDateTime = DateTime.UtcNow
            };
            var errorMessage = "Something went wrong";
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(errorMessage, Encoding.UTF8, "text/plain")
            };
            var handler = CreateHandler(response);
            _httpClient = new HttpClient(handler.Object);
            var client = new UserFavoriteClient(_httpClient, _loggerMock.Object, _settingsMock.Object);

            // Act
            await Assert.ThrowsAsync<InternalServerErrorException>(() => client.UpdateFavorites(request));

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
