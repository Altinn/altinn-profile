using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.IntegrationTests.Utils;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.IntegrationTests.API.Controllers;

public class UsersControllerProfileSettingsTests : IClassFixture<ProfileWebApplicationFactory<Program>>
{
    private readonly ProfileWebApplicationFactory<Program> _factory;

    private readonly JsonSerializerOptions _serializerOptionsCamelCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public UsersControllerProfileSettingsTests(ProfileWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _factory.MemoryCache.Clear();

        // Ensure previous setups don't leak between tests
        _factory.ProfileSettingsRepositoryMock.Reset();
    }

    [Fact]
    public async Task PutCurrentProfileSettings_AsUser_UpdatesSettingsAndReturnsSuccess()
    {
        // Arrange
        const int userId = 2516356;

        var request = new ProfileSettingPreference
        {
            Language = "nb",
            DoNotPromptForParty = true,
            PreselectedPartyUuid = Guid.NewGuid(),
            ShowClientUnits = true,
            ShouldShowSubEntities = true,
            ShouldShowDeletedEntities = false
        };

        ProfileSettings captured = null;
        _factory.ProfileSettingsRepositoryMock
            .Setup(r => r.UpdateProfileSettings(It.IsAny<ProfileSettings>()))
            .Callback<ProfileSettings>(ps => captured = ps)
            .ReturnsAsync((ProfileSettings x) => x);

        HttpClient client = _factory.CreateClient();

        HttpRequestMessage httpRequest = new(HttpMethod.Put, "/profile/api/v1/users/current(profilesettings");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", PrincipalUtil.GetToken(userId));
        httpRequest.Content = JsonContent.Create(request, options: _serializerOptionsCamelCase);

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequest);

        // Assert
        Assert.True(response.IsSuccessStatusCode, $"Expected success status code but got {(int)response.StatusCode}");

        // Ensure repository was called with mapped ProfileSettings
        Assert.NotNull(captured);
        Assert.Equal(userId, captured!.UserId);
        Assert.Equal(request.Language, captured.LanguageType);
        Assert.Equal(request.DoNotPromptForParty, captured.DoNotPromptForParty);
        Assert.Equal(request.PreselectedPartyUuid, captured.PreselectedPartyUuid);
        Assert.Equal(request.ShowClientUnits, captured.ShowClientUnits);
        Assert.Equal(request.ShouldShowSubEntities, captured.ShouldShowSubEntities);
        Assert.Equal(request.ShouldShowDeletedEntities, captured.ShouldShowDeletedEntities);

        _factory.ProfileSettingsRepositoryMock.Verify(r => r.UpdateProfileSettings(It.IsAny<ProfileSettings>()), Times.Once);
    }

    [Fact]
    public async Task PutCurrentProfileSettings_MissingUserId_ReturnsBadRequest_RepositoryNotCalled()
    {
        // Arrange
        var request = new ProfileSettingPreference
        {
            Language = "nb",
            DoNotPromptForParty = true
        };

        _factory.ProfileSettingsRepositoryMock
            .Setup(r => r.UpdateProfileSettings(It.IsAny<ProfileSettings>()))
            .ReturnsAsync((ProfileSettings x) => x);

        HttpClient client = _factory.CreateClient();

        HttpRequestMessage httpRequest = new(HttpMethod.Put, "/profile/api/v1/users/current(profilesettings");

        // Use an org token so userId claim is not present for a user -> BadRequest (consistent with other tests)
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", PrincipalUtil.GetOrgToken("ttd"));
        httpRequest.Content = JsonContent.Create(request, options: _serializerOptionsCamelCase);

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _factory.ProfileSettingsRepositoryMock.Verify(r => r.UpdateProfileSettings(It.IsAny<ProfileSettings>()), Times.Never);
    }

    [Fact]
    public async Task PutCurrentProfileSettings_AsUser_RepositoryCalled_ReturnsSuccess()
    {
        // Arrange
        const int userId = 2516356;

        var request = new ProfileSettingPreference
        {
            Language = "nb",
            DoNotPromptForParty = false
        };

        _factory.ProfileSettingsRepositoryMock
            .Setup(r => r.UpdateProfileSettings(It.IsAny<ProfileSettings>()))
            .ReturnsAsync((ProfileSettings x) => x);

        HttpClient client = _factory.CreateClient();

        HttpRequestMessage httpRequest = new(HttpMethod.Put, "/profile/api/v1/users/current(profilesettings");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", PrincipalUtil.GetToken(userId));
        httpRequest.Content = JsonContent.Create(request, options: _serializerOptionsCamelCase);

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequest);

        // Assert
        Assert.True(response.IsSuccessStatusCode, $"Expected success status code but got {(int)response.StatusCode}");
        _factory.ProfileSettingsRepositoryMock.Verify(r => r.UpdateProfileSettings(It.IsAny<ProfileSettings>()), Times.Once);
    }
}
