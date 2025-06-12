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
        private readonly Mock<IRegisterClient> _registerClient;

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
                            NotificationAddressID = 1
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
            _registerClient = new Mock<IRegisterClient>();

            _service = new OrganizationNotificationAddressesService(_repository.Object, _updateClient.Object, _registerClient.Object);
        }

        [Fact]
        public async Task GetOrganizationNotificationAddresses_WhenFound_Returns()
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
        public async Task GetOrganizationNotificationAddresses_WhenNothingFound_ReturnsEmptyList()
        {
            var lookup = new List<string>() { "123456789" };

            _repository.Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            // Act
            var result = await _service.GetOrganizationNotificationAddresses(lookup, CancellationToken.None);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetOrganizationNotificationAddresses_WhenLookingUpMainUnitAndNothingFound_ReturnsEmptyList()
        {
            var lookup = new List<string>() { "123456789" };

            _repository.Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            _registerClient.Setup(r => r.GetMainUnit(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string)null);

            // Act
            var result = await _service.GetOrganizationNotificationAddresses(lookup, CancellationToken.None, true);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetOrganizationNotificationAddresses_WhenLookingUpMainUnitAndFound_ReturnsAddressFromMainUnit()
        {
            var lookup = new List<string>() { "111111111" };

            _repository.SetupSequence(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([])
                .ReturnsAsync(_testdata.Where(o => o.OrganizationNumber == "123456789"));
            _registerClient.Setup(r => r.GetMainUnit(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string)null);

            // Act
            var result = await _service.GetOrganizationNotificationAddresses(lookup, CancellationToken.None, true);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task CreateNotificationAddress_SuccessfulCreation_ReturnsOrganization()
        { 
            // Arrange
            _updateClient.Setup(c => c.CreateNewNotificationAddress(It.IsAny<NotificationAddress>(), It.IsAny<string>()))
                .ReturnsAsync("registry-id");
            
            _repository.Setup(r => r.CreateNotificationAddressAsync(It.IsAny<string>(), It.IsAny<NotificationAddress>(), It.IsAny<string>()))
                .ReturnsAsync(new NotificationAddress { });
            
            // Act
            var result = await _service.CreateNotificationAddress("123456789", new NotificationAddress(), CancellationToken.None); 
            
            // Assert
            Assert.NotNull(result.Address); 
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
            Assert.NotNull(result.Address);
            _updateClient.VerifyNoOtherCalls();
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

        [Fact]
        public async Task UpdateNotificationAddress_SuccessfulUpdate_ReturnsUpdatedAddress()
        {
            // Arrange
            _updateClient.Setup(c => c.UpdateNotificationAddress(It.IsAny<string>(), It.IsAny<NotificationAddress>(), It.IsAny<string>()))
                .ReturnsAsync("registry-id");

            _repository.Setup(r => r.UpdateNotificationAddressAsync(It.IsAny<NotificationAddress>(), It.IsAny<string>()))
                .ReturnsAsync(new NotificationAddress { });

            // Act
            var result = await _service.UpdateNotificationAddress("123456789", new NotificationAddress { NotificationAddressID = 1 }, CancellationToken.None);

            // Assert
            Assert.NotNull(result.Address);
        }

        [Fact]
        public async Task DeleteNotificationAddress_SuccessfulDeletion_ReturnsDeletedAddress()
        {
            // Arrange
            _updateClient.Setup(c => c.DeleteNotificationAddress(It.IsAny<string>()))
                .ReturnsAsync("registry-id");

            _repository.Setup(r => r.DeleteNotificationAddressAsync(It.IsAny<int>()))
                .ReturnsAsync(new NotificationAddress { IsSoftDeleted = true });

            // Act
            var result = await _service.DeleteNotificationAddress("123456789", 1, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task DeleteNotificationAddress_WhenTryingToDeleteLastAddress_ThrowsInvalidOperationException()
        {
            // Arrange
            _repository.Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([new Organization { OrganizationNumber = "123456789", NotificationAddresses = [new NotificationAddress { NotificationAddressID = 1 }] }]);

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteNotificationAddress("123456789", 1, CancellationToken.None));

            // Assert
            Assert.Contains("Cannot delete the last notification address", ex.Message);
        }

        [Fact]
        public async Task DeleteNotificationAddress_WhenNoAddressFound_ReturnsNull()
        {
            // Act
            var result = await _service.DeleteNotificationAddress("123456789", 10000, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task DeleteNotificationAddress_WhenNoOrganizationFound_ReturnsNull()
        {
            // Arrange
            _repository.Setup(r => r.GetOrganizationsAsync(It.IsAny<List<string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

            // Act
            var result = await _service.DeleteNotificationAddress("1", 1, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
    }
}
