using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.Person.ContactPreferences;
using Altinn.Profile.Core.User;
using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Models;

using Microsoft.Extensions.Options;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Core.User;

public class UserProfileServiceTests
{
    private readonly Mock<IUserProfileClient> _userProfileClientMock = new();
    private readonly Mock<IUserProfileComparer> _userProfileComparerMock = new();
    private readonly Mock<IProfileSettingsRepository> _profileSettingsRepositoryMock = new();
    private readonly Mock<IPersonService> _personServiceMock = new();
    private readonly Mock<IRegisterClient> _registerClientMock = new();
    private readonly Mock<IUserContactInfoRepository> _userContactInfoRepositoryMock = new();
    private readonly Mock<IOptionsMonitor<CoreSettings>> _settingsMock = new();

    public UserProfileServiceTests()
    {
        _settingsMock.Setup(s => s.CurrentValue).Returns(new CoreSettings
        {
            RegisterAsPrimaryUserProfileSource = false,
            RegisterLookupInShadowMode = false,
        });

        _personServiceMock
            .Setup(p => p.GetContactPreferencesAsync(It.IsAny<System.Collections.Generic.IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableList<PersonContactPreferences>.Empty);
    }

    private UserProfileService CreateSut() => new(
        _userProfileClientMock.Object,
        _userProfileComparerMock.Object,
        _profileSettingsRepositoryMock.Object,
        _personServiceMock.Object,
        _registerClientMock.Object,
        _userContactInfoRepositoryMock.Object,
        _settingsMock.Object);

    [Fact]
    public async Task GetUser_ById_LegacyPath_PropagatesCancellationTokenToProfileSettingsRepository()
    {
        // Arrange
        const int UserId = 2001607;
        using var cts = new CancellationTokenSource();
        var expectedToken = cts.Token;

        _userProfileClientMock.Setup(c => c.GetUser(UserId)).ReturnsAsync(new UserProfile { UserId = UserId });

        var sut = CreateSut();

        // Act
        await sut.GetUser(UserId, expectedToken);

        // Assert
        _profileSettingsRepositoryMock.Verify(
            r => r.GetProfileSettings(UserId, expectedToken),
            Times.Once);
    }

    [Fact]
    public async Task GetUser_BySsn_LegacyPath_PropagatesCancellationTokenToProfileSettingsRepository()
    {
        // Arrange
        const int UserId = 2001607;
        const string Ssn = "01025101038";
        using var cts = new CancellationTokenSource();
        var expectedToken = cts.Token;

        _userProfileClientMock.Setup(c => c.GetUser(Ssn)).ReturnsAsync(new UserProfile { UserId = UserId });

        var sut = CreateSut();

        // Act
        await sut.GetUser(Ssn, expectedToken);

        // Assert
        _profileSettingsRepositoryMock.Verify(
            r => r.GetProfileSettings(UserId, expectedToken),
            Times.Once);
    }

    [Fact]
    public async Task GetUserByUsername_LegacyPath_PropagatesCancellationTokenToProfileSettingsRepository()
    {
        // Arrange
        const int UserId = 2001072;
        const string Username = "OrstaECUser";
        using var cts = new CancellationTokenSource();
        var expectedToken = cts.Token;

        _userProfileClientMock.Setup(c => c.GetUserByUsername(Username)).ReturnsAsync(new UserProfile { UserId = UserId });

        var sut = CreateSut();

        // Act
        await sut.GetUserByUsername(Username, expectedToken);

        // Assert
        _profileSettingsRepositoryMock.Verify(
            r => r.GetProfileSettings(UserId, expectedToken),
            Times.Once);
    }

    [Fact]
    public async Task GetUserByUuid_LegacyPath_PropagatesCancellationTokenToProfileSettingsRepository()
    {
        // Arrange
        const int UserId = 2001607;
        var userUuid = new System.Guid("cc86d2c7-1695-44b0-8e82-e633243fdf31");
        using var cts = new CancellationTokenSource();
        var expectedToken = cts.Token;

        _userProfileClientMock.Setup(c => c.GetUserByUuid(userUuid)).ReturnsAsync(new UserProfile { UserId = UserId, UserUuid = userUuid });

        var sut = CreateSut();

        // Act
        await sut.GetUserByUuid(userUuid, expectedToken);

        // Assert
        _profileSettingsRepositoryMock.Verify(
            r => r.GetProfileSettings(UserId, expectedToken),
            Times.Once);
    }

    [Fact]
    public async Task GetUser_ById_ShadowMode_PropagatesCancellationTokenToProfileSettingsRepositoryOnLegacyEnrichment()
    {
        // Arrange
        const int UserId = 2001607;
        using var cts = new CancellationTokenSource();
        var expectedToken = cts.Token;

        _settingsMock.Setup(s => s.CurrentValue).Returns(new CoreSettings
        {
            RegisterAsPrimaryUserProfileSource = false,
            RegisterLookupInShadowMode = true,
        });

        _userProfileClientMock.Setup(c => c.GetUser(UserId)).ReturnsAsync(new UserProfile { UserId = UserId });
        _registerClientMock.Setup(c => c.GetUserParty(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(default(Altinn.Register.Contracts.Party));

        var sut = CreateSut();

        // Act
        await sut.GetUser(UserId, expectedToken);

        // Assert
        _profileSettingsRepositoryMock.Verify(
            r => r.GetProfileSettings(UserId, expectedToken),
            Times.Once);
    }
}
