using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.OrganizationNotificationAddresses;
using Moq;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Core.OrganizationNotificationAddresses
{
    public class OrganizationNotificationAddressesServiceTests
    {
        private readonly Mock<IOrganizationNotificationAddressRepository> _repository;
        private readonly IOrganizationNotificationAddressesService _service;
        private readonly List<Organization> _testdata;

        public OrganizationNotificationAddressesServiceTests()
        {
            _testdata =
            [
                new()
                {
                    RegistryOrganizationId = 1,
                    RegistryOrganizationNumber = "123456789",
                    NotificationAddresses =
                    [
                        new()
                        {
                            FullAddress = "test@test.com",
                            AddressType = AddressType.Email,
                        },
                        new()
                        {
                            FullAddress = "+4798765432",
                            AddressType = AddressType.SMS,
                        },
                        new()
                        {
                            FullAddress = "+4747765432",
                            AddressType = AddressType.SMS,
                        }
                    ]
                },
                new()
                {
                    RegistryOrganizationId = 2,
                    RegistryOrganizationNumber = "987654321",
                }
            ];
            _repository = new Mock<IOrganizationNotificationAddressRepository>();
            _repository.Setup(r => r.GetOrganizationsAsync(It.IsAny<OrgContactPointLookup>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata);
            _service = new OrganizationNotificationAddressesService(_repository.Object);
        }

        [Fact]
        public async Task GetNotificationContactPoints_WhenFound_Returns()
        {
            var lookup = new OrgContactPointLookup
            {
                OrganizationNumbers = ["123456789"]
            };

            // Act
            var result = await _service.GetNotificationContactPoints(lookup, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            result.Match(
                    success =>
                    {
                        Assert.IsType<OrgContactPointsList>(success);
                        Assert.NotEmpty(success.ContactPointsList);
                        var matchedOrg1 = success.ContactPointsList.FirstOrDefault();
                        Assert.NotEmpty(matchedOrg1.EmailList);
                        Assert.Single(matchedOrg1.EmailList);
                        Assert.Equal(matchedOrg1.OrganizationNumber, _testdata[0].RegistryOrganizationNumber);
                        Assert.Equal(2, matchedOrg1.MobileNumberList.Count);
                    },
                    error => throw new Exception("No error value should be returned if SBL client respons with 200 OK."));
        }

        [Fact]
        public async Task GetNotificationContactPoints_WhenNothingFound_ReturnsError()
        {
            var lookup = new OrgContactPointLookup
            {
                OrganizationNumbers = ["123456789"]
            };
            _repository.Setup(r => r.GetOrganizationsAsync(It.IsAny<OrgContactPointLookup>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            // Act
            var result = await _service.GetNotificationContactPoints(lookup, CancellationToken.None);

            // Assert
            Assert.True(result.IsError);
            result.Match(
                success => throw new Exception("No success value should be returned if SBL client respons with 5xx."),
                error =>
                {
                    Assert.IsType<bool>(error);
                    Assert.False(error);
                });
        }
    }
}
