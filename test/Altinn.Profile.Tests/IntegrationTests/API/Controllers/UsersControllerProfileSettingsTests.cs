using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
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
            .Setup(r => r.UpdateProfileSettings(It.IsAny<ProfileSettings>(), It.IsAny<CancellationToken>()))
            .Callback<ProfileSettings, CancellationToken>((ProfileSettings x, CancellationToken _) => captured = x)
            .ReturnsAsync((ProfileSettings x, CancellationToken _) => x);

        HttpClient client = _factory.CreateClient();

        HttpRequestMessage httpRequest = new(HttpMethod.Put, "/profile/api/v1/users/current/profilesettings");
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

        _factory.ProfileSettingsRepositoryMock.Verify(r => r.UpdateProfileSettings(It.IsAny<ProfileSettings>(), It.IsAny<CancellationToken>()), Times.Once);
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
            .Setup(r => r.UpdateProfileSettings(It.IsAny<ProfileSettings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProfileSettings x, CancellationToken _) => x);

        HttpClient client = _factory.CreateClient();

        HttpRequestMessage httpRequest = new(HttpMethod.Put, "/profile/api/v1/users/current/profilesettings");

        // Use an org token so userId claim is not present for a user -> BadRequest (consistent with other tests)
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", PrincipalUtil.GetOrgToken("ttd"));
        httpRequest.Content = JsonContent.Create(request, options: _serializerOptionsCamelCase);

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _factory.ProfileSettingsRepositoryMock.Verify(r => r.UpdateProfileSettings(It.IsAny<ProfileSettings>(), It.IsAny<CancellationToken>()), Times.Never);
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
            .Setup(r => r.UpdateProfileSettings(It.IsAny<ProfileSettings>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProfileSettings x, CancellationToken _) => x);

        HttpClient client = _factory.CreateClient();

        HttpRequestMessage httpRequest = new(HttpMethod.Put, "/profile/api/v1/users/current/profilesettings");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", PrincipalUtil.GetToken(userId));
        httpRequest.Content = JsonContent.Create(request, options: _serializerOptionsCamelCase);

        // Act
        HttpResponseMessage response = await client.SendAsync(httpRequest);

        // Assert
        Assert.True(response.IsSuccessStatusCode, $"Expected success status code but got {(int)response.StatusCode}");
        _factory.ProfileSettingsRepositoryMock.Verify(r => r.UpdateProfileSettings(It.IsAny<ProfileSettings>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PatchUsersCurrent_AsUser_PatchSucceeds_ReturnsUpdatedPreferences()
    {
        // Arrange
        const int userId = 400000;
        var returnedPreselected = Guid.NewGuid();

        // The repository backing the service should return the patched settings
        _factory.ProfileSettingsRepositoryMock.Setup(m => m.PatchProfileSettings(It.IsAny<ProfileSettingsPatchModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProfileSettings
            {
                UserId = userId,
                LanguageType = "nb",
                DoNotPromptForParty = true,
                PreselectedPartyUuid = returnedPreselected,
                ShowClientUnits = true,
                ShouldShowSubEntities = true,
                ShouldShowDeletedEntities = false
            });

        HttpClient client = _factory.CreateClient();

        // Build patch request payload (camelCase)
        string payload = JsonSerializer.Serialize(
            new
            {
                language = "nb",
                doNotPromptForParty = true,
                preselectedPartyUuid = returnedPreselected,
                showClientUnits = true,
                shouldShowSubEntities = true,
                shouldShowDeletedEntities = false
            },
            _serializerOptionsCamelCase);

        var request = new HttpRequestMessage(HttpMethod.Patch, "/profile/api/v1/users/current/profilesettings")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", PrincipalUtil.GetToken(userId));

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string responseContent = await response.Content.ReadAsStringAsync();
        var actual = JsonSerializer.Deserialize<ProfileSettingPreference>(responseContent, _serializerOptionsCamelCase);

        Assert.NotNull(actual);
        Assert.Equal("nb", actual.Language);
        Assert.True(actual.DoNotPromptForParty);
        Assert.Equal(returnedPreselected, actual.PreselectedPartyUuid);
        Assert.True(actual.ShowClientUnits);
        Assert.True(actual.ShouldShowSubEntities);
        Assert.False(actual.ShouldShowDeletedEntities);
    }

    [Fact]
    public async Task PatchUsersCurrent_AsUser_PatchReturnsNull_ReturnsNotFound()
    {
        // Arrange
        const int userId = 410000;

        _factory.ProfileSettingsRepositoryMock.Setup(m => m.PatchProfileSettings(It.IsAny<ProfileSettingsPatchModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProfileSettings)null);

        HttpClient client = _factory.CreateClient();

        string payload = JsonSerializer.Serialize(
            new
            {
                language = "nb"
            }, 
            _serializerOptionsCamelCase);

        var request = new HttpRequestMessage(HttpMethod.Patch, "/profile/api/v1/users/current/profilesettings")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", PrincipalUtil.GetToken(userId));

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PatchUsersCurrent_AsOrg_MissingUserIdClaim_ReturnsBadRequest()
    {
        // Arrange
        HttpClient client = _factory.CreateClient();

        string payload = JsonSerializer.Serialize(
            new
            {
                language = "nb"
            }, 
            _serializerOptionsCamelCase);

        var request = new HttpRequestMessage(HttpMethod.Patch, "/profile/api/v1/users/current/profilesettings")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        // Use an org token which does not contain a userId claim
        string token = PrincipalUtil.GetOrgToken("ttd");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        string responseContent = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("UserId must be provided in claims", responseContent);
    }
}
