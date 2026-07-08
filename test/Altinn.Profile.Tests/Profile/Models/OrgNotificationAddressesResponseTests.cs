using System;
using System.Collections.Generic;

using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Altinn.Profile.Models;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Models;

public class OrgNotificationAddressesResponseTests
{
    [Fact]
    public void Create_ShouldReturnCorrectResponse()
    {
        // Arrange
        var organizations = new List<Organization>
        {
            new Organization
            {
                OrganizationNumber = "81234567",
                NotificationAddresses = new List<NotificationAddress>
                {
                    new NotificationAddress
                    {
                        Address = "navn",
                        AddressType = AddressType.Email,
                        Domain = "navnesen.no",
                        FullAddress = "navn@navnesen.no"
                    },
                    new NotificationAddress
                    {
                        Address = "ola.nordmann",
                        AddressType = AddressType.Email,
                        Domain = "firma.no",
                        FullAddress = "ola.nordmann@firma.no"
                    }
                }
            },
            new Organization
            {
                OrganizationNumber = "82345678",
                NotificationAddresses = new List<NotificationAddress>
                {
                    new NotificationAddress
                    {
                        Address = "99999999",
                        AddressType = AddressType.SMS,
                        Domain = "+47",
                        FullAddress = "+4799999999"
                    },
                    new NotificationAddress
                    {
                        Address = "kari.nordmann",
                        AddressType = AddressType.Email,
                        Domain = "firma.no",
                        IsSoftDeleted = true,
                        FullAddress = "kari.nordmann@firma.no"
                    }
                }
            },
            new Organization
            {
                OrganizationNumber = "83456789",
                NotificationAddresses = new List<NotificationAddress>
                {
                    new NotificationAddress
                    {
                        Address = "99999999",
                        AddressType = AddressType.SMS,
                        Domain = "+47",
                        FullAddress = "+4799999999",
                        HasRegistryAccepted = false
                    }
                }
            },
            new Organization
            {
                OrganizationNumber = "31456789"
            }
        };

        // Act
        OrgNotificationAddressesResponse actual = OrgNotificationAddressesResponse.Create(organizations);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(4, actual.ContactPointsList.Count);

        // Item 1
        Assert.Equal("81234567", actual.ContactPointsList[0].OrganizationNumber);
        Assert.Equal("navn@navnesen.no", actual.ContactPointsList[0].EmailList[0]);
        Assert.Equal("ola.nordmann@firma.no", actual.ContactPointsList[0].EmailList[1]);

        // Item 2
        Assert.Equal("82345678", actual.ContactPointsList[1].OrganizationNumber);
        Assert.Equal("+4799999999", actual.ContactPointsList[1].MobileNumberList[0]);
        Assert.Empty(actual.ContactPointsList[1].EmailList);

        // Item 3
        Assert.Equal("83456789", actual.ContactPointsList[2].OrganizationNumber);
        Assert.Empty(actual.ContactPointsList[2].MobileNumberList);
        Assert.Empty(actual.ContactPointsList[2].EmailList);

        // Item 4
        Assert.Equal("31456789", actual.ContactPointsList[3].OrganizationNumber);
        Assert.Empty(actual.ContactPointsList[3].MobileNumberList);
        Assert.Empty(actual.ContactPointsList[3].EmailList);
    }
}
