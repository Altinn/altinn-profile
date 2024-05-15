using System;
using System.Collections.Generic;

using Altinn.Profile.Core.Unit.ContactPoints;
using Altinn.Profile.Core.User.ContactPoints;
using Altinn.Profile.Integrations.SblBridge;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.SblBridge
{
    public class PartyNotificationContactPointsTests
    {
        [Fact]
        public void MapToUnitContactPoints_EmptyListProvided_EmptyListReturned()
        {
            // Act
            UnitContactPointsList actual = PartyNotificationContactPoints.MapToUnitContactPoints([]);

            // Assert
            Assert.Empty(actual.ContactPointsList);
        }

        [Fact]
        public void MapToUnitContactPoints_LegacyPartyIdMappedToPartyId()
        {
            // Arrange
            List<PartyNotificationContactPoints> input = new List<PartyNotificationContactPoints>
            {
                new PartyNotificationContactPoints
                {
                    PartyId = Guid.NewGuid(),
                    LegacyPartyId = 512345,
                    OrganizationNumber = "123456789",
                    ContactPoints = new List<UserRegisteredContactPoint>
                    {
                        new UserRegisteredContactPoint
                        {
                            LegacyUserId = 212345,
                            Email = "user@domain.com",
                            MobileNumber = "12345678"
                        }
                    }
                }
            };

            UnitContactPointsList expected = new()
            {
                ContactPointsList = [new UnitContactPoints()
                {
                    OrganizationNumber = "123456789",
                    PartyId = 512345,
                    UserContactPoints = [
                        new UserContactPoints()
                        {
                            UserId = 212345,
                            Email = "user@domain.com",
                            MobileNumber = "12345678",
                            IsReserved = false,
                            NationalIdentityNumber = string.Empty
                        }
                    ]
                }
                ]
            };

            // Act
            var actual = PartyNotificationContactPoints.MapToUnitContactPoints(input);

            // Assert
            Assert.Equivalent(expected.ContactPointsList, actual.ContactPointsList);
        }
    }
}
