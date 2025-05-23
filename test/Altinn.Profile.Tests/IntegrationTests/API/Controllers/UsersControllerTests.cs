#nullable enable

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

using Altinn.Platform.Profile.Models;
using Altinn.Profile.Controllers;
using Altinn.Profile.Integrations.SblBridge;
using Altinn.Profile.Tests.IntegrationTests.Mocks;
using Altinn.Profile.Tests.IntegrationTests.Utils;
using Altinn.Profile.Tests.Testdata;

using Microsoft.AspNetCore.Mvc.Testing;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers;

public class UsersControllerTests : IClassFixture<WebApplicationFactory<UsersController>>
{
    private readonly WebApplicationFactorySetup<UsersController> _webApplicationFactorySetup;

    private readonly JsonSerializerOptions _serializerOptionsCamelCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public UsersControllerTests(WebApplicationFactory<UsersController> factory)
    {
        _webApplicationFactorySetup = new WebApplicationFactorySetup<UsersController>(factory);

        SblBridgeSettings sblBrideSettings = new() { ApiProfileEndpoint = "http://localhost/" };
        _webApplicationFactorySetup.SblBridgeSettingsOptions.Setup(s => s.Value).Returns(sblBrideSettings);
    }

    [Fact]
    public async Task GetUsersCurrent_SblBridgeFindsProfile_ResponseOk_ReturnsUserProfile()
    {
        // Arrange
        const int UserId = 2516356;

        HttpRequestMessage? sblRequest = null;
        DelegatingHandlerStub messageHandler = new(async (request, token) =>
        {
            sblRequest = request;

            UserProfile userProfile = await TestDataLoader.Load<UserProfile>(UserId.ToString());
            return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
        });
        _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

        HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, "/profile/api/v1/users/current");

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        // Assert
        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"users/{UserId}", sblRequest?.RequestUri?.ToString());

        string responseContent = await response.Content.ReadAsStringAsync();

        UserProfile? actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, _serializerOptionsCamelCase);
        
        Assert.NotNull(actualUser);

        // These asserts check that deserializing with camel casing was successful.
        Assert.Equal(UserId, actualUser.UserId);
        Assert.Equal("sophie", actualUser.UserName);
        Assert.Equal("Sophie Salt", actualUser.Party.Name);
        Assert.Equal("Sophie", actualUser.Party.Person.FirstName);
        Assert.Equal("nb", actualUser.ProfileSettingPreference.Language);
    }

    [Fact]
    public async Task GetUsersCurrent_AsOrg_ResponseBadRequest()
    {
        // Arrange
        const int UserId = 2516356;

        HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, "/profile/api/v1/users/current");

        string token = PrincipalUtil.GetOrgToken("ttd");
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        // Assert
        string responseContent = await response.Content.ReadAsStringAsync();

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

        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        // Assert
        string responseContent = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("UserId must be provided in claims", responseContent);
    }

    [Fact]
    public async Task GetUsersById_AsUser_SblBridgeFindsProfile_ResponseOk_ReturnsUserProfile()
    {
        // Arrange
        const int UserId = 2516356;

        HttpRequestMessage? sblRequest = null;
        DelegatingHandlerStub messageHandler = new(async (request, token) =>
        {
            sblRequest = request;

            UserProfile userProfile = await TestDataLoader.Load<UserProfile>(UserId.ToString());
            return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
        });
        _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

        HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, $"/profile/api/v1/users/{UserId}");

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        // Assert
        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"users/{UserId}", sblRequest.RequestUri?.ToString());

        string responseContent = await response.Content.ReadAsStringAsync();

        UserProfile? actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, _serializerOptionsCamelCase);
        
        Assert.NotNull(actualUser);

        // These asserts check that deserializing with camel casing was successful.
        Assert.Equal(UserId, actualUser.UserId);
        Assert.Equal("sophie", actualUser.UserName);
        Assert.Equal("Sophie Salt", actualUser.Party.Name);
        Assert.Equal("Sophie", actualUser.Party.Person.FirstName);
        Assert.Equal("nb", actualUser.ProfileSettingPreference.Language);
    }

    [Fact]
    public async Task GetUsersById_AsOrg_SblBridgeFindsProfile_ResponseOk_ReturnsUserProfile()
    {
        // Arrange
        const int UserId = 2516356;

        HttpRequestMessage? sblRequest = null;
        DelegatingHandlerStub messageHandler = new(async (request, token) =>
        {
            sblRequest = request;

            UserProfile userProfile = await TestDataLoader.Load<UserProfile>(UserId.ToString());
            return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
        });
        _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/users/{UserId}");
        string token = PrincipalUtil.GetOrgToken("ttd");
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        // Assert
        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"users/{UserId}", sblRequest.RequestUri?.ToString());

        string responseContent = await response.Content.ReadAsStringAsync();

        UserProfile? actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, _serializerOptionsCamelCase);
        
        Assert.NotNull(actualUser);

        // These asserts check that deserializing with camel casing was successful.
        Assert.Equal(UserId, actualUser.UserId);
        Assert.Equal("sophie", actualUser.UserName);
        Assert.Equal("Sophie Salt", actualUser.Party.Name);
        Assert.Equal("Sophie", actualUser.Party.Person.FirstName);
        Assert.Equal("nb", actualUser.ProfileSettingPreference.Language);
    }

    [Fact]
    public async Task GetUsersById_AsSystemUser_SblBridgeFindsProfile_ResponseOk_ReturnsUserProfile()
    {
        // Arrange
        const int UserId = 2516356;

        HttpRequestMessage? sblRequest = null;
        DelegatingHandlerStub messageHandler = new(async (request, token) =>
        {
            sblRequest = request;

            UserProfile userProfile = await TestDataLoader.Load<UserProfile>(UserId.ToString());
            return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
        });
        _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/users/{UserId}");
        string token = PrincipalUtil.GetSystemUserToken(Guid.NewGuid());
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        // Assert
        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"users/{UserId}", sblRequest.RequestUri?.ToString());

        string responseContent = await response.Content.ReadAsStringAsync();

        UserProfile? actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, _serializerOptionsCamelCase);

        Assert.NotNull(actualUser);

        // These asserts check that deserializing with camel casing was successful.
        Assert.Equal(UserId, actualUser.UserId);
        Assert.Equal("sophie", actualUser.UserName);
        Assert.Equal("Sophie Salt", actualUser.Party.Name);
        Assert.Equal("Sophie", actualUser.Party.Person.FirstName);
        Assert.Equal("nb", actualUser.ProfileSettingPreference.Language);
    }

    [Fact]
    public async Task GetUsersById_AsInvalidSystemUser_SblBridgeFindsProfile_ResponseOk_ReturnsUserProfile()
    {
        //// The content of the bearer token is invalid, but the profile doesn't check the content of the token. It's only
        //// checking that the token is valid by verifying the signature. The purpose of the test is to trigger an
        //// exception during telemetry enrichment from the claims principal.

        // Arrange
        const int UserId = 2516356;

        HttpRequestMessage? sblRequest = null;
        DelegatingHandlerStub messageHandler = new(async (request, token) =>
        {
            sblRequest = request;

            UserProfile userProfile = await TestDataLoader.Load<UserProfile>(UserId.ToString());
            return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
        });
        _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/users/{UserId}");
        string token = PrincipalUtil.GetInvalidSystemUserToken(Guid.NewGuid());
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

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
        DelegatingHandlerStub messageHandler = new(async (request, token) =>
        {
            sblRequest = request;

            UserProfile userProfile = await TestDataLoader.Load<UserProfile>(userUuid.ToString());
            return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
        });
        _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

        HttpRequestMessage httpRequestMessage = CreateGetRequest(userId, $"/profile/api/v1/users/byuuid/{userUuid}");

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        // Assert
        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Get, sblRequest.Method);
        Assert.EndsWith($"users?useruuid={userUuid}", sblRequest.RequestUri?.ToString());

        string responseContent = await response.Content.ReadAsStringAsync();

        UserProfile? actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, _serializerOptionsCamelCase);
        
        Assert.NotNull(actualUser);

        // These asserts check that deserializing with camel casing was successful.
        Assert.Equal(userId, actualUser.UserId);
        Assert.Equal(userUuid, actualUser.UserUuid);
        Assert.Equal("LEO WILHELMSEN", actualUser.Party.Name);
        Assert.Equal(userUuid, actualUser.Party.PartyUuid);
        Assert.Equal("LEO", actualUser.Party.Person.FirstName);
        Assert.Equal("nb", actualUser.ProfileSettingPreference.Language);
    }

    [Fact]
    public async Task GetUsersByUuid_AsUser_UserAuthenticatedMissingPlatformAccesToken_ReturnsForbidden()
    {
        // Arrange
        const int userId = 20000009;
        Guid userUuid = new("cc86d2c7-1695-44b0-8e82-e633243fdf31");

        HttpRequestMessage httpRequestMessage = CreateGetRequest(userId, $"/profile/api/v1/users/byuuid/{userUuid}");

        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

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
        DelegatingHandlerStub messageHandler = new(async (request, token) =>
        {
            sblRequest = request;

            return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.NotFound });
        });
        _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

        HttpRequestMessage httpRequestMessage = CreateGetRequest(userId, $"/profile/api/v1/users/byuuid/{userUuid}");

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

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

        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUsersById_AsUser_SblBridgeReturnsNotFound_ResponseNotFound()
    {
        // Arrange
        const int UserId = 2222222;

        HttpRequestMessage? sblRequest = null;
        DelegatingHandlerStub messageHandler = new(async (request, token) =>
        {
            sblRequest = request;

            return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.NotFound });
        });
        _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

        HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, $"/profile/api/v1/users/{UserId}");

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

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
        DelegatingHandlerStub messageHandler = new(async (request, token) =>
        {
            sblRequest = request;

            return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.ServiceUnavailable });
        });
        _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

        HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, $"/profile/api/v1/users/{UserId}");

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

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
        HttpRequestMessage? sblRequest = null;
        DelegatingHandlerStub messageHandler = new(async (request, token) =>
        {
            sblRequest = request;

            UserProfile userProfile = await TestDataLoader.Load<UserProfile>("2516356");
            return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
        });
        _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

        StringContent content = new("\"01017512345\"", Encoding.UTF8, "application/json");
        HttpRequestMessage httpRequestMessage = CreatePostRequest(2222222, $"/profile/api/v1/users/", content);

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        // Assert
        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Post, sblRequest.Method);
        Assert.EndsWith($"users", sblRequest.RequestUri?.ToString());

        Assert.NotNull(sblRequest.Content);

        string? requestContent = await sblRequest.Content.ReadAsStringAsync();

        Assert.Equal("\"01017512345\"", requestContent);

        string responseContent = await response.Content.ReadAsStringAsync();

        UserProfile? actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, _serializerOptionsCamelCase);
        
        Assert.NotNull(actualUser);

        // These asserts check that deserializing with camel casing was successful.
        Assert.Equal(2516356, actualUser.UserId);
        Assert.Equal("sophie", actualUser.UserName);
        Assert.Equal("Sophie Salt", actualUser.Party.Name);
        Assert.Equal("Sophie", actualUser.Party.Person.FirstName);
        Assert.Equal("nb", actualUser.ProfileSettingPreference.Language);
    }

    [Fact]
    public async Task GetUsersBySsn_AsUser_SblBridgeReturnsNotFound_RespondsNotFound()
    {
        // Arrange
        HttpRequestMessage? sblRequest = null;
        DelegatingHandlerStub messageHandler = new(async (request, token) =>
        {
            sblRequest = request;

            return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.NotFound });
        });
        _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

        StringContent content = new("\"01017512345\"", Encoding.UTF8, "application/json");
        HttpRequestMessage httpRequestMessage = CreatePostRequest(2222222, $"/profile/api/v1/users/", content);

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Post, sblRequest.Method);
        Assert.EndsWith($"users", sblRequest.RequestUri?.ToString());

        Assert.NotNull(sblRequest.Content);

        string requestContent = await sblRequest.Content.ReadAsStringAsync();

        Assert.Equal("\"01017512345\"", requestContent);
    }

    [Fact]
    public async Task GetUsersBySsn_AsUser_SblBridgeReturnsUnavailable_RespondsNotFound()
    {
        // Arrange
        HttpRequestMessage? sblRequest = null;
        DelegatingHandlerStub messageHandler = new(async (request, token) =>
        {
            sblRequest = request;

            return await Task.FromResult(new HttpResponseMessage() { StatusCode = HttpStatusCode.ServiceUnavailable });
        });
        _webApplicationFactorySetup.SblBridgeHttpMessageHandler = messageHandler;

        StringContent content = new("\"01017512345\"", Encoding.UTF8, "application/json");
        HttpRequestMessage httpRequestMessage = CreatePostRequest(2222222, $"/profile/api/v1/users/", content);

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _webApplicationFactorySetup.GetTestServerClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        Assert.NotNull(sblRequest);
        Assert.Equal(HttpMethod.Post, sblRequest.Method);
        Assert.EndsWith($"users", sblRequest.RequestUri?.ToString());

        Assert.NotNull(sblRequest.Content);

        string requestContent = await sblRequest.Content.ReadAsStringAsync();

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
