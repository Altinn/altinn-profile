using System;
using System.Collections.Generic;
using System.Linq;

using Altinn.Profile.Core.User;
using Altinn.Profile.Models;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

using static Altinn.Profile.Core.User.UserProfileComparer;

namespace Altinn.Profile.Tests.Profile.Core.User;

public class UserProfileComparerTests
{
    [Fact]
    public void CompareAndLog_EqualProfiles_ReturnsNoMismatches()
    {
        Mock<ILogger<UserProfileComparer>> loggerMock = new();
        UserProfileComparer target = new(loggerMock.Object);

        UserProfile source = CreateUserProfile();
        UserProfile compared = CreateUserProfile();

        IReadOnlyList<UserProfileMismatch> mismatches = target.CompareAndLog(source, compared);

        Assert.Empty(mismatches);
        VerifyWarningCount(loggerMock, Times.Never());
    }

    [Fact]
    public void CompareAndLog_NullVsEmptyString_LogsMismatchWithoutPersonalValues()
    {
        Mock<ILogger<UserProfileComparer>> loggerMock = new();
        UserProfileComparer target = new(loggerMock.Object);

        UserProfile source = CreateUserProfile();
        UserProfile compared = CreateUserProfile();
        source.UserName = null;
        compared.UserName = string.Empty;

        IReadOnlyList<UserProfileMismatch> mismatches = target.CompareAndLog(source, compared);

        UserProfileMismatch mismatch = Assert.Single(mismatches);
        Assert.Equal("UserName", mismatch.FieldPath);
        Assert.Equal(UserProfileMismatchType.NullVsEmptyString, mismatch.MismatchType);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("UserName", StringComparison.Ordinal)
                    && v.ToString()!.Contains(UserProfileMismatchType.NullVsEmptyString.ToString(), StringComparison.Ordinal)
                    && v.ToString()!.Contains(source.UserType.ToString(), StringComparison.Ordinal)
                    && !v.ToString()!.Contains("source-user", StringComparison.Ordinal)
                    && !v.ToString()!.Contains("target-user", StringComparison.Ordinal)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public void CompareAndLog_StringWithExtraSpaces_ReportsExtraSpaces()
    {
        Mock<ILogger<UserProfileComparer>> loggerMock = new();
        UserProfileComparer target = new(loggerMock.Object);

        UserProfile source = CreateUserProfile();
        UserProfile compared = CreateUserProfile();
        compared.UserName = $" {source.UserName} ";

        IReadOnlyList<UserProfileMismatch> mismatches = target.CompareAndLog(source, compared);

        UserProfileMismatch mismatch = Assert.Single(mismatches);
        Assert.Equal("UserName", mismatch.FieldPath);
        Assert.Equal(UserProfileMismatchType.ExtraSpaces, mismatch.MismatchType);
        VerifyWarningCount(loggerMock, Times.Once());
    }

    [Fact]
    public void CompareAndLog_StringWithExtraSpacesInside_ReportsExtraSpaces()
    {
        Mock<ILogger<UserProfileComparer>> loggerMock = new();
        UserProfileComparer target = new(loggerMock.Object);

        UserProfile source = CreateUserProfile();
        UserProfile compared = CreateUserProfile();
        source.UserName = "Firstname Lastname";
        compared.UserName = "Firstname  Lastname";

        IReadOnlyList<UserProfileMismatch> mismatches = target.CompareAndLog(source, compared);

        UserProfileMismatch mismatch = Assert.Single(mismatches);
        Assert.Equal("UserName", mismatch.FieldPath);
        Assert.Equal(UserProfileMismatchType.ExtraSpaces, mismatch.MismatchType);
        VerifyWarningCount(loggerMock, Times.Once());
    }

    [Fact]
    public void CompareAndLog_MissingNestedObject_ReportsMissingField()
    {
        Mock<ILogger<UserProfileComparer>> loggerMock = new();
        UserProfileComparer target = new(loggerMock.Object);

        UserProfile source = CreateUserProfile();
        UserProfile compared = CreateUserProfile();
        source.Party.Person = null;

        IReadOnlyList<UserProfileMismatch> mismatches = target.CompareAndLog(source, compared);

        UserProfileMismatch mismatch = Assert.Single(mismatches);
        Assert.Equal("Party.Person", mismatch.FieldPath);
        Assert.Equal(UserProfileMismatchType.MissingField, mismatch.MismatchType);
        VerifyWarningCount(loggerMock, Times.Once());
    }

    [Fact]
    public void CompareAndLog_DifferentValue_ReportsWrongValue()
    {
        Mock<ILogger<UserProfileComparer>> loggerMock = new();
        UserProfileComparer target = new(loggerMock.Object);

        UserProfile source = CreateUserProfile();
        UserProfile compared = CreateUserProfile();
        compared.UserType = Models.Enums.UserType.SelfIdentified;

        IReadOnlyList<UserProfileMismatch> mismatches = target.CompareAndLog(source, compared);

        UserProfileMismatch mismatch = Assert.Single(mismatches);
        Assert.Equal("UserType", mismatch.FieldPath);
        Assert.Equal(UserProfileMismatchType.WrongValue, mismatch.MismatchType);
        VerifyWarningCount(loggerMock, Times.Once());
    }

    private static UserProfile CreateUserProfile()
    {
        return new UserProfile
        {
            UserId = 123,
            UserUuid = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            UserName = "source-user",
            ExternalIdentity = "external-id",
            IsReserved = false,
            PhoneNumber = "11111111",
            Email = "masked@example.org",
            PartyId = 456,
            UserType = Models.Enums.UserType.SSNIdentified,
            ProfileSettingPreference = new ProfileSettingPreference
            {
                Language = "nb",
                PreSelectedPartyId = 0,
                DoNotPromptForParty = false,
                PreselectedPartyUuid = null,
                ShowClientUnits = false,
                ShouldShowSubEntities = false,
                ShouldShowDeletedEntities = false,
            },
            Party = new Register.Contracts.V1.Party
            {
                PartyId = 456,
                PartyUuid = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                PartyTypeName = Register.Contracts.V1.PartyType.Person,
                SSN = string.Empty,
                OrgNumber = string.Empty,
                Name = "masked-name",
                IsDeleted = false,
                LastChangedInAltinn = DateTimeOffset.Parse("2025-01-01T00:00:00+00:00"),
                Person = new Register.Contracts.V1.Person
                {
                    SSN = string.Empty,
                    Name = "masked-name",
                    FirstName = "first",
                    MiddleName = string.Empty,
                    LastName = "last",
                    TelephoneNumber = string.Empty,
                    MobileNumber = string.Empty,
                    MailingAddress = string.Empty,
                    MailingPostalCode = string.Empty,
                    MailingPostalCity = string.Empty,
                    AddressMunicipalNumber = string.Empty,
                    AddressMunicipalName = string.Empty,
                    AddressStreetName = string.Empty,
                    AddressHouseNumber = string.Empty,
                    AddressHouseLetter = string.Empty,
                    AddressPostalCode = string.Empty,
                    AddressCity = string.Empty,
                    DateOfDeath = null,
                },
            },
        };
    }

    private static void VerifyWarningCount(Mock<ILogger<UserProfileComparer>> loggerMock, Times expectedTimes)
    {
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            expectedTimes);
    }
}
