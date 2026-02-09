using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.Person.ContactPreferences;
using Altinn.Profile.Core.User;
using Altinn.Profile.Core.User.ContactPoints;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.Testdata;

using ImTools;

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
        var identities = new List<string>
        {
            "urn:altinn:person:idporten-email:user1@example.com",
            "urn:altinn:person:idporten-email:user2@test.no",
            "urn:altinn:person:idporten-email:admin@altinn.no"
        };
        var target = new UserContactPointService(_userProfileServiceMock.Object, _personServiceMock.Object);

        // Act
        SelfIdentifiedUserContactPointsList result = await target.GetSiContactPoints(identities, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.ContactPointsList.Count);

        Assert.Contains(result.ContactPointsList, cp =>
            cp.Email == "user1@example.com" &&
            cp.ExternalIdentity == "urn:altinn:person:idporten-email:user1@example.com" &&
            cp.MobileNumber is null);

        Assert.Contains(result.ContactPointsList, cp =>
            cp.Email == "user2@test.no" &&
            cp.ExternalIdentity == "urn:altinn:person:idporten-email:user2@test.no" &&
            cp.MobileNumber is null);

        Assert.Contains(result.ContactPointsList, cp =>
            cp.Email == "admin@altinn.no" &&
            cp.ExternalIdentity == "urn:altinn:person:idporten-email:admin@altinn.no" &&
            cp.MobileNumber is null);
    }

    [Fact]
    public async Task GetSiContactPoints_WithMixedFormats_MissingUrnPrefixWillBeDiscarded()
    {
        // Arrange
        var identities = new List<string>
    {
        "urn:altinn:person:idporten-email:user1@altinn.no",
        "urn:altinn:person:idporten-email:user2@altinn.no",
        "unprefixed@test.no"
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
            cp.ExternalIdentity == "urn:altinn:person:idporten-email:user1@altinn.no" &&
            cp.MobileNumber is null);

        Assert.Contains(result.ContactPointsList, cp =>
            cp.Email == "user2@altinn.no" &&
            cp.ExternalIdentity == "urn:altinn:person:idporten-email:user2@altinn.no" &&
            cp.MobileNumber is null);
    }

    [Fact]
    public async Task GetSiContactPoints_EmptyList_ReturnsEmptyResult()
    {
        // Arrange
        var emailIdentifiers = new List<string>();
        var target = new UserContactPointService(_userProfileServiceMock.Object, _personServiceMock.Object);

        // Act
        SelfIdentifiedUserContactPointsList result = await target.GetSiContactPoints(emailIdentifiers, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.ContactPointsList);
    }

    [Fact]
    public async Task GetSiContactPoints_WithUriEncodedEmailCharacters_ReturnsDecodedEmail()
    {
        // Arrange
        var identities = new List<string>
        {
            // Plus sign (+) encoded as %2B
            "urn:altinn:person:idporten-email:user%2Btag@example.com",
            
            // Percent (%) encoded as %25
            "urn:altinn:person:idporten-email:user%2550off@test.no",
            
            // Space encoded as %20
            "urn:altinn:person:idporten-email:first%20last@company.com",
            
            // Hash (#) encoded as %23
            "urn:altinn:person:idporten-email:user%23123@altinn.no",
            
            // Equals (=) encoded as %3D
            "urn:altinn:person:idporten-email:user%3Dname@domain.com",
            
            // Ampersand (&) encoded as %26
            "urn:altinn:person:idporten-email:user%26company@test.org"
        };
        var target = new UserContactPointService(_userProfileServiceMock.Object, _personServiceMock.Object);

        // Act
        SelfIdentifiedUserContactPointsList result = await target.GetSiContactPoints(identities, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(6, result.ContactPointsList.Count);

        // Verify decoded emails
        Assert.Contains(result.ContactPointsList, cp => 
            cp.Email == "user+tag@example.com" && 
            cp.ExternalIdentity == "urn:altinn:person:idporten-email:user%2Btag@example.com" &&
            cp.MobileNumber is null);

        Assert.Contains(result.ContactPointsList, cp => 
            cp.Email == "user%50off@test.no" && 
            cp.ExternalIdentity == "urn:altinn:person:idporten-email:user%2550off@test.no" &&
            cp.MobileNumber is null);

        Assert.Contains(result.ContactPointsList, cp => 
            cp.Email == "first last@company.com" && 
            cp.ExternalIdentity == "urn:altinn:person:idporten-email:first%20last@company.com" &&
            cp.MobileNumber is null);

        Assert.Contains(result.ContactPointsList, cp => 
            cp.Email == "user#123@altinn.no" && 
            cp.ExternalIdentity == "urn:altinn:person:idporten-email:user%23123@altinn.no" &&
            cp.MobileNumber is null);

        Assert.Contains(result.ContactPointsList, cp => 
            cp.Email == "user=name@domain.com" && 
            cp.ExternalIdentity == "urn:altinn:person:idporten-email:user%3Dname@domain.com" &&
            cp.MobileNumber is null);

        Assert.Contains(result.ContactPointsList, cp => 
            cp.Email == "user&company@test.org" && 
            cp.ExternalIdentity == "urn:altinn:person:idporten-email:user%26company@test.org" &&
            cp.MobileNumber is null);
    }
}
