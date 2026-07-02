using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.ProfessionalNotificationAddresses;
using Altinn.Profile.Core.Unit.ContactPoints;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Core.Unit;

public sealed class UnitContactPointServiceTest
{
    private const string _resourcePrefix = "urn:altinn:resource:";

    [Fact]
    public async Task GetUserRegisteredContactPoints_ReturnsCorrectlyFilteredUserContactPoints()
    {
        // Arrange
        const string resourceId = "resourceId";
        const string organizationNumber = "867567862";

        List<Party> partyList = new()
        {
            new Party
            {
                PartyUuid = Guid.NewGuid(),
                OrganizationIdentifier = organizationNumber
            }
        };

        List<UserPartyContactInfo> userPartyContactInfo = new()
        {
            new UserPartyContactInfo
            {
                UserId = 1,
                PartyUuid = partyList[0].PartyUuid,
                EmailAddress = "navn@navnesen.no",
                UserPartyContactInfoResources = new List<UserPartyContactInfoResource>
                {
                    new UserPartyContactInfoResource
                    {
                        ResourceId = resourceId // Stored resource ids are without the urn:altinn:resource prefix
                    },
                    new UserPartyContactInfoResource
                    {
                        ResourceId = "another-resource-id"
                    }
                }
            },
            new UserPartyContactInfo
            {
                UserId = 2,
                PartyUuid = partyList[0].PartyUuid,
                EmailAddress = "not-navn@navnesen.no",
                UserPartyContactInfoResources = new List<UserPartyContactInfoResource>
                {
                    new UserPartyContactInfoResource
                    {
                        ResourceId = "another-resource-id"
                    }
                }
            }
        };

        Mock<IRegisterClient> registerClient = new();
        registerClient
            .Setup(x => x.GetPartyUuids(
                It.Is<string[]>(ids => ids.Length == 1 && ids[0] == organizationNumber),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(partyList);

        Mock<IProfessionalNotificationsRepository> professionalNotificationsRepository = new();
        professionalNotificationsRepository
            .Setup(x => x.GetAllNotificationAddressesForPartyAsync(
                It.Is<Guid>(id => id == partyList[0].PartyUuid),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPartyContactInfo);

        UnitContactPointService target = new(professionalNotificationsRepository.Object, registerClient.Object);

        // Act
        UnitContactPointsList actualUnitContactPointsList = await target.GetUserRegisteredContactPoints(
            [organizationNumber], _resourcePrefix + resourceId, TestContext.Current.CancellationToken);

        // Assert
        registerClient.VerifyAll();
        professionalNotificationsRepository.VerifyAll();

        Assert.Single(actualUnitContactPointsList.ContactPointsList);
        Assert.Equal("navn@navnesen.no", actualUnitContactPointsList.ContactPointsList[0].UserContactPoints[0].Email);
    }
}
