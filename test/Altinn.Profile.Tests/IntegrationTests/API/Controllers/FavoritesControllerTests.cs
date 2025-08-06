using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core.PartyGroups;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.IntegrationTests.Utils;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers
{
    public class FavoritesControllerTests : IClassFixture<ProfileWebApplicationFactory<Program>>
    {
        private readonly JsonSerializerOptions _serializerOptionsCamelCase = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly ProfileWebApplicationFactory<Program> _factory;

        public FavoritesControllerTests(ProfileWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _factory.PartyGroupRepositoryMock.Reset();
            _factory.PartyGroupRepositoryMock
                .Setup(x => x.GetFavorites(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Group
                {
                    Parties = [new PartyGroupAssociation { PartyUuid = Guid.NewGuid() }, new PartyGroupAssociation { PartyUuid = Guid.NewGuid() }],
                    Name = "__favoritter__"
                });
        }

        [Fact]
        public async Task GetFavorites_WhenRepositoryReturnsValues_IsOk()
        {
            // Arrange
            const int UserId = 2516356;

            HttpClient client = _factory.CreateClient();

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

            _factory.PartyGroupRepositoryMock
                .Setup(x => x.GetFavorites(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Group { Parties = [], Name = "__favoritter__" });

            HttpClient client = _factory.CreateClient();

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

            _factory.PartyGroupRepositoryMock
                .Setup(x => x.GetFavorites(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Group)null);

            HttpClient client = _factory.CreateClient();

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

            _factory.PartyGroupRepositoryMock
                            .Setup(x => x.AddPartyToFavorites(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(false);
            SetupAuthHandler(_factory, partyGuid, UserId);

            HttpClient client = _factory.CreateClient();

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

            _factory.PartyGroupRepositoryMock
                            .Setup(x => x.AddPartyToFavorites(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            SetupAuthHandler(_factory, partyGuid, UserId);

            HttpClient client = _factory.CreateClient();

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

            _factory.PartyGroupRepositoryMock
                            .Setup(x => x.AddPartyToFavorites(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            SetupAuthHandler(_factory, partyGuid, UserId);

            HttpClient client = _factory.CreateClient();

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

            _factory.PartyGroupRepositoryMock
                            .Setup(x => x.DeleteFromFavorites(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            SetupAuthHandler(_factory, partyGuid, UserId);

            HttpClient client = _factory.CreateClient();

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

            _factory.PartyGroupRepositoryMock
                            .Setup(x => x.DeleteFromFavorites(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(false);
            SetupAuthHandler(_factory, partyGuid, UserId);

            HttpClient client = _factory.CreateClient();

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

            SetupAuthHandler(_factory, partyGuid, UserId);

            HttpClient client = _factory.CreateClient();

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

        private static void SetupAuthHandler(ProfileWebApplicationFactory<Program> factory, Guid partyGuid, int UserId, bool access = true)
        {
            factory.RegisterClientMock.Reset();
            factory.RegisterClientMock
                .Setup(x => x.GetPartyId(partyGuid, It.IsAny<CancellationToken>()))
                .ReturnsAsync((int)partyGuid.GetHashCode()); // Simulate party ID retrieval

            factory.AuthorizationClientMock.Reset();
            factory.AuthorizationClientMock
                .Setup(x => x.ValidateSelectedParty(UserId, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(access);
        }
    }
}
