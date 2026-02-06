#nullable enable

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.IntegrationTests.Utils;
using Altinn.Profile.Tests.Testdata;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers;

public class UsersControllerTests : IClassFixture<ProfileWebApplicationFactory<Program>>
{
    private readonly ProfileWebApplicationFactory<Program> _factory;

    private readonly JsonSerializerOptions _serializerOptionsCamelCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public UsersControllerTests(ProfileWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _factory.MemoryCache.Clear();
    }

    [Fact]
    public async Task GetUsersCurrent_SblBridgeFindsProfile_ResponseOk_ReturnsUserProfile()
    {
        // Arrange
        const int UserId = 2516356;
        Guid preselectedPartyUuid = Guid.NewGuid();

        HttpRequestMessage? sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            UserProfile userProfile = await TestDataLoader.Load<UserProfile>(UserId.ToString());
            return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
        });

        _factory.ProfileSettingsRepositoryMock.Setup(m => m.GetProfileSettings(UserId))
            .ReturnsAsync(new ProfileSettings
            {
                UserId = UserId,
                LanguageType = "nn",
                DoNotPromptForParty = true,
                PreselectedPartyUuid = preselectedPartyUuid,
            });

        HttpClient client = _factory.CreateClient();

        HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, "/profile/api/v1/users/current");

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"users/{UserId}", sblRequest?.RequestUri?.ToString());

        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        UserProfile? actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, _serializerOptionsCamelCase);
        
        Assert.NotNull(actualUser);

        // These asserts check that deserializing with camel casing was successful.
        Assert.Equal(UserId, actualUser.UserId);
        Assert.Equal("sophie", actualUser.UserName);
        Assert.Equal("Sophie Salt", actualUser.Party.Name);
        Assert.Equal("Sophie", actualUser.Party.Person?.FirstName);
        Assert.Equal("nn", actualUser.ProfileSettingPreference.Language);
        Assert.True(actualUser.ProfileSettingPreference.DoNotPromptForParty);
        Assert.Equal(preselectedPartyUuid, actualUser.ProfileSettingPreference.PreselectedPartyUuid);
        Assert.False(actualUser.ProfileSettingPreference.ShowClientUnits);
        Assert.False(actualUser.ProfileSettingPreference.ShouldShowDeletedEntities);
        Assert.False(actualUser.ProfileSettingPreference.ShouldShowSubEntities);
    }

    [Fact]
    public async Task GetUsersCurrent_AsOrg_ResponseBadRequest()
    {
        // Arrange
        const int UserId = 2516356;

        HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, "/profile/api/v1/users/current");

        string token = PrincipalUtil.GetOrgToken("ttd");
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("UserId must be provided in claims", responseContent);
    }

    [Fact]
    public async Task GetUsersCurrent_AsSystemUser_ResponseBadRequest()
    {
        // Arrange
        const int UserId = 2516356;

        HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, "/profile/api/v1/users/current");

        string token = PrincipalUtil.GetSystemUserToken(Guid.NewGuid());
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("UserId must be provided in claims", responseContent);
    }

    [Fact]
    public async Task GetUsersById_AsUser_SblBridgeFindsProfile_ResponseOk_ReturnsEnrichedUserProfile()
    {
        // Arrange
        const int UserId = 2516356;
        var preselectedPartyUuid = Guid.NewGuid();

        HttpRequestMessage? sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            UserProfile userProfile = await TestDataLoader.Load<UserProfile>(UserId.ToString());
            return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
        });
        _factory.ProfileSettingsRepositoryMock.Setup(m => m.GetProfileSettings(UserId))
            .ReturnsAsync(new ProfileSettings
            {
                UserId = UserId,
                LanguageType = "en",
                DoNotPromptForParty = true,
                PreselectedPartyUuid = preselectedPartyUuid,
                ShouldShowSubEntities = true,
                ShowClientUnits = true,
                ShouldShowDeletedEntities = false,
            });

        HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, $"/profile/api/v1/users/{UserId}");

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"users/{UserId}", sblRequest.RequestUri?.ToString());

        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        UserProfile? actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, _serializerOptionsCamelCase);
        
        Assert.NotNull(actualUser);

        // These asserts check that deserializing with camel casing was successful.
        Assert.Equal(UserId, actualUser.UserId);
        Assert.Equal("sophie", actualUser.UserName);
        Assert.Equal("Sophie Salt", actualUser.Party.Name);
        Assert.Equal("Sophie", actualUser.Party.Person?.FirstName);
        Assert.Equal("en", actualUser.ProfileSettingPreference.Language);
        Assert.True(actualUser.ProfileSettingPreference.DoNotPromptForParty);
        Assert.Equal(preselectedPartyUuid, actualUser.ProfileSettingPreference.PreselectedPartyUuid);
        Assert.True(actualUser.ProfileSettingPreference.ShowClientUnits);
        Assert.False(actualUser.ProfileSettingPreference.ShouldShowDeletedEntities);
        Assert.True(actualUser.ProfileSettingPreference.ShouldShowSubEntities);
    }

    [Fact]
    public async Task GetUsersById_AsOrg_SblBridgeFindsProfile_ResponseOk_ReturnsUserProfile()
    {
        // Arrange
        const int UserId = 2516639;

        HttpRequestMessage? sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            UserProfile userProfile = await TestDataLoader.Load<UserProfile>(UserId.ToString());
            return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
        });

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/users/{UserId}");
        string token = PrincipalUtil.GetOrgToken("ttd");
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"users/{UserId}", sblRequest.RequestUri?.ToString());

        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        UserProfile? actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, _serializerOptionsCamelCase);
        
        Assert.NotNull(actualUser);

        // These asserts check that deserializing with camel casing was successful.
        Assert.Equal(UserId, actualUser.UserId);
        Assert.Equal("franky", actualUser.UserName);
        Assert.Equal("Blå mandag", actualUser.Party.Name);
        Assert.Equal("Blå", actualUser.Party.Person?.FirstName);
        Assert.Equal("nb", actualUser.ProfileSettingPreference.Language);
    }

    [Fact]
    public async Task GetUsersById_AsSystemUser_SblBridgeFindsProfile_ResponseOk_ReturnsUserProfile()
    {
        // Arrange
        const int UserId = 2516639;

        HttpRequestMessage? sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            UserProfile userProfile = await TestDataLoader.Load<UserProfile>(UserId.ToString());
            return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
        });

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/users/{UserId}");
        string token = PrincipalUtil.GetSystemUserToken(Guid.NewGuid());
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"users/{UserId}", sblRequest.RequestUri?.ToString());

        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        UserProfile? actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, _serializerOptionsCamelCase);

        Assert.NotNull(actualUser);

        // These asserts check that deserializing with camel casing was successful.
        Assert.Equal(UserId, actualUser.UserId);
        Assert.Equal("franky", actualUser.UserName);
        Assert.Equal("Blå mandag", actualUser.Party.Name);
        Assert.Equal("Blå", actualUser.Party.Person?.FirstName);
        Assert.Equal("nb", actualUser.ProfileSettingPreference.Language);
    }

    [Fact]
    public async Task GetUsersById_AsInvalidSystemUser_SblBridgeFindsProfile_ResponseOk_ReturnsUserProfile()
    {
        //// The content of the bearer token is invalid, but Profile doesn't check the content of the token. It's only
        //// checking that the token is valid by verifying the signature. The purpose of the test is to ensure we can
        //// handle an exception during deserialization of an invalid authorization_details claims value. The request
        //// processing should continue as normal.

        // Arrange
        const int UserId = 2516356;

        HttpRequestMessage? sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            UserProfile userProfile = await TestDataLoader.Load<UserProfile>(UserId.ToString());
            return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
        });

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/users/{UserId}");
        string token = PrincipalUtil.GetInvalidSystemUserToken(Guid.NewGuid());
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"users/{UserId}", sblRequest.RequestUri?.ToString());
    }

    [Fact]
    public async Task GetUsersByUuid_AsUser_SblBridgeFindsProfile_ResponseOk_ReturnsUserProfile()
    {
        // Arrange
        const int userId = 20000009;
        Guid userUuid = new("cc86d2c7-1695-44b0-8e82-e633243fdf31");

        HttpRequestMessage? sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            UserProfile userProfile = await TestDataLoader.Load<UserProfile>(userUuid.ToString());
            return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
        });
        _factory.ProfileSettingsRepositoryMock.Setup(m => m.GetProfileSettings(userId))
            .ReturnsAsync(new ProfileSettings
            {
                UserId = userId,
                LanguageType = "nb",
                DoNotPromptForParty = true,
                PreselectedPartyUuid = null,
            });

        HttpRequestMessage httpRequestMessage = CreateGetRequest(userId, $"/profile/api/v1/users/byuuid/{userUuid}");

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"users?useruuid={userUuid}", sblRequest.RequestUri?.ToString());

        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        UserProfile? actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, _serializerOptionsCamelCase);
        
        Assert.NotNull(actualUser);

        // These asserts check that deserializing with camel casing was successful.
        Assert.Equal(userId, actualUser.UserId);
        Assert.Equal(userUuid, actualUser.UserUuid);
        Assert.Equal("LEO WILHELMSEN", actualUser.Party.Name);
        Assert.Equal(userUuid, actualUser.Party.PartyUuid);
        Assert.Equal("LEO", actualUser.Party.Person?.FirstName);
        Assert.Equal("nb", actualUser.ProfileSettingPreference.Language);
        Assert.True(actualUser.ProfileSettingPreference.DoNotPromptForParty);
        Assert.Null(actualUser.ProfileSettingPreference.PreselectedPartyUuid);
    }

    [Fact]
    public async Task GetUsersByUuid_AsUser_UserAuthenticatedMissingPlatformAccesToken_ReturnsForbidden()
    {
        // Arrange
        const int userId = 20000009;
        Guid userUuid = new("cc86d2c7-1695-44b0-8e82-e633243fdf31");

        HttpRequestMessage httpRequestMessage = CreateGetRequest(userId, $"/profile/api/v1/users/byuuid/{userUuid}");

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUsersByUuid_AsUser_SblBridgeReturnsNotFound_ResponseNotFound()
    {
        // Arrange
        const int userId = 20000009;
        Guid userUuid = new("cc86d2c7-1695-44b0-8e82-e633243fdf31");

        HttpRequestMessage? sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.NotFound });
        });

        HttpRequestMessage httpRequestMessage = CreateGetRequest(userId, $"/profile/api/v1/users/byuuid/{userUuid}");

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"users?useruuid={userUuid}", sblRequest.RequestUri?.ToString());
    }

    [Fact]
    public async Task GetUsersByUuid_MissingAuthentication_NotAuthorized()
    {
        // Arrange
        Guid userUuid = new("cc86d2c7-1695-44b0-8e82-e633243fdf31");

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/users/byuuid/{userUuid}");

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUsersById_AsUser_SblBridgeReturnsNotFound_ResponseNotFound()
    {
        // Arrange
        const int UserId = 2222222;

        HttpRequestMessage? sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.NotFound });
        });

        HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, $"/profile/api/v1/users/{UserId}");

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"users/{UserId}", sblRequest.RequestUri?.ToString());
    }

    [Fact]
    public async Task GetUsersById_AsUser_SblBridgeReturnsUnavailable_ResponseNotFound()
    {
        // Arrange
        const int UserId = 2222222;

        HttpRequestMessage? sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.ServiceUnavailable });
        });

        HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, $"/profile/api/v1/users/{UserId}");

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"users/{UserId}", sblRequest.RequestUri?.ToString());
    }

    [Fact]
    public async Task GetUsersBySsn_AsUser_SblBridgeFindsProfile_ReturnsUserProfile()
    {
        // Arrange
        const int UserId = 2516356;

        HttpRequestMessage? sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            UserProfile userProfile = await TestDataLoader.Load<UserProfile>(UserId.ToString());
            return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
        });

        _factory.ProfileSettingsRepositoryMock.Setup(m => m.GetProfileSettings(UserId))
            .ReturnsAsync(new ProfileSettings
            {
                UserId = UserId,
                LanguageType = "en",
                DoNotPromptForParty = true,
                PreselectedPartyUuid = null,
            });

        StringContent content = new("\"01017512345\"", Encoding.UTF8, "application/json");
        HttpRequestMessage httpRequestMessage = CreatePostRequest(2222222, $"/profile/api/v1/users/", content);

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Post, sblRequest.Method);
        Assert.EndsWith($"users", sblRequest.RequestUri?.ToString());

        Assert.NotNull(sblRequest.Content);

        string? requestContent = await sblRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal("\"01017512345\"", requestContent);

        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        UserProfile? actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, _serializerOptionsCamelCase);
        
        Assert.NotNull(actualUser);

        // These asserts check that deserializing with camel casing was successful.
        Assert.Equal(2516356, actualUser.UserId);
        Assert.Equal("sophie", actualUser.UserName);
        Assert.Equal("Sophie Salt", actualUser.Party.Name);
        Assert.Equal("Sophie", actualUser.Party.Person?.FirstName);
        Assert.Equal("en", actualUser.ProfileSettingPreference.Language);
        Assert.True(actualUser.ProfileSettingPreference.DoNotPromptForParty);
        Assert.Null(actualUser.ProfileSettingPreference.PreselectedPartyUuid);
    }

    [Fact]
    public async Task GetUsersBySsn_AsUser_SblBridgeReturnsNotFound_RespondsNotFound()
    {
        // Arrange
        HttpRequestMessage? sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.NotFound });
        });

        StringContent content = new("\"01017512345\"", Encoding.UTF8, "application/json");
        HttpRequestMessage httpRequestMessage = CreatePostRequest(2222222, $"/profile/api/v1/users/", content);

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Post, sblRequest.Method);
        Assert.EndsWith($"users", sblRequest.RequestUri?.ToString());

        Assert.NotNull(sblRequest.Content);

        string requestContent = await sblRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal("\"01017512345\"", requestContent);
    }

    [Fact]
    public async Task GetUsersBySsn_AsUser_SblBridgeReturnsUnavailable_RespondsNotFound()
    {
        // Arrange
        HttpRequestMessage? sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;

            return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.ServiceUnavailable });
        });

        StringContent content = new("\"01017512345\"", Encoding.UTF8, "application/json");
        HttpRequestMessage httpRequestMessage = CreatePostRequest(2222222, $"/profile/api/v1/users/", content);

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Post, sblRequest.Method);
        Assert.EndsWith($"users", sblRequest.RequestUri?.ToString());

        Assert.NotNull(sblRequest.Content);

        string requestContent = await sblRequest.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal("\"01017512345\"", requestContent);
    }

    private static HttpRequestMessage CreateGetRequest(int userId, string requestUri)
    {
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, requestUri);
        string token = PrincipalUtil.GetToken(userId);
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return httpRequestMessage;
    }

    private static HttpRequestMessage CreatePostRequest(int userId, string requestUri, StringContent content)
    {
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, requestUri);
        string token = PrincipalUtil.GetToken(userId);
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        httpRequestMessage.Content = content;
        return httpRequestMessage;
    }
}
