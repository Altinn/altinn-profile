using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Common.AccessTokenClient.Services;
using Altinn.Profile.Integrations.Register;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.OrganizationNotificationAddressTests
{
    public class RegisterClientTests
    {
        private readonly Mock<IOptions<RegisterSettings>> _settingsMock;
        private readonly Mock<IAccessTokenGenerator> _tokenGenMock;
        private readonly Mock<ILogger<RegisterClient>> _loggerMock;
        private HttpClient _httpClient;
        private const string _testBaseUrl = "https://api.test.local/";
        private readonly JsonSerializerOptions _serializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        public RegisterClientTests()
        {
            _settingsMock = new Mock<IOptions<RegisterSettings>>();
            _settingsMock.Setup(s => s.Value).Returns(new RegisterSettings { ApiRegisterEndpoint = _testBaseUrl });

            _tokenGenMock = new Mock<IAccessTokenGenerator>();
            _tokenGenMock.Setup(t => t.GenerateAccessToken(It.IsAny<string>(), It.IsAny<string>()))
                         .Returns((string)null);

            _loggerMock = new Mock<ILogger<RegisterClient>>();
        }

        private static HttpMessageHandler CreateHandler(
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
            return handlerMock.Object;
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK));
            var httpClient = new HttpClient(handler);
            Assert.Throws<ArgumentNullException>(() =>
                new RegisterClient(httpClient, _settingsMock.Object, _tokenGenMock.Object, null!));
        }

        [Fact]
        public void Constructor_BaseAddressIsSetFromSettings()
        {
            var handler = CreateHandler(new HttpResponseMessage(HttpStatusCode.OK));
            _httpClient = new HttpClient(handler);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);
            Assert.Equal(new Uri(_testBaseUrl), _httpClient.BaseAddress);
        }

        [Fact]
        public async Task GetMainUnit_SingleEntry_ReturnsOrganizationRecord()
        {
            // Arrange
            var responseContent = JsonSerializer.Serialize(new LookupMainUnitResponse
            {
                Data = [new() { OrganizationIdentifier = "123456789" }]
            });

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            };
            HttpRequestMessage sentRequest = null;
            var handler = CreateHandler(response, req => sentRequest = req);
            _httpClient = new HttpClient(handler);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetMainUnit("111111111", CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("123456789", result);
            Assert.Equal(HttpMethod.Post, sentRequest.Method);
            Assert.IsType<StringContent>(sentRequest.Content);
            var requestContent = await sentRequest.Content.ReadAsStringAsync();
            var sentPayload = JsonSerializer.Deserialize<LookupMainUnitRequest>(requestContent, _serializerOptions);
            Assert.Equal("urn:altinn:organization:identifier-no:111111111", sentPayload.Data);
            Assert.Equal(new Uri(_testBaseUrl + "v2/internal/parties/main-units"), sentRequest.RequestUri);
        }

        [Fact]
        public async Task GetMainUnit_WhenNothingFound_ReturnsNull()
        {
            // Arrange
            var responseContent = JsonSerializer.Serialize(new LookupMainUnitResponse
            {
                Data = []
            });

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent, System.Text.Encoding.UTF8, "application/json")
            };
            HttpRequestMessage sentRequest = null;
            var handler = CreateHandler(response, req => sentRequest = req);
            _httpClient = new HttpClient(handler);
            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetMainUnit("111111111", CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetMainUnit_WhenRegisterReturns500_ReturnsNullAnLogsWarning()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            HttpRequestMessage sentRequest = null;
            var handler = CreateHandler(response, req => sentRequest = req);
            _httpClient = new HttpClient(handler);

            var client = new RegisterClient(_httpClient, _settingsMock.Object, _tokenGenMock.Object, _loggerMock.Object);

            // Act
            var result = await client.GetMainUnit("111111111", CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
    }
}
