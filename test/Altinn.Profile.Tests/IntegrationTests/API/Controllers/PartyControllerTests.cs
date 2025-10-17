using System;
using System.Collections.Generic;
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
    public class PartyControllerTests : IClassFixture<ProfileWebApplicationFactory<Program>>
    {
        private readonly JsonSerializerOptions _serializerOptionsCamelCase = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly ProfileWebApplicationFactory<Program> _factory;

        public PartyControllerTests(ProfileWebApplicationFactory<Program> factory)
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
                        Name = "__favoritter__",
                    }
                });
        }

        [Fact]
        public async Task GetPartyGroups_WhenRepositoryReturnsValues_IsOk()
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

            GroupResponse groupResponse = JsonSerializer.Deserialize<GroupResponse>(
                responseContent, _serializerOptionsCamelCase);

            Assert.NotEmpty(groupResponse.Parties);
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
                        Parties = [new PartyGroupAssociation { PartyUuid = Guid.NewGuid() }, new PartyGroupAssociation { PartyUuid = Guid.NewGuid() }],
                        Name = "__favoritter__",
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

            GroupResponse groupResponse = JsonSerializer.Deserialize<GroupResponse>(
                responseContent, _serializerOptionsCamelCase);

            Assert.Empty(groupResponse.Parties);
        }

        [Fact]
        public async Task GetPartyGroups_WhenRepositoryReturnsNull_IsOk()
        {
            // Arrange
            const int UserId = 2516356;

            _factory.PartyGroupRepositoryMock
               .Setup(x => x.GetGroups(It.IsAny<int>(), false, It.IsAny<CancellationToken>()))
               .ReturnsAsync(new List<Group>
               {
                    new Group
                    {
                        Parties = [new PartyGroupAssociation { PartyUuid = Guid.NewGuid() }, new PartyGroupAssociation { PartyUuid = Guid.NewGuid() }],
                        Name = "__favoritter__",
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

            GroupResponse groupResponse = JsonSerializer.Deserialize<GroupResponse>(
                responseContent, _serializerOptionsCamelCase);

            Assert.Empty(groupResponse.Parties);
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
