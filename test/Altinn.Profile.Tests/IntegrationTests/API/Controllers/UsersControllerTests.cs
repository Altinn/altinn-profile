#nullable enable

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Authorization.ModelUtils;
using Altinn.Profile.Core.Person.ContactPreferences;
using Altinn.Profile.Core.User.ContactInfo;
using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.IntegrationTests.Utils;
using Altinn.Profile.Tests.Testdata;
using Altinn.Register.Contracts;
using Altinn.Register.Contracts.Testing;

using Microsoft.Extensions.Configuration;

using Moq;

using Xunit;

using static Altinn.Register.Contracts.PartyUrn;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers;

public class UsersControllerTests : IClassFixture<ProfileWebApplicationFactory<Program>>
{
    private readonly ProfileWebApplicationFactory<Program> _factory;

    private readonly JsonSerializerOptions _serializerOptionsCamelCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public UsersControllerTests(ProfileWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _factory.InMemoryConfigurationCollection.Clear();
        _factory.MemoryCache.Clear();
        _factory.PersonServiceMock.Reset();
        _factory.ProfileSettingsRepositoryMock.Reset();
        _factory.UserContactInfoRepositoryMock.Reset();
    }

    [Fact]
    public async Task GetUsersCurrent_RegisterFindsProfile_ResponseOk_ReturnsUserProfile()
    {
        // Arrange
        const int UserId = 2516356;
        Guid preselectedPartyUuid = Guid.NewGuid();

        Person registerPerson = Person.Minimal("14836498780") with
        {
            PartyId = 987654,
            Uuid = Guid.NewGuid(),
            ShortName = "Sophie Salt",
            FirstName = "Sophie",
            LastName = "Salt",
            ModifiedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
            User = new PartyUser(UserId, "sophie", ImmutableValueArray<uint>.Empty.Add(UserId))
        };

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyByUserIdAndPartyIdLookup(_factory, UserId, registerPerson, preselectedPartyUuid, 123456);

        _factory.PersonServiceMock.Setup(m => m.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PersonContactPreferences { Email = "test@mail.com", NationalIdentityNumber = "1", MobileNumber = "+4798765432", IsReserved = true }]);
        _factory.ProfileSettingsRepositoryMock.Setup(m => m.GetProfileSettings(UserId, It.IsAny<CancellationToken>()))
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
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        UserProfile? actualUser = 
            JsonSerializer.Deserialize<UserProfile>(responseContent, _serializerOptionsCamelCase);
        
        Assert.NotNull(actualUser);

        // These asserts check that deserializing with camel casing was successful.
        Assert.Equal(UserId, actualUser.UserId);
        Assert.Equal("sophie", actualUser.UserName);
        Assert.Equal("Sophie Salt", actualUser.Party.Name);
        Assert.Equal("Sophie", actualUser.Party.Person?.FirstName);
        Assert.Equal("nn", actualUser.ProfileSettingPreference.Language);
        Assert.True(actualUser.ProfileSettingPreference.DoNotPromptForParty);
        Assert.Equal(preselectedPartyUuid, actualUser.ProfileSettingPreference.PreselectedPartyUuid);
        Assert.Equal(123456, actualUser.ProfileSettingPreference.PreSelectedPartyId);
        Assert.False(actualUser.ProfileSettingPreference.ShowClientUnits);
        Assert.False(actualUser.ProfileSettingPreference.ShouldShowDeletedEntities);
        Assert.False(actualUser.ProfileSettingPreference.ShouldShowSubEntities);
        Assert.True(actualUser.IsReserved);
        Assert.Equal("test@mail.com", actualUser.Email);
        Assert.Equal("+4798765432", actualUser.PhoneNumber);
    }

    [Fact]
    public async Task GetUsersCurrent_RegisterFindsProfile_ResponseOk_ReturnsWithDefaultProfileSettings()
    {
        // Arrange
        const int UserId = 1002356;

        Person registerPerson = Person.Minimal("14836498780") with
        {
            PartyId = 987654,
            Uuid = Guid.NewGuid(),
            ShortName = "Sophie Salt",
            FirstName = "Sophie",
            LastName = "Salt",
            ModifiedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
            User = new PartyUser(UserId, "sophie", ImmutableValueArray<uint>.Empty.Add(UserId))
        };

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, registerPerson);

        _factory.ProfileSettingsRepositoryMock.Setup(m => m.GetProfileSettings(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProfileSettings?)null);

        HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, $"/profile/api/v1/users/current");

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        UserProfile? actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, _serializerOptionsCamelCase);

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
    public async Task GetUsersCurrent_AsOrg_ResponseForbidden()
    {
        // Arrange
        const int UserId = 2516356;

        HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, "/profile/api/v1/users/current");

        string token = PrincipalUtil.GetOrgToken("ttd");
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = 
            await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUsersCurrent_AsSystemUser_ResponseForbidden()
    {
        // Arrange
        const int UserId = 2516356;

        HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, "/profile/api/v1/users/current");

        string token = PrincipalUtil.GetSystemUserToken(Guid.NewGuid());
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = 
            await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUsersById_AsUser_RegisterFindsProfile_ResponseOk_ReturnsEnrichedUserProfile()
    {
        // Arrange
        const int UserId = 2516356;
        var preselectedPartyUuid = Guid.NewGuid();
        Person registerPerson = Person.Minimal("14836498780") with
        {
            PartyId = 987654,
            Uuid = Guid.NewGuid(),
            ShortName = "Sophie Salt",
            FirstName = "Sophie",
            LastName = "Salt",
            ModifiedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
            User = new PartyUser(UserId, "sophie", ImmutableValueArray<uint>.Empty.Add(UserId))
        };

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyByUserIdAndPartyIdLookup(_factory, UserId, registerPerson, preselectedPartyUuid, 123456);

        _factory.ProfileSettingsRepositoryMock.Setup(m => m.GetProfileSettings(UserId, It.IsAny<CancellationToken>()))
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
        _factory.PersonServiceMock.Setup(m => m.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PersonContactPreferences { Email = "test@mail.com", NationalIdentityNumber = "1", MobileNumber = "+4798765432", IsReserved = true }]);

        HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, $"/profile/api/v1/users/{UserId}");

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
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
        Assert.True(actualUser.IsReserved);
        Assert.Equal("test@mail.com", actualUser.Email);
        Assert.Equal("+4798765432", actualUser.PhoneNumber);
        Assert.Equal(123456, actualUser.ProfileSettingPreference.PreSelectedPartyId);
    }

    [Fact]
    public async Task GetUsersById_AsUser_RegisterFindsProfileWithoutProfileSettings_ResponseOk_ReturnsWithDefaultValues()
    {
        // Arrange
        const int UserId = 1002356;

        Person registerPerson = Person.Minimal("14836498780") with
        {
            PartyId = 987654,
            Uuid = Guid.NewGuid(),
            ShortName = "Sophie Salt",
            FirstName = "Sophie",
            LastName = "Salt",
            ModifiedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
            User = new PartyUser(UserId, "sophie", ImmutableValueArray<uint>.Empty.Add(UserId))
        };

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, registerPerson);

        _factory.ProfileSettingsRepositoryMock.Setup(m => m.GetProfileSettings(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProfileSettings?)null);

        HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, $"/profile/api/v1/users/{UserId}");

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        UserProfile? actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, _serializerOptionsCamelCase);

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
    public async Task GetUsersById_AsOrg_RegisterFindsProfile_ResponseOk_ReturnsUserProfile()
    {
        // Arrange
        const int UserId = 2516639;

        Person registerPerson = Person.Minimal("14836498780") with
        {
            PartyId = 987654,
            Uuid = Guid.NewGuid(),
            ShortName = "Blå mandag",
            FirstName = "Blå",
            LastName = "mandag",
            ModifiedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
            User = new PartyUser(UserId, "franky", ImmutableValueArray<uint>.Empty.Add(UserId))
        };

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, registerPerson);

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/users/{UserId}");
        string token = PrincipalUtil.GetOrgToken("ttd");
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
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
    public async Task GetUsersById_AsSystemUser_RegisterFindsProfile_ResponseOk_ReturnsUserProfile()
    {
        // Arrange
        const int UserId = 2516639;

        Person registerPerson = Person.Minimal("14836498780") with
        {
            PartyId = 987654,
            Uuid = Guid.NewGuid(),
            ShortName = "Blå mandag",
            FirstName = "Blå",
            LastName = "mandag",
            ModifiedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
            User = new PartyUser(UserId, "franky", ImmutableValueArray<uint>.Empty.Add(UserId))
        };

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, registerPerson);

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/users/{UserId}");
        string token = PrincipalUtil.GetSystemUserToken(Guid.NewGuid());
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
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
    public async Task GetUsersById_AsInvalidSystemUser_RegisterFindsProfile_ResponseOk_ReturnsUserProfile()
    {
        //// The content of the bearer token is invalid, but Profile doesn't check the content of the token. It's only
        //// checking that the token is valid by verifying the signature. The purpose of the test is to ensure we can
        //// handle an exception during deserialization of an invalid authorization_details claims value. The request
        //// processing should continue as normal.

        // Arrange
        const int UserId = 2516356;

        Person registerPerson = Person.Minimal("14836498780") with
        {
            PartyId = 987654,
            Uuid = Guid.NewGuid(),
            ShortName = "Sophie Salt",
            FirstName = "Sophie",
            LastName = "Salt",
            ModifiedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
            User = new PartyUser(UserId, "sophie", ImmutableValueArray<uint>.Empty.Add(UserId))
        };

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, registerPerson);

        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"/profile/api/v1/users/{UserId}");
        string token = PrincipalUtil.GetInvalidSystemUserToken(Guid.NewGuid());
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetUsersByUuid_AsUser_RegisterFindsProfile_ResponseOk_ReturnsUserProfile()
    {
        // Arrange
        const int userId = 20000009;
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
            User = new PartyUser(userId, "leo", ImmutableValueArray<uint>.Empty.Add(userId))
        };

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, registerPerson);
        _factory.ProfileSettingsRepositoryMock.Setup(m => m.GetProfileSettings(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProfileSettings
            {
                UserId = userId,
                LanguageType = "nb",
                DoNotPromptForParty = true,
                PreselectedPartyUuid = null,
            });
        _factory.PersonServiceMock.Setup(m => m.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PersonContactPreferences { Email = "test@mail.com", NationalIdentityNumber = "1", MobileNumber = "+4798765432", IsReserved = true }]);

        HttpRequestMessage httpRequestMessage = CreateGetRequest(userId, $"/profile/api/v1/users/byuuid/{userUuid}");

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

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
        Assert.True(actualUser.IsReserved);
        Assert.Equal("test@mail.com", actualUser.Email);
        Assert.Equal("+4798765432", actualUser.PhoneNumber);
    }

    [Fact]
    public async Task GetUsersByUuid_AsUser_RegisterFindsProfileWithoutProfileSettings_ResponseOk_ReturnsWithDefaultValues()
    {
        // Arrange
        const int UserId = 1002356;
        Guid userUuid = new("c0f1e38d-9339-4848-95d0-bef81ee32486");

        Person registerPerson = Person.Minimal("14836498780") with
        {
            PartyId = 987654,
            Uuid = userUuid,
            ShortName = "Sophie Salt",
            FirstName = "Sophie",
            LastName = "Salt",
            ModifiedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
            User = new PartyUser(UserId, "sophie", ImmutableValueArray<uint>.Empty.Add(UserId))
        };

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, registerPerson);

        _factory.ProfileSettingsRepositoryMock.Setup(m => m.GetProfileSettings(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProfileSettings?)null);

        HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, $"/profile/api/v1/users/byuuid/{userUuid}");

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        UserProfile? actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, _serializerOptionsCamelCase);

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
    public async Task GetUsersByUuid_AsUser_RegisterReturnsNotFound_ResponseNotFound()
    {
        // Arrange
        const int userId = 20000009;
        Guid userUuid = new("cc86d2c7-1695-44b0-8e82-e633243fdf31");

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, null, HttpStatusCode.PartialContent);

        HttpRequestMessage httpRequestMessage = CreateGetRequest(userId, $"/profile/api/v1/users/byuuid/{userUuid}");

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
    public async Task GetUsersById_AsUser_RegisterReturnsNotFound_ResponseNotFound()
    {
        // Arrange
        const int UserId = 2222222;

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, null, HttpStatusCode.PartialContent);

        HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, $"/profile/api/v1/users/{UserId}");

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUsersById_AsUser_RegisterReturnsServiceUnavailable_ResponseNotFound()
    {
        // Arrange
        const int UserId = 2222222;

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, null, HttpStatusCode.ServiceUnavailable);

        HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, $"/profile/api/v1/users/{UserId}");

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUsersBySsn_AsUser_RegisterFindsProfile_ReturnsUserProfile()
    {
        // Arrange
        const int UserId = 2516356;
        string ssn = "14836498780";

        Person registerPerson = Person.Minimal(ssn) with
        {
            PartyId = 987654,
            Uuid = Guid.NewGuid(),
            ShortName = "Sophie Salt",
            FirstName = "Sophie",
            LastName = "Salt",
            ModifiedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
            User = new PartyUser(UserId, "sophie", ImmutableValueArray<uint>.Empty.Add(UserId))
        };

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, registerPerson);

        _factory.ProfileSettingsRepositoryMock.Setup(m => m.GetProfileSettings(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProfileSettings
            {
                UserId = UserId,
                LanguageType = "en",
                DoNotPromptForParty = true,
                PreselectedPartyUuid = null,
            });
        _factory.PersonServiceMock.Setup(m => m.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new PersonContactPreferences { Email = "test@mail.com", NationalIdentityNumber = "1", MobileNumber = "+4798765432", IsReserved = true }]);

        StringContent content = new($"\"{ssn}\"", Encoding.UTF8, "application/json");
        HttpRequestMessage httpRequestMessage = CreatePostRequest(2222222, $"/profile/api/v1/users/", content);

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
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
        Assert.True(actualUser.IsReserved);
        Assert.Equal("test@mail.com", actualUser.Email);
        Assert.Equal("+4798765432", actualUser.PhoneNumber);
    }

    [Fact]
    public async Task GetUsersBySsn_AsUser_RegisterFindsProfileWithoutProfileSettings_ResponseOk_ReturnsWithDefaultValues()
    {
        // Arrange
        const int UserId = 1002356;
        var ssn = "14836498780";

        Person registerPerson = Person.Minimal(ssn) with
        {
            PartyId = 987654,
            Uuid = Guid.NewGuid(),
            ShortName = "Sophie Salt",
            FirstName = "Sophie",
            LastName = "Salt",
            ModifiedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
            User = new PartyUser(UserId, "sophie", ImmutableValueArray<uint>.Empty.Add(UserId))
        };

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, registerPerson);
        _factory.ProfileSettingsRepositoryMock.Setup(m => m.GetProfileSettings(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProfileSettings?)null);

        StringContent content = new($"\"{ssn}\"", Encoding.UTF8, "application/json");
        HttpRequestMessage httpRequestMessage = CreatePostRequest(2222222, $"/profile/api/v1/users/", content);

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        UserProfile? actualUser = JsonSerializer.Deserialize<UserProfile>(
            responseContent, _serializerOptionsCamelCase);

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
    public async Task GetUsersBySsn_AsUser_RegisterDoesNotFindUser_RespondsNotFound()
    {
        // Arrange
        var ssn = "01017512345";
        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, null, HttpStatusCode.PartialContent);

        StringContent content = new($"\"{ssn}\"", Encoding.UTF8, "application/json");
        HttpRequestMessage httpRequestMessage = CreatePostRequest(2222222, $"/profile/api/v1/users/", content);

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUsersBySsn_AsUser_RegisterReturnsServiceUnavailable_RespondsNotFound()
    {
        // Arrange
        var ssn = "01017512345";
        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, null, HttpStatusCode.ServiceUnavailable);

        StringContent content = new($"\"{ssn}\"", Encoding.UTF8, "application/json");
        HttpRequestMessage httpRequestMessage = CreatePostRequest(2222222, $"/profile/api/v1/users/", content);

        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUsersById_AsUser_RegisterAsPrimaryEnabled_RegisterHasSsn_UsesRegisterAndSkipsSbl()
    {
        // Arrange
        const int userId = 2516356;

        HttpRequestMessage? sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction((request, token) =>
        {
            sblRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        Person registerPerson = Person.Minimal("14836498780") with
        {
            PartyId = 987654,
            Uuid = Guid.NewGuid(),
            ShortName = "Register Person",
            FirstName = "Register",
            LastName = "Person",
            ModifiedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
        };

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, registerPerson);

        _factory.ProfileSettingsRepositoryMock
            .Setup(m => m.GetProfileSettings(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProfileSettings?)null);

        _factory.PersonServiceMock
            .Setup(m => m.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var client = CreateClientWithRegisterAsPrimary(true);

        HttpRequestMessage httpRequestMessage = CreateGetRequest(userId, $"/profile/api/v1/users/{userId}");
        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(sblRequest); // register path should not call legacy

        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        UserProfile? actualUser = JsonSerializer.Deserialize<UserProfile>(responseContent, _serializerOptionsCamelCase);

        Assert.NotNull(actualUser);
        Assert.Equal("Register Person", actualUser.Party.Name);
        Assert.Equal("14836498780", actualUser.Party.SSN);
    }

    [Fact]
    public async Task GetUsersById_AsUser_RegisterAsPrimaryDisabled_FindsUserAtSbl()
    {
        // Arrange
        const int userId = 2516356;

        HttpRequestMessage? sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;
            UserProfile userProfile = await TestDataLoader.Load<UserProfile>(userId.ToString());
            return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
        });

        _factory.ProfileSettingsRepositoryMock
            .Setup(m => m.GetProfileSettings(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProfileSettings?)null);

        _factory.PersonServiceMock
            .Setup(m => m.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        HttpClient client = CreateClientWithRegisterAsPrimary(false);

        HttpRequestMessage httpRequestMessage = CreateGetRequest(userId, $"/profile/api/v1/users/{userId}");
        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(sblRequest); // fallback path must call legacy
        Assert.EndsWith($"users/{userId}", sblRequest!.RequestUri?.ToString());
    }

    [Fact]
    public async Task GetUsersById_AsUser_RegisterAsPrimaryEnabled_RegisterReturnsSelfIdentified()
    {
        // Arrange
        const int userId = 2516356;

        HttpRequestMessage? sblRequest = null;
        _factory.SblBridgeHttpMessageHandler.ChangeHandlerFunction(async (request, token) =>
        {
            sblRequest = request;
            UserProfile userProfile = await TestDataLoader.Load<UserProfile>(userId.ToString());
            return new HttpResponseMessage() { Content = JsonContent.Create(userProfile) };
        });

        SelfIdentifiedUser selfIdentifiedFromRegister = await TestDataLoader.Load<SelfIdentifiedUser>("siuser-input");

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, selfIdentifiedFromRegister);

        _factory.ProfileSettingsRepositoryMock
            .Setup(m => m.GetProfileSettings(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProfileSettings?)null);

        _factory.PersonServiceMock
            .Setup(m => m.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        HttpClient client = CreateClientWithRegisterAsPrimary(true);

        HttpRequestMessage httpRequestMessage = CreateGetRequest(userId, $"/profile/api/v1/users/{userId}");
        httpRequestMessage.Headers.Add("PlatformAccessToken", PrincipalUtil.GetAccessToken("ttd", "unittest"));

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(sblRequest); // register path should not call legacy

        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        UserProfile? actualUser = JsonSerializer.Deserialize<UserProfile>(responseContent, _serializerOptionsCamelCase);
        Assert.NotNull(actualUser);
        Assert.NotNull(actualUser.Party);
    }

    [Fact]
    public async Task GetUsersCurrent_RegisterFindsSelfIdentifiedProfile_ResponseOk_ReturnsLocallyEnrichedContactInfo()
    {
        // Arrange
        const int UserId = 21226106;
        SelfIdentifiedUser registerPerson = SelfIdentifiedUser.MinimalLegacy("uidp_p9zpzf9ti9fff1RiIF") with { User = new PartyUser(UserId, null, ImmutableValueArray<uint>.Empty.Add((uint)UserId)) } with
        {
            PartyId = 987654,
            Uuid = Guid.NewGuid(),
            ModifiedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
        };

        RegisterHttpMessageHandlerHelpers.SetupRegisterUserPartyLookup(_factory, registerPerson);

        _factory.ProfileSettingsRepositoryMock
            .Setup(m => m.GetProfileSettings(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProfileSettings?)null);

        _factory.UserContactInfoRepositoryMock
            .Setup(m => m.Get(UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserContactInfo
            {
                UserId = UserId,
                UserUuid = new Guid("b07236de-b091-4c94-b522-43016d770d41"),
                Username = "uidp_p9zpzf9ti9fff1RiIF",
                CreatedAt = DateTime.UtcNow,
                EmailAddress = "siuser@example.com",
                PhoneNumber = "+4799999999",
            });

        HttpRequestMessage httpRequestMessage = CreateGetRequest(UserId, "/profile/api/v1/users/current");

        HttpClient client = _factory.CreateClient();

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequestMessage, TestContext.Current.CancellationToken);

        // Assert
        string responseContent = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        UserProfile? actualUser = JsonSerializer.Deserialize<UserProfile>(responseContent, _serializerOptionsCamelCase);

        Assert.NotNull(actualUser);
        Assert.Equal(UserId, actualUser.UserId);
        Assert.Equal(Models.Enums.UserType.SelfIdentified, actualUser.UserType);
        Assert.Equal("siuser@example.com", actualUser.Email);
        Assert.Equal("+4799999999", actualUser.PhoneNumber);
        Assert.False(actualUser.IsReserved);

        _factory.UserContactInfoRepositoryMock.Verify(m => m.Get(UserId, It.IsAny<CancellationToken>()), Times.Once);
        _factory.PersonServiceMock.Verify(m => m.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()), Times.Never);
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

    private HttpClient CreateClientWithRegisterAsPrimary(bool enabled)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["CoreSettings:RegisterAsPrimaryUserProfileSource"] = enabled ? "true" : "false"
                });
            });
        }).CreateClient();
    }
}
