using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using Altinn.Platform.Profile.Models;
using Altinn.Profile.Core;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.Person.ContactPreferences;
using Altinn.Profile.Core.User;
using Altinn.Profile.Core.User.ContactPoints;
using Altinn.Profile.Tests.Testdata;
using Microsoft.Extensions.Options;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.Core.User;

public class UserContactPointServiceTest 
{
    private readonly Mock<IOptions<CoreSettings>> _coreSettingsOptions;
    private readonly Mock<IUserProfileService> _userProfileServiceMock = new();
    private readonly Mock<IPersonService> _personServiceMock = new();
    
    private const int _userIdA = 2001606;
    private const int _userIdB = 2001607;
    
    private string UserIdAStr => _userIdA.ToString();

    private string UserIdBStr => _userIdB.ToString();

    public UserContactPointServiceTest()
    {
        _coreSettingsOptions = new Mock<IOptions<CoreSettings>>();
    }

    private async Task<List<UserContactPoints>> Setup(bool mockedFeatureFlag)
    {
        _coreSettingsOptions.Setup(s => s.Value).Returns(new CoreSettings { EnableLocalKrrFetch = mockedFeatureFlag });

        var userProfileA = await TestDataLoader.Load<UserProfile>(UserIdAStr);
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

        var userProfileB = await TestDataLoader.Load<UserProfile>(UserIdBStr);
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

        _userProfileServiceMock.Setup(m => m.GetUser(UserIdAStr)).ReturnsAsync(userProfileA);
        _userProfileServiceMock.Setup(m => m.GetUser(UserIdBStr)).ReturnsAsync(userProfileB);

        _personServiceMock.Setup(m => m.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>())).ReturnsAsync([]);

        return [expectedUserContactPointA, expectedUserContactPointB];
    }

    /// <summary>
    /// Tests that the userprofile available in the cache is returned to the caller without forwarding request to decorated service.
    /// </summary>
    [Fact]
    public async Task GetContactPoints_FeatureFlagDisabled_PersonServiceNotCalled()
    {
        // Arrange
        List<UserContactPoints> expectedUsers = await Setup(mockedFeatureFlag: false);
        var target = new UserContactPointService(_userProfileServiceMock.Object, _personServiceMock.Object, _coreSettingsOptions.Object);

        // Act
        Result<UserContactPointsList, bool> result = await target.GetContactPoints(
            [
                expectedUsers[0].NationalIdentityNumber,
                expectedUsers[1].NationalIdentityNumber
            ]);

        // Assert
        _personServiceMock.Verify(service => service.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>()), Times.Never());
    }

    [Fact]
    public async Task GetContactPoints_FeatureFlagDisabled_ProfileServiceIsCalled()
    {
        // Arrange
        List<UserContactPoints> expectedUsers = await Setup(mockedFeatureFlag: false);
        var target = new UserContactPointService(_userProfileServiceMock.Object, _personServiceMock.Object, _coreSettingsOptions.Object);

        // Act
        Result<UserContactPointsList, bool> result = await target.GetContactPoints(
            [
                expectedUsers[0].NationalIdentityNumber,
                expectedUsers[1].NationalIdentityNumber
            ]);

        // Assert
        _userProfileServiceMock.Verify(service => service.GetUser(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetContactPoints_FeatureFlagEnabled_PersonServiceIsCalled()
    {
        // Arrange
        List<UserContactPoints> expectedUsers = await Setup(mockedFeatureFlag: true);
        var target = new UserContactPointService(_userProfileServiceMock.Object, _personServiceMock.Object, _coreSettingsOptions.Object);

        // Act
        Result<UserContactPointsList, bool> result = await target.GetContactPoints(
            [
                expectedUsers[0].NationalIdentityNumber,
                expectedUsers[1].NationalIdentityNumber
            ]);

        // Assert
        _personServiceMock.Verify(service => service.GetContactPreferencesAsync(It.IsAny<IEnumerable<string>>()), Times.Exactly(1));
    }

    [Fact]
    public async Task GetContactPoints_FeatureFlagEnabled_ProfileServiceNotCalled()
    {
        // Arrange
        List<UserContactPoints> expectedUsers = await Setup(mockedFeatureFlag: true);
        var target = new UserContactPointService(_userProfileServiceMock.Object, _personServiceMock.Object, _coreSettingsOptions.Object);

        // Act
        Result<UserContactPointsList, bool> result = await target.GetContactPoints(
            [
                expectedUsers[0].NationalIdentityNumber,
                expectedUsers[1].NationalIdentityNumber
            ]);

        // Assert
        _userProfileServiceMock.Verify(service => service.GetUser(It.IsAny<string>()), Times.Never());
    }

    [Fact]
    public async Task GetContactPoints_FeatureFlagEnabled_ReturnsExpectedUserContactPoints()
    {
        // Arrange
        List<UserContactPoints> expectedUsers = await Setup(mockedFeatureFlag: true);
        var target = new UserContactPointService(_userProfileServiceMock.Object, _personServiceMock.Object, _coreSettingsOptions.Object);

        // Act
        Result<UserContactPointsList, bool> result = await target.GetContactPoints(
            [
                expectedUsers[0].NationalIdentityNumber,
                expectedUsers[1].NationalIdentityNumber
            ]);

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
    }

    [Fact]
    public async Task GetContactPoints_FeatureFlagDisabled_ReturnsExpectedUserContactPoints()
    {
        // Arrange
        List<UserContactPoints> expectedUsers = await Setup(mockedFeatureFlag: false);
        var target = new UserContactPointService(_userProfileServiceMock.Object, _personServiceMock.Object, _coreSettingsOptions.Object);

        // Act
        Result<UserContactPointsList, bool> result = await target.GetContactPoints(
            [
                expectedUsers[0].NationalIdentityNumber,
                expectedUsers[1].NationalIdentityNumber
            ]);

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
    }

    private static bool AreEqualUserContactPoints(UserContactPoints a, UserContactPoints b)
    {
        return a.NationalIdentityNumber == b.NationalIdentityNumber &&
            a.Email == b.Email &&
            a.IsReserved == b.IsReserved &&
            a.MobileNumber == b.MobileNumber;
    }
}
