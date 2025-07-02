using System;
using System.Net;
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

        private static void SetupAuthHandler(WebApplicationFactorySetup<FavoritesController> _webApplicationFactorySetup, Guid partyGuid, int UserId, bool access = true)
        {
            _webApplicationFactorySetup.RegisterClientMock
                .Setup(x => x.GetPartyId(partyGuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync((int)partyGuid.GetHashCode()); // Simulate party ID retrieval
            _webApplicationFactorySetup.AuthorizationClientMock
                .Setup(x => x.ValidateSelectedParty(UserId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(access);
        }

        [Fact]
        public async Task GetFavorites_WhenRepositoryReturnsValues_IsOk()
        {
            // Arrange
            const int UserId = 2516356;

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Get, UserId, "profile/api/v1/users/current/party-groups/favorites");

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

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Get, UserId, "profile/api/v1/users/current/party-groups/favorites");

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

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Get, UserId, "profile/api/v1/users/current/party-groups/favorites");

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
        public async Task AddToFavorites_WhenAlreadyInGroup_ReturnsNoContent()
        {
            // Arrange
            const int UserId = 2516356;
            var partyGuid = Guid.NewGuid();

            _webApplicationFactorySetup.PartyGroupRepositoryMock
                            .Setup(x => x.AddPartyToFavorites(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(false);
            SetupAuthHandler(_webApplicationFactorySetup, partyGuid, UserId);

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Put, UserId, $"profile/api/v1/users/current/party-groups/favorites/{partyGuid}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task AddToFavorites_WhenAdded_Returns201Created()
        {
            // Arrange
            const int UserId = 2516356;
            var partyGuid = Guid.NewGuid();

            _webApplicationFactorySetup.PartyGroupRepositoryMock
                            .Setup(x => x.AddPartyToFavorites(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            SetupAuthHandler(_webApplicationFactorySetup, partyGuid, UserId);

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Put, UserId, $"profile/api/v1/users/current/party-groups/favorites/{partyGuid}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task AddToFavorites_WhenAddingEmptyGuid_Returns400BadRequest()
        {
            // Arrange
            const int UserId = 2516356;
            var partyGuid = Guid.Empty;

            _webApplicationFactorySetup.PartyGroupRepositoryMock
                            .Setup(x => x.AddPartyToFavorites(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            SetupAuthHandler(_webApplicationFactorySetup, partyGuid, UserId);

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Put, UserId, $"profile/api/v1/users/current/party-groups/favorites/{partyGuid}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task DeleteFromFavorites_WhenDeletedSuccessfully_ReturnsNoContent()
        {
            // Arrange
            const int UserId = 2516356;
            var partyGuid = Guid.NewGuid();

            _webApplicationFactorySetup.PartyGroupRepositoryMock
                            .Setup(x => x.DeleteFromFavorites(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            SetupAuthHandler(_webApplicationFactorySetup, partyGuid, UserId);

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Delete, UserId, $"profile/api/v1/users/current/party-groups/favorites/{partyGuid}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        public async Task DeleteFromFavorites_WhenNotInGroup_ReturnsNotFound()
        {
            // Arrange
            const int UserId = 2516356;
            var partyGuid = Guid.NewGuid();

            _webApplicationFactorySetup.PartyGroupRepositoryMock
                            .Setup(x => x.DeleteFromFavorites(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(false);
            SetupAuthHandler(_webApplicationFactorySetup, partyGuid, UserId);

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Delete, UserId, $"profile/api/v1/users/current/party-groups/favorites/{partyGuid}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task DeleteFromFavorites_WhenEmptyPartyUuid_ReturnsBadRequest()
        {
            // Arrange
            const int UserId = 2516356;
            var partyGuid = Guid.Empty;

            HttpClient client = _webApplicationFactorySetup.GetTestServerClient();
            SetupAuthHandler(_webApplicationFactorySetup, partyGuid, UserId);

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Delete, UserId, $"profile/api/v1/users/current/party-groups/favorites/{partyGuid}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        private static HttpRequestMessage CreateRequest(HttpMethod method, int userId, string requestUri)
        {
            HttpRequestMessage httpRequestMessage = new(method, requestUri);
            string token = PrincipalUtil.GetToken(userId);
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return httpRequestMessage;
        }
    }
}
