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
        private readonly OrganizationNotificationAddressesService _service;
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
            Assert.IsType<OrgContactPointsList>(result);
            Assert.NotEmpty(result.ContactPointsList);
            var matchedOrg1 = result.ContactPointsList.FirstOrDefault();
            Assert.NotEmpty(matchedOrg1.EmailList);
            Assert.Single(matchedOrg1.EmailList);
            Assert.Equal(matchedOrg1.OrganizationNumber, _testdata[0].RegistryOrganizationNumber);
            Assert.Equal(2, matchedOrg1.MobileNumberList.Count);
        }

        [Fact]
        public async Task GetNotificationContactPoints_WhenNothingFound_ReturnsEmptyList()
        {
            var lookup = new OrgContactPointLookup
            {
                OrganizationNumbers = ["123456789"]
            };
            _repository.Setup(r => r.GetOrganizationsAsync(It.IsAny<OrgContactPointLookup>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Organization>());

            // Act
            var result = await _service.GetNotificationContactPoints(lookup, CancellationToken.None);

            // Assert
            Assert.Empty(result.ContactPointsList);
        }
    }
}
