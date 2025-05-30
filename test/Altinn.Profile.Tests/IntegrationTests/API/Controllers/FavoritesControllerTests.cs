using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Controllers;
using Altinn.Profile.Core.PartyGroups;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.IntegrationTests.Utils;
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
                .ReturnsAsync(new Group { Parties = [new PartyGroupAssociation { PartyUuid = Guid.NewGuid() }, new PartyGroupAssociation { PartyUuid = Guid.NewGuid() }], Name = "__favoritter__" });
        }

        [Fact]
        public async Task GetFavorites_WhenRepositoryReturnsValues_IsOk()
        {
            // Arrange
            const int UserId = 2516356;

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, "profile/api/v1/users/current/party-groups/favorites");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();

            GroupResponse favorites = JsonSerializer.Deserialize<GroupResponse>(
                responseContent, _serializerOptionsCamelCase);

            Assert.NotEmpty(favorites.Parties);
        }

        [Fact]
        public async Task GetFavorites_WhenRepositoryReturnsEmptyGroup_IsOk()
        {
            // Arrange
            const int UserId = 2516356;

            _webApplicationFactorySetup.PartyGroupRepositoryMock
                .Setup(x => x.GetFavorites(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Group { Parties = [], Name = "__favoritter__" });

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, "profile/api/v1/users/current/party-groups/favorites");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();

            GroupResponse favorites = JsonSerializer.Deserialize<GroupResponse>(
                responseContent, _serializerOptionsCamelCase);

            Assert.Empty(favorites.Parties);
        }

        [Fact]
        public async Task GetFavorites_WhenRepositoryReturnsNull_IsOk()
        {
            // Arrange
            const int UserId = 2516356;

            _webApplicationFactorySetup.PartyGroupRepositoryMock
                .Setup(x => x.GetFavorites(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Group)null);

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, "profile/api/v1/users/current/party-groups/favorites");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();

            GroupResponse favorites = JsonSerializer.Deserialize<GroupResponse>(
                responseContent, _serializerOptionsCamelCase);

            Assert.Empty(favorites.Parties);
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
