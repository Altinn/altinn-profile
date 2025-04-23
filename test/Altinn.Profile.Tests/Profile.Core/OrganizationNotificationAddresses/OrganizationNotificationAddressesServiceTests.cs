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
        private readonly Mock<IOrganizationNotificationAddressUpdateClient> _updateClient;

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

            _updateClient = new Mock<IOrganizationNotificationAddressUpdateClient>();

            _service = new OrganizationNotificationAddressesService(_repository.Object, _updateClient.Object);
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

        [Fact]
        public async Task CreateNotificationAddress_SuccessfulCreation_ReturnsOrganization()
        { 
            // Arrange
            _updateClient.Setup(c => c.CreateNewNotificationAddress(It.IsAny<NotificationAddress>(), It.IsAny<string>()))
                .ReturnsAsync(("registry-id", null));
            
            _repository.Setup(r => r.CreateNotificationAddressAsync(It.IsAny<string>(), It.IsAny<NotificationAddress>()))
                .ReturnsAsync(new Organization { NotificationAddresses = [], OrganizationNumber = "123456789" });
            
            // Act
            var result = await _service.CreateNotificationAddress("123456789", new NotificationAddress(), CancellationToken.None); 
            
            // Assert
            Assert.NotNull(result); 
        }

        [Fact]
        public async Task CreateNotificationAddress_WhenDuplicateIsFound_ReturnsOrganizationWithoutUpdate()
        {
            // Arrange
            _repository.Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_testdata.Where(o => o.OrganizationNumber == "123456789"));
            
            // Act
            var result = await _service.CreateNotificationAddress("123456789", new NotificationAddress { FullAddress = "test@test.com", AddressType = AddressType.Email }, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            _updateClient.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateNotificationAddress_WhenRegistryReturnsNull_AddsAddressWithUpdateMessageAndReturnsOrg()
        {
            // Arrange
            var orgNum = _testdata[0].OrganizationNumber;
            var newAddress = new NotificationAddress
            {
                FullAddress = "null@test.com",
                AddressType = AddressType.Email
            };

            // Simulate registry returning null id and error message
            _updateClient
                .Setup(u => u.CreateNewNotificationAddress(newAddress, orgNum))
                .ReturnsAsync((null, "Error occurred"));

            // Act
            var result = await _service.CreateNotificationAddress(orgNum, newAddress, CancellationToken.None);

            // Assert
            Assert.Equal(orgNum, result.OrganizationNumber);
            Assert.Contains(result.NotificationAddresses, na => na.FullAddress == newAddress.FullAddress);
            Assert.Equal("Error occurred", result.NotificationAddresses.First(na => na.FullAddress == newAddress.FullAddress).UpdateMessage);
        }

        [Fact]
        public async Task CreateNotificationAddress_WhenUpdateClientThrows_ThrowsExceptionWithContext()
        {
            // Arrange
            var orgNum = _testdata[0].OrganizationNumber;
            var newAddress = new NotificationAddress
            {
                FullAddress = "throw@test.com",
                AddressType = AddressType.SMS
            };
            var innerEx = new InvalidOperationException("Something went wrong");

            _updateClient
                .Setup(u => u.CreateNewNotificationAddress(newAddress, orgNum))
                .ThrowsAsync(innerEx);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _service.CreateNotificationAddress(orgNum, newAddress, CancellationToken.None));
            Assert.Contains("Something went wrong", ex.Message);
        }
    }
}
