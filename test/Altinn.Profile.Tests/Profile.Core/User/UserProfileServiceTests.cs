using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Authorization.ModelUtils;
using Altinn.Profile.Core;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.Person.ContactPreferences;
using Altinn.Profile.Core.User;
using Altinn.Profile.Models;
using Altinn.Register.Contracts;
using Altinn.Register.Contracts.Testing;

using Microsoft.Extensions.Options;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Core.User;

public class UserProfileServiceTests
{
    private readonly Mock<IProfileSettingsRepository> _profileSettingsRepositoryMock = new();
    private readonly Mock<IPersonService> _personServiceMock = new();
    private readonly Mock<IRegisterClient> _registerClientMock = new();
    private readonly Mock<IUserContactInfoRepository> _userContactInfoRepositoryMock = new();
    private readonly Mock<IOptionsMonitor<CoreSettings>> _settingsMock = new();

    public UserProfileServiceTests()
    {
        _personServiceMock
            .Setup(p => p.GetContactPreferencesAsync(It.IsAny<System.Collections.Generic.IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ImmutableList<PersonContactPreferences>.Empty);
    }

    private UserProfileService CreateSut() => new(
        _profileSettingsRepositoryMock.Object,
        _personServiceMock.Object,
        _registerClientMock.Object,
        _userContactInfoRepositoryMock.Object,
        _settingsMock.Object);

    [Fact]
    public async Task GetUser_ById_PropagatesCancellationTokenToProfileSettingsRepository()
    {
        // Arrange
        const int UserId = 2001607;
        using var cts = new CancellationTokenSource();
        var expectedToken = cts.Token;
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
        _registerClientMock.Setup(c => c.GetUserParty(UserId, It.IsAny<CancellationToken>())).ReturnsAsync(registerPerson);

        var sut = CreateSut();

        // Act
        await sut.GetUser(UserId, expectedToken);

        // Assert
        _profileSettingsRepositoryMock.Verify(
            r => r.GetProfileSettings(UserId, expectedToken),
            Times.Once);
    }

    [Fact]
    public async Task GetUser_BySsn_PropagatesCancellationTokenToProfileSettingsRepository()
    {
        // Arrange
        const int UserId = 2001607;
        const string Ssn = "14836498780";
        using var cts = new CancellationTokenSource();
        var expectedToken = cts.Token;

        Person registerPerson = Person.Minimal(Ssn) with
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
        _registerClientMock.Setup(c => c.GetUserPartyBySsn(Ssn, It.IsAny<CancellationToken>())).ReturnsAsync(registerPerson);

        var sut = CreateSut();

        // Act
        await sut.GetUser(Ssn, expectedToken);

        // Assert
        _profileSettingsRepositoryMock.Verify(
            r => r.GetProfileSettings(UserId, expectedToken),
            Times.Once);
    }

    [Fact]
    public async Task GetUserByUsername_PropagatesCancellationTokenToProfileSettingsRepository()
    {
        // Arrange
        const int UserId = 2001072;
        const string Username = "OrstaECUser";
        using var cts = new CancellationTokenSource();
        var expectedToken = cts.Token;

        Person registerPerson = Person.Minimal("14836498780") with
        {
            PartyId = 987654,
            Uuid = Guid.NewGuid(),
            ShortName = "Register Person",
            FirstName = "Register",
            LastName = "Person",
            ModifiedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
            User = new PartyUser(UserId, Username, ImmutableValueArray<uint>.Empty.Add(UserId))
        };
        _registerClientMock.Setup(c => c.GetUserPartyByUsername(Username, It.IsAny<CancellationToken>())).ReturnsAsync(registerPerson);

        var sut = CreateSut();

        // Act
        await sut.GetUserByUsername(Username, expectedToken);

        // Assert
        _profileSettingsRepositoryMock.Verify(
            r => r.GetProfileSettings(UserId, expectedToken),
            Times.Once);
    }

    [Fact]
    public async Task GetUserByUuid_PropagatesCancellationTokenToProfileSettingsRepository()
    {
        // Arrange
        const int UserId = 2001607;
        var userUuid = new Guid("cc86d2c7-1695-44b0-8e82-e633243fdf31");
        using var cts = new CancellationTokenSource();
        var expectedToken = cts.Token;
        Person registerPerson = Person.Minimal("14836498780") with
        {
            PartyId = 987654,
            Uuid = userUuid,
            ShortName = "Register Person",
            FirstName = "Register",
            LastName = "Person",
            ModifiedAt = DateTimeOffset.UtcNow,
            IsDeleted = false,
            User = new PartyUser(UserId, "register.person", ImmutableValueArray<uint>.Empty.Add(UserId))
        };

        _registerClientMock.Setup(c => c.GetUserParty(userUuid, It.IsAny<CancellationToken>())).ReturnsAsync(registerPerson);

        var sut = CreateSut();

        // Act
        await sut.GetUserByUuid(userUuid, expectedToken);

        // Assert
        _profileSettingsRepositoryMock.Verify(
            r => r.GetProfileSettings(UserId, expectedToken),
            Times.Once);
    }
}
