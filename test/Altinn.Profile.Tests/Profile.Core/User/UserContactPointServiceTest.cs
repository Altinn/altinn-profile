using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.Person.ContactPreferences;
using Altinn.Profile.Core.User;
using Altinn.Profile.Core.User.ContactPoints;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.Testdata;

using Microsoft.Extensions.Options;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Core.User;

public class UserContactPointServiceTest
{
    private readonly Mock<IUserProfileService> _userProfileServiceMock = new();
    private readonly Mock<IPersonService> _personServiceMock = new();

    private static readonly string _userIdAStr = "2001606";

    private static readonly string _userIdBStr = "2001607";

    private async Task<List<UserContactPoints>> MockTestUsers() // Take a look at IAsyncLifetime / InitializeAsync from XUnit, as something for next time
    {
        var userProfileA = await TestDataLoader.Load<UserProfile>(_userIdAStr);
        var contactPreferencesA = new PersonContactPreferences()
        {
            NationalIdentityNumber = userProfileA.Party.SSN,
            Email = userProfileA.Email,
            IsReserved = userProfileA.IsReserved,
            MobileNumber = userProfileA.PhoneNumber
        };
        var expectedUserContactPointA = new UserContactPoints()
        {
            Email = userProfileA.Email,
            NationalIdentityNumber = userProfileA.Party.SSN,
            IsReserved = userProfileA.IsReserved,
            MobileNumber = userProfileA.PhoneNumber,
        };

        var userProfileB = await TestDataLoader.Load<UserProfile>(_userIdBStr);
        var contactPreferencesB = new PersonContactPreferences()
        {
            NationalIdentityNumber = userProfileB.Party.SSN,
            Email = userProfileB.Email,
            IsReserved = userProfileB.IsReserved,
            MobileNumber = userProfileB.PhoneNumber
        };
        var expectedUserContactPointB = new UserContactPoints()
        {
            Email = userProfileB.Email,
            NationalIdentityNumber = userProfileB.Party.SSN,
            IsReserved = userProfileB.IsReserved,
            MobileNumber = userProfileB.PhoneNumber,
        };
        _userProfileServiceMock.Setup(m => m.GetUser(userProfileA.Party.SSN)).ReturnsAsync(userProfileA);
        _userProfileServiceMock.Setup(m => m.GetUser(userProfileB.Party.SSN)).ReturnsAsync(userProfileB);
        _personServiceMock.Setup(m => m.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>())).ReturnsAsync([contactPreferencesA, contactPreferencesB]);

        return [expectedUserContactPointA, expectedUserContactPointB];
    }

    [Fact]
    public async Task GetContactPoints_WhenPersonServiceIsCalled_IsSuccess()
    {
        // Arrange
        List<UserContactPoints> expectedUsers = await MockTestUsers();
        var target = new UserContactPointService(_userProfileServiceMock.Object, _personServiceMock.Object);

        // Act
        Result<UserContactPointsList, bool> result = await target.GetContactPoints(
            [
                expectedUsers[0].NationalIdentityNumber,
                expectedUsers[1].NationalIdentityNumber
            ],
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess, "Expected a success result");

        result.Match(
            actual =>
            {
                Assert.Equal(2, actual.ContactPointsList.Count);
                Assert.Contains(actual.ContactPointsList, ob => AreEqualUserContactPoints(ob, expectedUsers[0]));
                Assert.Contains(actual.ContactPointsList, ob => AreEqualUserContactPoints(ob, expectedUsers[1]));
            },
            _ => { });

        _personServiceMock.Verify(service => service.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
    }

    [Fact]
    public async Task GetContactPoints_WhenNoUsers_ReturnsZeroForUserIds()
    {
        // Arrange
        List<UserContactPoints> expectedUsers = await MockTestUsers();
        var target = new UserContactPointService(_userProfileServiceMock.Object, _personServiceMock.Object);

        // Act
        Result<UserContactPointsList, bool> result = await target.GetContactPoints(
            [
                expectedUsers[0].NationalIdentityNumber,
                expectedUsers[1].NationalIdentityNumber
            ],
            CancellationToken.None);

        // Assert
        result.Match(
            actual =>
            {
                Assert.DoesNotContain(actual.ContactPointsList, contactPoint => contactPoint.UserId != 0);
            },
            _ => { });
    }

    private static bool AreEqualUserContactPoints(UserContactPoints a, UserContactPoints b)
    {
        return a.NationalIdentityNumber == b.NationalIdentityNumber &&
            a.Email == b.Email &&
            a.IsReserved == b.IsReserved &&
            a.MobileNumber == b.MobileNumber;
    }

    [Fact]
    public async Task GetSiContactPoints_WithUrnMailtoPrefix_ReturnsStrippedEmailAndBlankMobileNumber()
    {
        // Arrange
        var identities = new List<Uri>
        {
            new("urn:altinn:person:idporten-email::user1@example.com"),
            new("urn:altinn:person:idporten-email::user2@test.no"),
            new("urn:altinn:person:idporten-email::admin@altinn.no")
        };
        var target = new UserContactPointService(_userProfileServiceMock.Object, _personServiceMock.Object);

        // Act
        SelfIdentifiedUserContactPointsList result = await target.GetSiContactPoints(identities, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.ContactPointsList.Count);

        Assert.Contains(result.ContactPointsList, cp =>
            cp.Email == "user1@example.com" &&
            cp.MobileNumber == string.Empty);

        Assert.Contains(result.ContactPointsList, cp =>
            cp.Email == "user2@test.no" &&
            cp.MobileNumber == string.Empty);

        Assert.Contains(result.ContactPointsList, cp =>
            cp.Email == "admin@altinn.no" &&
            cp.MobileNumber == string.Empty);
    }

    [Fact]
    public async Task GetSiContactPoints_WithMixedFormats_MissingUrnPrefixWillBeDiscarded()
    {
        // Arrange
        var identities = new List<Uri>
    {
        new("urn:altinn:person:idporten-email::user1@altinn.no"),
        new("urn:altinn:person:idporten-email::user2@altinn.no"),
        new("unprefixed@test.no")
    };
        var target = new UserContactPointService(_userProfileServiceMock.Object, _personServiceMock.Object);

        // Act
        SelfIdentifiedUserContactPointsList result = await target.GetSiContactPoints(identities, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.ContactPointsList.Count);

        // Verify prefixed email is stripped
        Assert.Contains(result.ContactPointsList, cp =>
            cp.Email == "user1@altinn.no" &&
            cp.MobileNumber == string.Empty);
    }

    [Fact]
    public async Task GetSiContactPoints_EmptyList_ReturnsEmptyResult()
    {
        // Arrange
        var emailIdentifiers = new List<Uri>();
        var target = new UserContactPointService(_userProfileServiceMock.Object, _personServiceMock.Object);

        // Act
        SelfIdentifiedUserContactPointsList result = await target.GetSiContactPoints(emailIdentifiers, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.ContactPointsList);
    }
}
