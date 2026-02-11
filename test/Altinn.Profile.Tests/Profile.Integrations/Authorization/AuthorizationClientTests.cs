using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Common.PEP.Configuration;
using Altinn.Profile.Integrations.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Moq;
using Moq.Protected;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Authorization
{
    public class AuthorizationClientTests
    {
        private const string _authEndpoint = "https://auth.test.local/";
        private readonly Mock<IOptions<PlatformSettings>> _settingsMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<ILogger<AuthorizationClient>> _logger;

        public AuthorizationClientTests()
        {
            _settingsMock = new Mock<IOptions<PlatformSettings>>();
            _settingsMock.Setup(s => s.Value).Returns(new PlatformSettings { ApiAuthorizationEndpoint = _authEndpoint });
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _logger = new Mock<ILogger<AuthorizationClient>>();
        }

        private static Mock<HttpMessageHandler> CreateHandler(HttpResponseMessage response, Action<HttpRequestMessage> requestCallback = null)
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
                    return response;
                });
            return handlerMock;
        }

        [Fact]
        public async Task ValidateSelectedParty_WhenAuthClientValidatesSuccessfully_ReturnsTrue()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(true)
            };
            var handler = CreateHandler(response, req =>
            {
                Assert.Equal(HttpMethod.Get, req.Method);
                Assert.Contains("parties/42/validate?userid=21", req.RequestUri.ToString());
                Assert.True(req.Headers.Contains("Authorization"));
            });
            var httpClient = new HttpClient(handler.Object);

            var context = new DefaultHttpContext();
            context.Request.Headers[HeaderNames.Authorization] = "Bearer testtoken";
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);

            var client = new AuthorizationClient(_settingsMock.Object, httpClient, _httpContextAccessorMock.Object, _logger.Object);

            // Act
            var result = await client.ValidateSelectedParty(21, 42, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateSelectedParty_UnsuccessfulStatus_ReturnsFalse()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.Forbidden);
            var handler = CreateHandler(response);
            var httpClient = new HttpClient(handler.Object);

            var context = new DefaultHttpContext();
            context.Request.Headers[HeaderNames.Authorization] = "Bearer testtoken";
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);

            var client = new AuthorizationClient(_settingsMock.Object, httpClient, _httpContextAccessorMock.Object, _logger.Object);

            // Act
            var result = await client.ValidateSelectedParty(21, 42, TestContext.Current.CancellationToken);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateSelectedParty_DeserializationFails_ThrowsException()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("not a bool", System.Text.Encoding.UTF8, "application/json")
            };
            var handler = CreateHandler(response);
            var httpClient = new HttpClient(handler.Object);

            var context = new DefaultHttpContext();
            context.Request.Headers[HeaderNames.Authorization] = "Bearer testtoken";
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);

            var client = new AuthorizationClient(_settingsMock.Object, httpClient, _httpContextAccessorMock.Object, _logger.Object);

            // Act & assert
            await Assert.ThrowsAsync<JsonException>(() => client.ValidateSelectedParty(21, 42, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task ValidateSelectedParty_NoAuthorizationHeader_ReturnsFalse()
        {
            // Arrange
            var handler = CreateHandler(null, req =>
            {
                Assert.True(req.Headers.Contains("Authorization"));
                Assert.Equal(string.Empty, req.Headers.GetValues("Authorization").ToString());
            });
            var httpClient = new HttpClient(handler.Object);

            var context = new DefaultHttpContext();

            // No Authorization header set
            _httpContextAccessorMock.Setup(a => a.HttpContext).Returns(context);

            var client = new AuthorizationClient(_settingsMock.Object, httpClient, _httpContextAccessorMock.Object, _logger.Object);

            // Act
            var result = await client.ValidateSelectedParty(21, 42, TestContext.Current.CancellationToken);

            // Assert
            Assert.False(result);
        }
    }
}
