using System;
using System.Collections.Generic;
using System.Linq;
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
    public class PartyGroupControllerTests : IClassFixture<ProfileWebApplicationFactory<Program>>
    {
        private readonly JsonSerializerOptions _serializerOptionsCamelCase = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly ProfileWebApplicationFactory<Program> _factory;

        public PartyGroupControllerTests(ProfileWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _factory.PartyGroupRepositoryMock.Reset();
            _factory.PartyGroupRepositoryMock
                .Setup(x => x.GetGroups(It.IsAny<int>(), false, It.IsAny<CancellationToken>()))
             .ReturnsAsync(new List<Group>
                {
                    new Group
                    {
                        Parties = [new PartyGroupAssociation { PartyUuid = Guid.NewGuid() }, new PartyGroupAssociation { PartyUuid = Guid.NewGuid() }],
                        Name = "__group0__",
                        GroupId = 1,
                    }
                });
        }

        [Fact]
        public async Task Get_ReturnsGroupedPartyUuids_WhenUserIsValid()
        {
            // Arrange
            const int UserId = 2516356;

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Get, UserId, "profile/api/v1/users/current/party-groups");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();

            List<GroupResponse> groupResponses = JsonSerializer.Deserialize<List<GroupResponse>>(
                responseContent, _serializerOptionsCamelCase);
                       
            Assert.Equal(2, groupResponses[0].Parties.Length);
            Assert.Equal("__group0__", groupResponses[0].Name);
            Assert.Equal(1, groupResponses[0].GroupId);
        }

        [Fact]
        public async Task Get_ReturnsMultipleGroupedPartyUuids_WhenUserIsValid()
        {
            // Arrange
            const int UserId = 2516356;

            _factory.PartyGroupRepositoryMock
                .Setup(x => x.GetGroups(It.IsAny<int>(), false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Group>
                {
                    new Group
                    {
                        Parties = [
                            new PartyGroupAssociation { PartyUuid = Guid.NewGuid() },
                            new PartyGroupAssociation { PartyUuid = Guid.NewGuid() }],
                        Name = "__group0__",
                        GroupId = 1,
                    },
                    new Group
                    {
                        Parties = [new PartyGroupAssociation { PartyUuid = Guid.NewGuid() },
                                   new PartyGroupAssociation { PartyUuid = Guid.NewGuid() },
                                   new PartyGroupAssociation { PartyUuid = Guid.NewGuid() },
                                   new PartyGroupAssociation { PartyUuid = Guid.NewGuid() }
                        ],
                        Name = "__group1__",
                        GroupId = 2,
                    },
                });

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Get, UserId, "profile/api/v1/users/current/party-groups");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();

            List<GroupResponse> groupResponses = JsonSerializer.Deserialize<List<GroupResponse>>(
                responseContent, _serializerOptionsCamelCase);

            Assert.Equal(2, groupResponses[0].Parties.Length);
            Assert.Equal("__group0__", groupResponses[0].Name);
            Assert.Equal(1, groupResponses[0].GroupId);
            Assert.Equal(4, groupResponses[1].Parties.Length);
            Assert.Equal("__group1__", groupResponses[1].Name);
            Assert.Equal(2, groupResponses[1].GroupId);
        }

        [Fact]
        public async Task GetPartyGroups_WhenRepositoryReturnsEmptyGroup_IsOk()
        {
            // Arrange
            const int UserId = 2516356;

            _factory.PartyGroupRepositoryMock
            .Setup(x => x.GetGroups(It.IsAny<int>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Group>
            {
                        new Group
                        {
                            Parties = [],
                            Name = "__group0__",
                            GroupId = 1,
                        }
            });

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Get, UserId, "profile/api/v1/users/current/party-groups");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();

            List<GroupResponse> groupResponse = JsonSerializer.Deserialize<List<GroupResponse>>(
                responseContent, _serializerOptionsCamelCase);

            Assert.Empty(groupResponse[0].Parties);
        }

        [Fact]
        public async Task GetPartyGroups_WhenUserHasNoGroups_ReturnsEmptyList()
        {
            // Arrange
            const int UserId = 2516356;

            _factory.PartyGroupRepositoryMock
               .Setup(x => x.GetGroups(It.IsAny<int>(), false, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new List<Group> { });

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Get, UserId, "profile/api/v1/users/current/party-groups");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();

            List<GroupResponse> groupResponse = JsonSerializer.Deserialize<List<GroupResponse>>(
                responseContent, _serializerOptionsCamelCase);

            Assert.Empty(groupResponse);
        }

        [Fact]
        public async Task GetPartyGroups_WhenRepositoryReturnsNull_ReturnsEmptyList()
        {
            // Arrange
            const int UserId = 2516356;

            _factory.PartyGroupRepositoryMock
               .Setup(x => x.GetGroups(It.IsAny<int>(), false, It.IsAny<CancellationToken>()))
               .ReturnsAsync((List<Group>)null);

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Get, UserId, "profile/api/v1/users/current/party-groups");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);

            string responseContent = await response.Content.ReadAsStringAsync();

            List<GroupResponse> groupResponse = JsonSerializer.Deserialize<List<GroupResponse>>(
                responseContent, _serializerOptionsCamelCase);

            Assert.NotNull(groupResponse);
            Assert.Empty(groupResponse);
        }

        [Fact]
        public async Task Get_WhenNoUserIdClaim_ReturnsBadRequest_AndRepositoryNotCalled()
        {
            // Arrange
            HttpClient client = _factory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "profile/api/v1/users/current/party-groups");

            // Act
            HttpResponseMessage response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            
            _factory.PartyGroupRepositoryMock.Verify(x => x.GetGroups(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
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
