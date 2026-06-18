using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Authorization.ModelUtils;
using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.Testdata;
using Altinn.Register.Contracts;
using Altinn.Register.Contracts.Testing;

using Microsoft.Extensions.Configuration;

using Moq;

using Xunit;

using static Altinn.Register.Contracts.PartyUrn;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

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
        _factory.InMemoryConfigurationCollection.Clear();
        _factory.ProfileSettingsRepositoryMock.Reset();

        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction((request, token) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)));
        _factory.RegisterHttpMessageHandler.ChangeHandlerFunction((request, token) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)));
    }

    [Fact]
    public async Task GetUserById_RegisterFindsProfile_ResponseOk_ReturnsUserProfile()
    {
        // Arrange
        const int UserId = 2516356;
        Person registerPerson = Person.Minimal("14836498780") with
        {
            PartyId = 987654,
            Uuid = Guid.NewGuid(),
            ShortName = "Register Person",
            FirstName = "Register",
            LastName = "Person",
            ModifiedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
            User = new PartyUser(UserId, "register.person", ImmutableValueArray<uint>.Empty.Add(UserId))
        };

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, registerPerson);

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { UserId = UserId });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        UserProfile actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, serializerOptionsCamelCase);

        // These asserts check that deserializing with camel casing was successful.
        Assert.Equal(UserId, actualUser.UserId);
        Assert.Equal("Register Person", actualUser.Party.Name);
        Assert.Equal("Register", actualUser.Party.Person.FirstName);
        Assert.Equal("nb", actualUser.ProfileSettingPreference.Language);
        Assert.Equal("register.person", actualUser.UserName);
    }

    [Fact]
    public async Task GetUserByUuid_RegisterFindsProfile_ResponseOk_ReturnsUserProfile()
    {
        // Arrange
        Guid userUuid = new("cc86d2c7-1695-44b0-8e82-e633243fdf31");
        Person registerPerson = Person.Minimal("14836498780") with
        {
            PartyId = 987654,
            Uuid = userUuid,
            ShortName = "LEO WILHELMSEN",
            FirstName = "LEO",
            LastName = "WILHELMSEN",
            ModifiedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
        };

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, registerPerson);

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { UserUuid = userUuid });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
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
    public async Task GetUserById_RegisterReturnsNotFound_ResponseNotFound()
    {
        // Arrange
        const int UserId = 2222222;

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, null, HttpStatusCode.PartialContent);

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { UserId = UserId });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserByUuid_RegisterReturnsNotFound_ResponseNotFound()
    {
        // Arrange
        Guid userUuid = new("cc86d2c7-1695-44b0-8e82-e633243fdf31");

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, null, HttpStatusCode.PartialContent);

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { UserUuid = userUuid });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_RegisterReturnsUnavailable_ResponseNotFound()
    {
        // Arrange
        const int UserId = 2222222;

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory,  null, HttpStatusCode.ServiceUnavailable);

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { UserId = UserId });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserByUuid_RegisterReturnsServiceUnavailable_ResponseNotFound()
    {
        // Arrange
        Guid userUuid = new("cc86d2c7-1695-44b0-8e82-e633243fdf31");

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, null, HttpStatusCode.ServiceUnavailable);

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { UserUuid = userUuid });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserBySsn_RegisterFindsProfile_ReturnsUserProfile()
    {
        // Arrange
        const string Ssn = "14836498780";
        Person registerPerson = Person.Minimal(Ssn) with
        {
            PartyId = 987654,
            Uuid = Guid.NewGuid(),
            ShortName = "Register Person",
            FirstName = "Register",
            LastName = "Person",
            ModifiedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
            User = new PartyUser(987654, "register.person", ImmutableValueArray<uint>.Empty.Add(987654))
        };

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, registerPerson);

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { Ssn = Ssn });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        UserProfile actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, serializerOptionsCamelCase);

        // These asserts check that deserializing with camel casing was successful.
        Assert.Equal(987654, actualUser.UserId);
        Assert.Equal("register.person", actualUser.UserName);
        Assert.Equal("Register Person", actualUser.Party.Name);
        Assert.Equal("Register", actualUser.Party.Person.FirstName);
        Assert.Equal("nb", actualUser.ProfileSettingPreference.Language);
    }

    [Fact]
    public async Task GetUserBySsn_RegisterReturnsNotFound_RespondsNotFound()
    {
        // Arrange
        const string Ssn = "01017512345";
        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, null, HttpStatusCode.PartialContent);

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { Ssn = Ssn });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserBySsn_RegisterReturnsServiceUnavailable_RespondsNotFound()
    {
        // Arrange
        const string Ssn = "01017512345";
        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, null, HttpStatusCode.ServiceUnavailable);

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { Ssn = Ssn });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserByUsername_RegisterFindsProfile_ResponseOk_ReturnsUserProfile()
    {
        // Arrange
        const string Username = "OrstaECUser";

        Person registerPerson = Person.Minimal("14836498780") with
        {
            PartyId = 987654,
            Uuid = Guid.NewGuid(),
            ShortName = "Register Person",
            FirstName = "Register",
            LastName = "Person",
            ModifiedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
            User = new PartyUser(987654, Username, ImmutableValueArray<uint>.Empty.Add(987654))
        };

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, registerPerson);

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { Username = Username });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        UserProfile actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, serializerOptionsCamelCase);

        // These asserts check that deserializing with camel casing was successful.
        Assert.Equal(Username, actualUser.UserName);
        Assert.Equal(987654, actualUser.Party.PartyId);
        Assert.Equal("Register Person", actualUser.Party.Name);
        Assert.Equal("nb", actualUser.ProfileSettingPreference.Language);
    }

    [Fact]
    public async Task GetUserByUsername_RegisterFindsProfile_ResponseOk_ReturnsWithDefaultValues()
    {
        // Arrange
        const int UserId = 1002356;
        const string username = "sophie";

        Person registerPerson = Person.Minimal("14836498780") with
        {
            PartyId = 987654,
            Uuid = Guid.NewGuid(),
            ShortName = "Sophie Salt",
            FirstName = "Sophie",
            LastName = "Salt",
            ModifiedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
            User = new PartyUser(UserId, username, ImmutableValueArray<uint>.Empty.Add(UserId))
        };

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, registerPerson);
        _factory.ProfileSettingsRepositoryMock.Setup(m => m.GetProfileSettings(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProfileSettings)null);

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { Username = username });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

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
        Assert.True(actualUser.ProfileSettingPreference.ShouldShowSubEntities);
    }

    [Fact]
    public async Task GetUserByUsername_RegisterReturnsNotFound_ResponseNotFound()
    {
        // Arrange
        const string Username = "NonExistingUsername";

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, null, HttpStatusCode.PartialContent);

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { Username = Username });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserByUsername_RegisterReturnsServiceUnavailable_ResponseNotFound()
    {
        // Arrange
        const string Username = "OrstaECUser";

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, null, HttpStatusCode.ServiceUnavailable);

        HttpRequestMessage httpRequestMessage = CreatePostRequest($"/profile/api/v1/internal/user/", new UserProfileLookup { Username = Username });

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
}
