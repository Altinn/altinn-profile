using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Platform.Profile.Models;
using Altinn.Profile.Controllers;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Tests.IntegrationTests.Mocks;
using Altinn.Profile.Tests.IntegrationTests.Utils;
using Altinn.Profile.Tests.Testdata;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers
{
    public class FavoritesControllerTests : IClassFixture<WebApplicationFactory<FavoritesController>>
    {
        private readonly WebApplicationFactorySetup<FavoritesController> _webApplicationFactorySetup;

        private readonly JsonSerializerOptions _serializerOptionsCamelCase = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public FavoritesControllerTests(WebApplicationFactory<FavoritesController> factory)
        {
            _webApplicationFactorySetup = new WebApplicationFactorySetup<FavoritesController>(factory);

            _webApplicationFactorySetup.PartyGroupRepositoryMock
                .Setup(x => x.GetFavorites(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([1, 2, 3]);
        }

        [Fact]
        public async Task GetFavorites_WhenRepositoryReturnsValues_IsOk()
        {
            // Arrange
            const int UserId = 2516356;

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, "profile/api/v1/groups/favorites");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();

            int[] favorites = JsonSerializer.Deserialize<int[]>(
                responseContent, _serializerOptionsCamelCase);

            Assert.NotEmpty(favorites);
        }

        [Fact]
        public async Task GetFavorites_WhenRepositoryReturnsEmpty_IsOk()
        {
            // Arrange
            const int UserId = 1234;
            _webApplicationFactorySetup.PartyGroupRepositoryMock
                .Setup(x => x.GetFavorites(UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<int>());
            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            HttpRequestMessage request = CreateGetRequest(UserId, "profile/api/v1/groups/favorites");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);
            int[] favorites = JsonSerializer.Deserialize<int[]>(
                await response.Content.ReadAsStringAsync(), _serializerOptionsCamelCase);
            Assert.Empty(favorites);
        }

        [Fact]
        public async Task GetFavorites_WhenRepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            const int UserId = 5678;
            _webApplicationFactorySetup.PartyGroupRepositoryMock
                .Setup(x => x.GetFavorites(UserId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));
            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            HttpRequestMessage request = CreateGetRequest(UserId, "profile/api/v1/groups/favorites");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task GetFavorites_WithoutAuthorizationHeader_ReturnsUnauthorized()
        {
            // Arrange
            const int UserId = 9999;
            _webApplicationFactorySetup.PartyGroupRepositoryMock
                .Setup(x => x.GetFavorites(UserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { 1 });
            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            // Create request without Authorization header
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "profile/api/v1/groups/favorites");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private static HttpRequestMessage CreateGetRequest(int userId, string requestUri)
        {
            HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, requestUri);
            string token = PrincipalUtil.GetToken(userId);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return httpRequestMessage;
        }
    }
}