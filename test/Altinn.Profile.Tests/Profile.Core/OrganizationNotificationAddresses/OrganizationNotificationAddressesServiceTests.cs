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
                    OrganizationNumber = "123456789",
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
                    OrganizationNumber = "987654321",
                }
            ];
            _repository = new Mock<IOrganizationNotificationAddressRepository>();
            _repository.Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata);
            _service = new OrganizationNotificationAddressesService(_repository.Object, null);
        }

        [Fact]
        public async Task GetNotificationContactPoints_WhenFound_Returns()
        {
            var lookup = new List<string>() { "123456789" };

            // Act
            var result = await _service.GetOrganizationNotificationAddresses(lookup, CancellationToken.None);

            // Assert
            Assert.IsType<List<Organization>>(result);
            Assert.NotEmpty(result);
            var matchedOrg1 = result.FirstOrDefault();
            Assert.Equal(matchedOrg1.OrganizationNumber, _testdata[0].OrganizationNumber);
            Assert.Equal(3, matchedOrg1.NotificationAddresses.Count);
        }

        [Fact]
        public async Task GetNotificationContactPoints_WhenNothingFound_ReturnsEmptyList()
        {
            var lookup = new List<string>() { "123456789" };

            _repository.Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Organization>());

            // Act
            var result = await _service.GetOrganizationNotificationAddresses(lookup, CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }
    }
}
