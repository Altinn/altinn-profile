using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core.User.PartyGroups;
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
             .ReturnsAsync(
                [
                    new() 
                    {
                        Parties = [new PartyGroupAssociation { PartyUuid = Guid.NewGuid() }, new PartyGroupAssociation { PartyUuid = Guid.NewGuid() }],
                        Name = "__group0__",
                        GroupId = 1,
                    }
                ]);
        }

        [Fact]
        public async Task Get_ReturnsGroupedPartyUuids_WhenUserIsValid()
        {
            // Arrange
            const int UserId = 2516356;

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Get, UserId, "profile/api/v1/users/current/party-groups");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

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
                .ReturnsAsync(
                [
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
                ]);

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Get, UserId, "profile/api/v1/users/current/party-groups");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

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
            .ReturnsAsync(
            [
                        new Group
                        {
                            Parties = [],
                            Name = "__group0__",
                            GroupId = 1,
                        }
            ]);

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Get, UserId, "profile/api/v1/users/current/party-groups");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

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
               .ReturnsAsync([]);

            HttpClient client = _factory.CreateClient();

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Get, UserId, "profile/api/v1/users/current/party-groups");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

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
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.True(response.IsSuccessStatusCode);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

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
            HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            
            _factory.PartyGroupRepositoryMock.Verify(x => x.GetGroups(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Create_ReturnsCreatedGroup_WhenRequestIsValid()
        {
            // Arrange
            const int UserId = 2516356;
            const string GroupName = "My Test Group";

            var createdGroup = new Group
            {
                GroupId = 42,
                UserId = UserId,
                Name = GroupName,
                IsFavorite = false,
                Parties = []
            };

            _factory.PartyGroupRepositoryMock
                .Setup(x => x.CreateGroup(UserId, GroupName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdGroup);

            HttpClient client = _factory.CreateClient();

            var requestBody = new GroupRequest { Name = GroupName };
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody, _serializerOptionsCamelCase),
                System.Text.Encoding.UTF8,
                "application/json");

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Post, UserId, "profile/api/v1/users/current/party-groups");
            httpRequestMessage.Content = content;

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

            GroupResponse groupResponse = JsonSerializer.Deserialize<GroupResponse>(
                responseContent, _serializerOptionsCamelCase);

            Assert.NotNull(groupResponse);
            Assert.Equal(42, groupResponse.GroupId);
            Assert.Equal(GroupName, groupResponse.Name);
            Assert.False(groupResponse.IsFavorite);
            Assert.Empty(groupResponse.Parties);

            _factory.PartyGroupRepositoryMock.Verify(
                x => x.CreateGroup(UserId, GroupName, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenNameIsMissing()
        {
            // Arrange
            const int UserId = 2516356;

            HttpClient client = _factory.CreateClient();

            var requestBody = new { };
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody, _serializerOptionsCamelCase),
                System.Text.Encoding.UTF8,
                "application/json");

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Post, UserId, "profile/api/v1/users/current/party-groups");
            httpRequestMessage.Content = content;

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            _factory.PartyGroupRepositoryMock.Verify(
                x => x.CreateGroup(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Create_ReturnsBadRequest_WhenNameIsEmpty()
        {
            // Arrange
            const int UserId = 2516356;

            HttpClient client = _factory.CreateClient();

            var requestBody = new { name = string.Empty };
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody, _serializerOptionsCamelCase),
                System.Text.Encoding.UTF8,
                "application/json");

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Post, UserId, "profile/api/v1/users/current/party-groups");
            httpRequestMessage.Content = content;

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            _factory.PartyGroupRepositoryMock.Verify(
                x => x.CreateGroup(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Create_WhenNoUserIdClaim_ReturnsUnauthorized_AndRepositoryNotCalled()
        {
            // Arrange
            HttpClient client = _factory.CreateClient();

            var requestBody = new GroupRequest { Name = "Test Group" };
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody, _serializerOptionsCamelCase),
                System.Text.Encoding.UTF8,
                "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, "profile/api/v1/users/current/party-groups")
            {
                Content = content
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            _factory.PartyGroupRepositoryMock.Verify(
                x => x.CreateGroup(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Create_ReturnsLocationHeader_WhenGroupIsCreated()
        {
            // Arrange
            const int UserId = 2516356;
            const string GroupName = "Header Test Group";

            var createdGroup = new Group
            {
                GroupId = 123,
                UserId = UserId,
                Name = GroupName,
                IsFavorite = false,
                Parties = []
            };

            _factory.PartyGroupRepositoryMock
                .Setup(x => x.CreateGroup(UserId, GroupName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(createdGroup);

            HttpClient client = _factory.CreateClient();

            var requestBody = new GroupRequest { Name = GroupName };
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody, _serializerOptionsCamelCase),
                System.Text.Encoding.UTF8,
                "application/json");

            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Post, UserId, "profile/api/v1/users/current/party-groups");
            httpRequestMessage.Content = content;

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.NotNull(response.Headers.Location);
            Assert.Contains("123", response.Headers.Location.ToString());
        }

        [Fact]
        public async Task GetById_ReturnsGroup_WhenGroupExists()
        {
            // Arrange
            const int UserId = 2516356;
            const int GroupId = 42;
            var partyUuid1 = Guid.NewGuid();
            var partyUuid2 = Guid.NewGuid();

            var group = new Group
            {
                GroupId = GroupId,
                UserId = UserId,
                Name = "Test Group",
                IsFavorite = false,
                Parties = [
                    new PartyGroupAssociation { PartyUuid = partyUuid1 },
                    new PartyGroupAssociation { PartyUuid = partyUuid2 }
                ]
            };

            _factory.PartyGroupRepositoryMock
                .Setup(x => x.GetGroup(UserId, GroupId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(group);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Get, UserId, $"profile/api/v1/users/current/party-groups/{GroupId}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            GroupResponse groupResponse = JsonSerializer.Deserialize<GroupResponse>(
                responseContent, _serializerOptionsCamelCase);

            Assert.NotNull(groupResponse);
            Assert.Equal(GroupId, groupResponse.GroupId);
            Assert.Equal("Test Group", groupResponse.Name);
            Assert.False(groupResponse.IsFavorite);
            Assert.Equal(2, groupResponse.Parties.Length);
            Assert.Contains(partyUuid1, groupResponse.Parties);
            Assert.Contains(partyUuid2, groupResponse.Parties);

            _factory.PartyGroupRepositoryMock.Verify(
                x => x.GetGroup(UserId, GroupId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenGroupDoesNotExist()
        {
            // Arrange
            const int UserId = 2516356;
            const int GroupId = 999;

            _factory.PartyGroupRepositoryMock
                .Setup(x => x.GetGroup(UserId, GroupId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Group)null);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Get, UserId, $"profile/api/v1/users/current/party-groups/{GroupId}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            _factory.PartyGroupRepositoryMock.Verify(
                x => x.GetGroup(UserId, GroupId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetById_ReturnsUnauthorized_WhenNoUserIdClaim()
        {
            // Arrange
            const int GroupId = 42;

            HttpClient client = _factory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, $"profile/api/v1/users/current/party-groups/{GroupId}");

            // Act
            HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            _factory.PartyGroupRepositoryMock.Verify(
                x => x.GetGroup(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GetById_ReturnsGroupWithEmptyParties_WhenGroupHasNoParties()
        {
            // Arrange
            const int UserId = 2516356;
            const int GroupId = 10;

            var group = new Group
            {
                GroupId = GroupId,
                UserId = UserId,
                Name = "Empty Group",
                IsFavorite = false,
                Parties = []
            };

            _factory.PartyGroupRepositoryMock
                .Setup(x => x.GetGroup(UserId, GroupId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(group);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Get, UserId, $"profile/api/v1/users/current/party-groups/{GroupId}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            GroupResponse groupResponse = JsonSerializer.Deserialize<GroupResponse>(
                responseContent, _serializerOptionsCamelCase);

            Assert.NotNull(groupResponse);
            Assert.Equal(GroupId, groupResponse.GroupId);
            Assert.Equal("Empty Group", groupResponse.Name);
            Assert.Empty(groupResponse.Parties);
        }

        [Fact]
        public async Task GetById_ReturnsFavoriteGroup_WhenGroupIsFavorite()
        {
            // Arrange
            const int UserId = 2516356;
            const int GroupId = 1;

            var group = new Group
            {
                GroupId = GroupId,
                UserId = UserId,
                Name = "Favorites",
                IsFavorite = true,
                Parties = [new PartyGroupAssociation { PartyUuid = Guid.NewGuid() }]
            };

            _factory.PartyGroupRepositoryMock
                .Setup(x => x.GetGroup(UserId, GroupId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(group);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Get, UserId, $"profile/api/v1/users/current/party-groups/{GroupId}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            GroupResponse groupResponse = JsonSerializer.Deserialize<GroupResponse>(
                responseContent, _serializerOptionsCamelCase);

            Assert.NotNull(groupResponse);
            Assert.True(groupResponse.IsFavorite);
            Assert.Equal("Favorites", groupResponse.Name);
        }

        [Fact]
        public async Task UpdateName_ReturnsUpdatedGroup_WhenRequestIsValid()
        {
            // Arrange
            const int UserId = 2516356;
            const int GroupId = 42;
            const string UpdatedName = "Updated Group Name";

            var updatedGroup = new Group
            {
                GroupId = GroupId,
                UserId = UserId,
                Name = UpdatedName,
                IsFavorite = false,
                Parties = [new PartyGroupAssociation { PartyUuid = Guid.NewGuid() }]
            };

            _factory.PartyGroupRepositoryMock
                .Setup(x => x.UpdateGroupName(UserId, GroupId, UpdatedName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateGroupResult(GroupOperationResult.Success, updatedGroup));

            HttpClient client = _factory.CreateClient();

            var requestBody = new GroupRequest { Name = UpdatedName };
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody, _serializerOptionsCamelCase),
                System.Text.Encoding.UTF8,
                "application/json");

            HttpRequestMessage httpRequestMessage = CreateRequest(new HttpMethod("PATCH"), UserId, $"profile/api/v1/users/current/party-groups/{GroupId}");
            httpRequestMessage.Content = content;

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            GroupResponse groupResponse = JsonSerializer.Deserialize<GroupResponse>(
                responseContent, _serializerOptionsCamelCase);

            Assert.NotNull(groupResponse);
            Assert.Equal(GroupId, groupResponse.GroupId);
            Assert.Equal(UpdatedName, groupResponse.Name);
            Assert.False(groupResponse.IsFavorite);
            Assert.Single(groupResponse.Parties);

            _factory.PartyGroupRepositoryMock.Verify(
                x => x.UpdateGroupName(UserId, GroupId, UpdatedName, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateName_ReturnsNotFound_WhenGroupDoesNotExist()
        {
            // Arrange
            const int UserId = 2516356;
            const int GroupId = 999;
            const string UpdatedName = "New Name";

            _factory.PartyGroupRepositoryMock
                .Setup(x => x.UpdateGroupName(UserId, GroupId, UpdatedName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateGroupResult(GroupOperationResult.NotFound, null));

            HttpClient client = _factory.CreateClient();

            var requestBody = new GroupRequest { Name = UpdatedName };
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody, _serializerOptionsCamelCase),
                System.Text.Encoding.UTF8,
                "application/json");

            HttpRequestMessage httpRequestMessage = CreateRequest(new HttpMethod("PATCH"), UserId, $"profile/api/v1/users/current/party-groups/{GroupId}");
            httpRequestMessage.Content = content;

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            _factory.PartyGroupRepositoryMock.Verify(
                x => x.UpdateGroupName(UserId, GroupId, UpdatedName, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task UpdateName_ReturnsBadRequest_WhenNameIsMissing()
        {
            // Arrange
            const int UserId = 2516356;
            const int GroupId = 42;

            HttpClient client = _factory.CreateClient();

            var requestBody = new { };
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody, _serializerOptionsCamelCase),
                System.Text.Encoding.UTF8,
                "application/json");

            HttpRequestMessage httpRequestMessage = CreateRequest(new HttpMethod("PATCH"), UserId, $"profile/api/v1/users/current/party-groups/{GroupId}");
            httpRequestMessage.Content = content;

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            _factory.PartyGroupRepositoryMock.Verify(
                x => x.UpdateGroupName(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateName_ReturnsBadRequest_WhenNameIsEmpty()
        {
            // Arrange
            const int UserId = 2516356;
            const int GroupId = 42;

            HttpClient client = _factory.CreateClient();

            var requestBody = new { name = string.Empty };
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody, _serializerOptionsCamelCase),
                System.Text.Encoding.UTF8,
                "application/json");

            HttpRequestMessage httpRequestMessage = CreateRequest(new HttpMethod("PATCH"), UserId, $"profile/api/v1/users/current/party-groups/{GroupId}");
            httpRequestMessage.Content = content;

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            _factory.PartyGroupRepositoryMock.Verify(
                x => x.UpdateGroupName(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateName_ReturnsUnauthorized_WhenNoUserIdClaim()
        {
            // Arrange
            const int GroupId = 42;

            HttpClient client = _factory.CreateClient();

            var requestBody = new GroupRequest { Name = "Updated Name" };
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody, _serializerOptionsCamelCase),
                System.Text.Encoding.UTF8,
                "application/json");

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"profile/api/v1/users/current/party-groups/{GroupId}")
            {
                Content = content
            };

            // Act
            HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            _factory.PartyGroupRepositoryMock.Verify(
                x => x.UpdateGroupName(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateName_PreservesPartiesAndIsFavorite_WhenUpdatingName()
        {
            // Arrange
            const int UserId = 2516356;
            const int GroupId = 42;
            const string UpdatedName = "New Name";
            var partyUuid1 = Guid.NewGuid();
            var partyUuid2 = Guid.NewGuid();

            var updatedGroup = new Group
            {
                GroupId = GroupId,
                UserId = UserId,
                Name = UpdatedName,
                IsFavorite = false,
                Parties = [
                    new PartyGroupAssociation { PartyUuid = partyUuid1 },
                    new PartyGroupAssociation { PartyUuid = partyUuid2 }
                ]
            };

            _factory.PartyGroupRepositoryMock
                .Setup(x => x.UpdateGroupName(UserId, GroupId, UpdatedName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateGroupResult(GroupOperationResult.Success, updatedGroup));

            HttpClient client = _factory.CreateClient();

            var requestBody = new GroupRequest { Name = UpdatedName };
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody, _serializerOptionsCamelCase),
                System.Text.Encoding.UTF8,
                "application/json");

            HttpRequestMessage httpRequestMessage = CreateRequest(new HttpMethod("PATCH"), UserId, $"profile/api/v1/users/current/party-groups/{GroupId}");
            httpRequestMessage.Content = content;

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            GroupResponse groupResponse = JsonSerializer.Deserialize<GroupResponse>(
                responseContent, _serializerOptionsCamelCase);

            Assert.NotNull(groupResponse);
            Assert.Equal(UpdatedName, groupResponse.Name);
            Assert.Equal(2, groupResponse.Parties.Length);
            Assert.Contains(partyUuid1, groupResponse.Parties);
            Assert.Contains(partyUuid2, groupResponse.Parties);
        }

        [Fact]
        public async Task UpdateName_ReturnsUnprocessableEntity_WhenGroupIsFavorite()
        {
            // Arrange
            const int UserId = 2516356;
            const int GroupId = 1;
            const string UpdatedName = "New Name";

            _factory.PartyGroupRepositoryMock
                .Setup(x => x.UpdateGroupName(UserId, GroupId, UpdatedName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UpdateGroupResult(GroupOperationResult.Forbidden, null));

            HttpClient client = _factory.CreateClient();

            var requestBody = new GroupRequest { Name = UpdatedName };
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody, _serializerOptionsCamelCase),
                System.Text.Encoding.UTF8,
                "application/json");

            HttpRequestMessage httpRequestMessage = CreateRequest(new HttpMethod("PATCH"), UserId, $"profile/api/v1/users/current/party-groups/{GroupId}");
            httpRequestMessage.Content = content;

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            _factory.PartyGroupRepositoryMock.Verify(
                x => x.UpdateGroupName(UserId, GroupId, UpdatedName, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Delete_ReturnsNoContent_WhenGroupIsDeleted()
        {
            // Arrange
            const int UserId = 2516356;
            const int GroupId = 42;

            _factory.PartyGroupRepositoryMock
                .Setup(x => x.DeleteGroup(UserId, GroupId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(GroupOperationResult.Success);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Delete, UserId, $"profile/api/v1/users/current/party-groups/{GroupId}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            _factory.PartyGroupRepositoryMock.Verify(
                x => x.DeleteGroup(UserId, GroupId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Delete_ReturnsNotFound_WhenGroupDoesNotExist()
        {
            // Arrange
            const int UserId = 2516356;
            const int GroupId = 999;

            _factory.PartyGroupRepositoryMock
                .Setup(x => x.DeleteGroup(UserId, GroupId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(GroupOperationResult.NotFound);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Delete, UserId, $"profile/api/v1/users/current/party-groups/{GroupId}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

            _factory.PartyGroupRepositoryMock.Verify(
                x => x.DeleteGroup(UserId, GroupId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Delete_ReturnsUnprocessableEntity_WhenGroupIsFavorite()
        {
            // Arrange
            const int UserId = 2516356;
            const int GroupId = 1;

            _factory.PartyGroupRepositoryMock
                .Setup(x => x.DeleteGroup(UserId, GroupId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(GroupOperationResult.Forbidden);

            HttpClient client = _factory.CreateClient();
            HttpRequestMessage httpRequestMessage = CreateRequest(HttpMethod.Delete, UserId, $"profile/api/v1/users/current/party-groups/{GroupId}");

            // Act
            HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);

            _factory.PartyGroupRepositoryMock.Verify(
                x => x.DeleteGroup(UserId, GroupId, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Delete_ReturnsUnauthorized_WhenNoUserIdClaim()
        {
            // Arrange
            const int GroupId = 42;

            HttpClient client = _factory.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Delete, $"profile/api/v1/users/current/party-groups/{GroupId}");

            // Act
            HttpResponseMessage response = await client.SendAsync(request, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

            _factory.PartyGroupRepositoryMock.Verify(
                x => x.DeleteGroup(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
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
