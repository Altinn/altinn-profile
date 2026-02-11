using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Models;

using Altinn.Profile.Tests.Testdata;

using Moq;

using Xunit;

using static Altinn.Register.Contracts.PartyUrn;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers;

public class UserProfileInternalControllerTests : IClassFixture<ProfileWebApplicationFactory<Program>>
{
    private readonly JsonSerializerOptions serializerOptionsCamelCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    private readonly ProfileWebApplicationFactory<Program> _factory;

    public UserProfileInternalControllerTests(ProfileWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _factory.MemoryCache.Clear();
    }

    [Fact]
    public async Task GetUserById_SblBridgeFindsProfile_ResponseOk_ReturnsUserProfile()
    {
        // Arrange
        const int UserId = 2516356;

        HttpRequestMessage sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            UserProfile userProfile = await TestDataLoader.Load<UserProfile>(UserId.ToString());
            return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
        });

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { UserId = UserId });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"sblbridge/profile/api/users/{UserId}", sblRequest.RequestUri.ToString());

        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        UserProfile actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, serializerOptionsCamelCase);

        // These asserts check that deserializing with camel casing was successful.
        Assert.Equal(UserId, actualUser.UserId);
        Assert.Equal("sophie", actualUser.UserName);
        Assert.Equal("Sophie Salt", actualUser.Party.Name);
        Assert.Equal("Sophie", actualUser.Party.Person.FirstName);
        Assert.Equal("nb", actualUser.ProfileSettingPreference.Language);
    }

    [Fact]
    public async Task GetUserByUuid_SblBridgeFindsProfile_ResponseOk_ReturnsUserProfile()
    {
        // Arrange
        Guid userUuid = new("cc86d2c7-1695-44b0-8e82-e633243fdf31");

        HttpRequestMessage sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            UserProfile userProfile = await TestDataLoader.Load<UserProfile>(userUuid.ToString());
            return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
        });

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { UserUuid = userUuid });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"sblbridge/profile/api/users?useruuid={userUuid}", sblRequest.RequestUri.ToString());

        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        UserProfile actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, serializerOptionsCamelCase);

        // These asserts check that deserializing with camel casing was successful.
        Assert.Equal(userUuid, actualUser.UserUuid);
        Assert.Equal("LEO WILHELMSEN", actualUser.Party.Name);
        Assert.Equal("LEO", actualUser.Party.Person.FirstName);
        Assert.Equal("nb", actualUser.ProfileSettingPreference.Language);
    }

    [Fact]
    public async Task GetUserListByUuid_SblBridgeFindsProfile_ResponseOk_ReturnsUserProfileList()
    {
        // Arrange
        List<Guid> userUuids = new List<Guid> { new("cc86d2c7-1695-44b0-8e82-e633243fdf31"), new("4c3b4909-eb17-45d5-bde1-256e065e196a") };

        HttpRequestMessage sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            List<UserProfile> userProfiles = new()
            {
                await TestDataLoader.Load<UserProfile>(userUuids[0].ToString()),
                await TestDataLoader.Load<UserProfile>(userUuids[1].ToString())
            };

            return new HttpResponseMessage() { Content = JsonContent.Create(userProfiles) };
        });

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/listbyuuid", userUuids);

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Post, sblRequest.Method);
        Assert.EndsWith($"sblbridge/profile/api/users/byuuid", sblRequest.RequestUri.ToString());

        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        List<UserProfile> actualUsers = JsonSerializer.Deserialize<List<UserProfile>>(
            responseContent, serializerOptionsCamelCase);

        // These asserts check that deserializing with camel casing was successful.
        Assert.Equal(userUuids[0], actualUsers[0].UserUuid);
        Assert.Equal("LEO WILHELMSEN", actualUsers[0].Party.Name);
        Assert.Equal("LEO", actualUsers[0].Party.Person.FirstName);
        Assert.Equal("nb", actualUsers[0].ProfileSettingPreference.Language);

        Assert.Equal(userUuids[1], actualUsers[1].UserUuid);
        Assert.Equal("ELENA FJAR", actualUsers[1].Party.Name);
        Assert.Equal("ELENA", actualUsers[1].Party.Person.FirstName);
        Assert.Equal("nn", actualUsers[1].ProfileSettingPreference.Language);
    }

    [Fact]
    public async Task GetUserListByUuid_SblBridgeFindsNoProfile_ResponseOk_ReturnsEmptyProfileList()
    {
        // Arrange
        List<Guid> userUuids = new List<Guid> { new("cc86d2c7-1695-44b0-8e82-e633243fdf31"), new("4c3b4909-eb17-45d5-bde1-256e065e196a") };

        HttpRequestMessage sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            List<UserProfile> userProfiles = new List<UserProfile>();

            return await Task.FromResult(new HttpResponseMessage() { Content = JsonContent.Create(userProfiles) });
        });

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/listbyuuid", userUuids);

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Post, sblRequest.Method);
        Assert.EndsWith($"sblbridge/profile/api/users/byuuid", sblRequest.RequestUri.ToString());

        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        List<UserProfile> actualUsers = JsonSerializer.Deserialize<List<UserProfile>>(
            responseContent, serializerOptionsCamelCase);

        // These asserts check that deserializing with camel casing was successful.
        Assert.NotNull(actualUsers);
        Assert.Empty(actualUsers);
    }

    [Fact]
    public async Task GetUserListByUuid_EmptyInput_ResponseBadRequest_ReturnsBadRequest()
    {
        // Arrange
        List<Guid> userUuids = new List<Guid>();

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/listbyuuid", userUuids);

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_SblBridgeReturnsNotFound_ResponseNotFound()
    {
        // Arrange
        const int UserId = 2222222;

        HttpRequestMessage sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.NotFound });
        });

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { UserId = UserId });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"sblbridge/profile/api/users/{UserId}", sblRequest.RequestUri.ToString());
    }

    [Fact]
    public async Task GetUserByUuid_SblBridgeReturnsNotFound_ResponseNotFound()
    {
        // Arrange
        Guid userUuid = new("cc86d2c7-1695-44b0-8e82-e633243fdf31");

        HttpRequestMessage sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.NotFound });
        });

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { UserUuid = userUuid });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"sblbridge/profile/api/users?useruuid={userUuid}", sblRequest.RequestUri.ToString());
    }

    [Fact]
    public async Task GetUserById_SblBridgeReturnsUnavailable_ResponseNotFound()
    {
        // Arrange
        const int UserId = 2222222;

        HttpRequestMessage sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.ServiceUnavailable });
        });

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { UserId = UserId });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"sblbridge/profile/api/users/{UserId}", sblRequest.RequestUri.ToString());
    }

    [Fact]
    public async Task GetUserByUuid_SblBridgeReturnsUnavailable_ResponseNotFound()
    {
        // Arrange
        Guid userUuid = new("cc86d2c7-1695-44b0-8e82-e633243fdf31");

        HttpRequestMessage sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.ServiceUnavailable });
        });

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { UserUuid = userUuid });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"sblbridge/profile/api/users?useruuid={userUuid}", sblRequest.RequestUri.ToString());
    }

    [Fact]
    public async Task GetUserBySsn_SblBridgeFindsProfile_ReturnsUserProfile()
    {
        // Arrange
        const string Ssn = "01017512345";
        HttpRequestMessage sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            UserProfile userProfile = await TestDataLoader.Load<UserProfile>("2516356");
            return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
        });

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { Ssn = Ssn });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Post, sblRequest.Method);
        Assert.EndsWith($"sblbridge/profile/api/users", sblRequest.RequestUri.ToString());

        string requestContent = await sblRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal($"\"{Ssn}\"", requestContent);

        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        UserProfile actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, serializerOptionsCamelCase);

        // These asserts check that deserializing with camel casing was successful.
        Assert.Equal(2516356, actualUser.UserId);
        Assert.Equal("sophie", actualUser.UserName);
        Assert.Equal("Sophie Salt", actualUser.Party.Name);
        Assert.Equal("Sophie", actualUser.Party.Person.FirstName);
        Assert.Equal("nb", actualUser.ProfileSettingPreference.Language);
    }

    [Fact]
    public async Task GetUserBySsn_SblBridgeReturnsNotFound_RespondsNotFound()
    {
        // Arrange
        const string Ssn = "01017512345";
        HttpRequestMessage sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.NotFound });
        });

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { Ssn = Ssn });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Post, sblRequest.Method);
        Assert.EndsWith($"sblbridge/profile/api/users", sblRequest.RequestUri.ToString());

        string requestContent = await sblRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal($"\"{Ssn}\"", requestContent);
    }

    [Fact]
    public async Task GetUserBySsn_SblBridgeReturnsUnavailable_RespondsNotFound()
    {
        // Arrange
        const string Ssn = "01017512345";
        HttpRequestMessage sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.ServiceUnavailable });
        });

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { Ssn = Ssn });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Post, sblRequest.Method);
        Assert.EndsWith($"sblbridge/profile/api/users", sblRequest.RequestUri.ToString());

        string requestContent = await sblRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal($"\"{Ssn}\"", requestContent);
    }

    [Fact]
    public async Task GetUserByUsername_SblBridgeFindsProfile_ResponseOk_ReturnsUserProfile()
    {
        // Arrange
        const string Username = "OrstaECUser";

        HttpRequestMessage sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            UserProfile userProfile = await TestDataLoader.Load<UserProfile>(Username);
            return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
        });

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { Username = Username });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"sblbridge/profile/api/users/?username={Username}", sblRequest.RequestUri.ToString());

        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        UserProfile actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, serializerOptionsCamelCase);

        // These asserts check that deserializing with camel casing was successful.
        Assert.Equal(Username, actualUser.UserName);
        Assert.Equal(50005545, actualUser.Party.PartyId);
        Assert.Equal("ORSTA OG HEGGEDAL ", actualUser.Party.Name);
        Assert.Equal("ORSTA OG HEGGEDAL", actualUser.Party.Organization.Name);
        Assert.Equal("nb", actualUser.ProfileSettingPreference.Language);
    }

    [Fact]
    public async Task GetUserByUsername_SblBridgeFindsProfileWithoutProfileSettings_ResponseOk_ReturnsWithDefaultValues()
    {
        // Arrange
        const int UserId = 1002356;
        const string username = "sophie";

        HttpRequestMessage sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            UserProfile userProfile = await TestDataLoader.Load<UserProfile>(UserId.ToString());
            return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
        });
        _factory.ProfileSettingsRepositoryMock.Setup(m => m.GetProfileSettings(UserId))
            .ReturnsAsync((ProfileSettings)null);

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { Username = username });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        // Assert
        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"sblbridge/profile/api/users/?username={username}", sblRequest.RequestUri.ToString());

        string responseContent = await response.Content.ReadAsStringAsync();

        UserProfile actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, serializerOptionsCamelCase);

        Assert.NotNull(actualUser);

        // These asserts check that deserializing with camel casing was successful.
        Assert.Equal(UserId, actualUser.UserId);
        Assert.Equal("sophie", actualUser.UserName);
        Assert.Equal("Sophie Salt", actualUser.Party.Name);
        Assert.Equal("Sophie", actualUser.Party.Person?.FirstName);
        Assert.Equal("nb", actualUser.ProfileSettingPreference.Language);
        Assert.False(actualUser.ProfileSettingPreference.DoNotPromptForParty);
        Assert.Null(actualUser.ProfileSettingPreference.PreselectedPartyUuid);
        Assert.False(actualUser.ProfileSettingPreference.ShowClientUnits);
        Assert.False(actualUser.ProfileSettingPreference.ShouldShowDeletedEntities);
        Assert.False(actualUser.ProfileSettingPreference.ShouldShowSubEntities);
    }

    [Fact]
    public async Task GetUserByUsername_SblBridgeReturnsNotFound_ResponseNotFound()
    {
        // Arrange
        const string Username = "NonExistingUsername";

        HttpRequestMessage sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.NotFound });
        });

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { Username = Username });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"sblbridge/profile/api/users/?username={Username}", sblRequest.RequestUri.ToString());
    }

    [Fact]
    public async Task GetUserByUsername_SblBridgeReturnsUnavailable_ResponseNotFound()
    {
        // Arrange
        const string Username = "OrstaECUser";

        HttpRequestMessage sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.ServiceUnavailable });
        });

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { Username = Username });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"sblbridge/profile/api/users/?username={Username}", sblRequest.RequestUri.ToString());
    }

    [Fact]
    public async Task GetUserEmptyInputModel_UserProfileInternalController_ResponseBadRequest()
    {
        // Arrange
        UserProfileLookup emptyInputModel = new UserProfileLookup();

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", emptyInputModel);

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUserNullInputModel_UserProfileInternalController_ResponseBadRequest()
    {
        // Arrange
        UserProfileLookup nullInputModel = null;

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", nullInputModel);

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static HttpRequestMessage CreatePostRequest(string requestUri, UserProfileLookup lookupRequest)
    {
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, requestUri);
        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(lookupRequest), Encoding.UTF8, "application/json");
        return httpRequestMessage;
    }

    private static HttpRequestMessage CreatePostRequest(string requestUri, List<Guid> listRequest)
    {
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, requestUri);
        httpRequestMessage.Content = new StringContent(JsonSerializer.Serialize(listRequest), Encoding.UTF8, "application/json");
        return httpRequestMessage;
    }
}
