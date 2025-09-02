using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Core.ProfessionalNotificationAddresses;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.ProfessionalNotifications
{
    public class ProfessionalNotificationsRepositoryTests : IDisposable
    {
        private bool _isDisposed;
        private readonly ProfileDbContext _databaseContext;
        private readonly ProfessionalNotificationsRepository _repository;
        private readonly Mock<IDbContextFactory<ProfileDbContext>> _dbContextFactory;

        public ProfessionalNotificationsRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ProfileDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _dbContextFactory = new Mock<IDbContextFactory<ProfileDbContext>>();
            _dbContextFactory.Setup(f => f.CreateDbContext())
                .Returns(new ProfileDbContext(options));
            _dbContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .Returns<CancellationToken>(ct => Task.FromResult(new ProfileDbContext(options)));

            _databaseContext = _dbContextFactory.Object.CreateDbContext();
            _repository = new ProfessionalNotificationsRepository(_dbContextFactory.Object);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _databaseContext.Database.EnsureDeleted();
                    _databaseContext.Dispose();
                }

                _isDisposed = true;
            }
        }

        private async Task SeedUserPartyContactInfo(int userId, Guid partyUuid, string email = null, string phone = null, List<UserPartyContactInfoResource> resources = null)
        {
            var contactInfo = new UserPartyContactInfo
            {
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = email,
                PhoneNumber = phone,
                LastChanged = DateTime.UtcNow,
                UserPartyContactInfoResources = resources
            };
            _databaseContext.UserPartyContactInfo.Add(contactInfo);
            await _databaseContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetNotificationAddress_WhenExists_ReturnsContactInfoWithResources()
        {
            // Arrange
            int userId = 1;
            Guid partyUuid = Guid.NewGuid();
            var resources = new List<UserPartyContactInfoResource>
            {
                new UserPartyContactInfoResource
                {
                    ResourceId = "res1",
                    UserPartyContactInfo = null! // Will be set by EF
                }
            };
            await SeedUserPartyContactInfo(userId, partyUuid, "test@example.com", "12345678", resources);

            // Act
            var result = await _repository.GetNotificationAddressAsync(userId, partyUuid, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(partyUuid, result.PartyUuid);
            Assert.Equal("test@example.com", result.EmailAddress);
            Assert.Equal("12345678", result.PhoneNumber);
            Assert.NotNull(result.UserPartyContactInfoResources);
            Assert.Single(result.UserPartyContactInfoResources);
            Assert.Equal("res1", result.UserPartyContactInfoResources[0].ResourceId);
        }

        [Fact]
        public async Task GetNotificationAddress_WhenNotExists_ReturnsNull()
        {
            // Arrange
            int userId = 2;
            Guid partyUuid = Guid.NewGuid();

            // Act
            var result = await _repository.GetNotificationAddressAsync(userId, partyUuid, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetNotificationAddress_WhenCancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            int userId = 1;
            Guid partyUuid = Guid.NewGuid();
            await SeedUserPartyContactInfo(userId, partyUuid, "test@example.com");
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _repository.GetNotificationAddressAsync(userId, partyUuid, cts.Token));
        }

        [Fact]
        public async Task AddOrUpdateNotificationAddressAsync_WhenNew_ReturnsTrue()
        {
            // Arrange 
            int userId = 1;
            Guid partyUuid = Guid.NewGuid();
            var contactInfo = new UserPartyContactInfo
            {
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = string.Empty
            };

            // Act
            var result = await _repository.AddOrUpdateNotificationAddressAsync(contactInfo, false, CancellationToken.None);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task AddOrUpdateNotificationAddressAsync_WhenAlreadyExists_ReturnsFalse()
        {
            // Arrange
            int userId = 1;
            Guid partyUuid = Guid.NewGuid();
            var contactInfo = new UserPartyContactInfo
            {
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = string.Empty
            };

            var secondCcontactInfo = new UserPartyContactInfo
            {
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = "updated@email.com"
            };

            await _repository.AddOrUpdateNotificationAddressAsync(contactInfo, false, CancellationToken.None);

            // Act
            var result = await _repository.AddOrUpdateNotificationAddressAsync(secondCcontactInfo, false, CancellationToken.None);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task AddOrUpdateNotificationAddressAsync_WhenRemovingResources_ReturnsFalse()
        {
            // Arrange
            int userId = 1;
            Guid partyUuid = Guid.NewGuid();
            var contactInfo = new UserPartyContactInfo
            {
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = string.Empty,
                UserPartyContactInfoResources =
                [
                    new() { ResourceId = "res1" },
                    new() { ResourceId = "res2" }
                ]
            };
            var updatedContactInfo = new UserPartyContactInfo
            {
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = "some@value.com",
                UserPartyContactInfoResources =
                [
                    new() { ResourceId = "res1" } // Removing "res2"
                ]
            };
            await _repository.AddOrUpdateNotificationAddressAsync(contactInfo, false, CancellationToken.None);

            // Act
            var result = await _repository.AddOrUpdateNotificationAddressAsync(updatedContactInfo, false, CancellationToken.None);
            var storedValue = await _repository.GetNotificationAddressAsync(userId, partyUuid, CancellationToken.None);

            // Assert
            Assert.False(result);
            Assert.NotNull(storedValue);
            Assert.Single(storedValue.UserPartyContactInfoResources);
            Assert.Equal("res1", storedValue.UserPartyContactInfoResources[0].ResourceId);
            Assert.Equal("some@value.com", storedValue.EmailAddress);
        }

        [Fact]
        public async Task AddOrUpdateNotificationAddressAsync_WhenEditingResources_ReturnsFalse()
        {
            // Arrange
            int userId = 1;
            Guid partyUuid = Guid.NewGuid();
            var contactInfo = new UserPartyContactInfo
            {
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = string.Empty,
                UserPartyContactInfoResources =
                [
                    new() { ResourceId = "urn:altinn:resource:res1" },
                ]
            };
            var updatedContactInfo = new UserPartyContactInfo
            {
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = "some@value.com",
                UserPartyContactInfoResources =
                [
                    new() { ResourceId = "urn:altinn:resource:res2" } 
                ]
            };
            await _repository.AddOrUpdateNotificationAddressAsync(contactInfo, false, CancellationToken.None);

            // Act
            var result = await _repository.AddOrUpdateNotificationAddressAsync(updatedContactInfo, false, CancellationToken.None);
            var storedValue = await _repository.GetNotificationAddressAsync(userId, partyUuid, CancellationToken.None);

            // Assert
            Assert.False(result);
            Assert.NotNull(storedValue);
            Assert.Single(storedValue.UserPartyContactInfoResources);
            Assert.Equal("urn:altinn:resource:res2", storedValue.UserPartyContactInfoResources[0].ResourceId);
            Assert.Equal("some@value.com", storedValue.EmailAddress);
        }

        [Fact]
        public async Task DeleteNotificationAddress_WhenExists_ReturnsContactInfoWithResources()
        {
            // Arrange
            int userId = 1;
            Guid partyUuid = Guid.NewGuid();
            var resources = new List<UserPartyContactInfoResource>
            {
                new() 
                {
                    ResourceId = "res1",
                    UserPartyContactInfo = null! // Will be set by EF
                }
            };
            await SeedUserPartyContactInfo(userId, partyUuid, "test@example.com", "12345678", resources);

            // Act
            var result = await _repository.DeleteNotificationAddressAsync(userId, partyUuid, CancellationToken.None);

            var shouldBeEmpty = await _repository.GetNotificationAddressAsync(userId, partyUuid, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(partyUuid, result.PartyUuid);
            Assert.Equal("test@example.com", result.EmailAddress);
            Assert.Equal("12345678", result.PhoneNumber);
            Assert.NotNull(result.UserPartyContactInfoResources);
            Assert.Single(result.UserPartyContactInfoResources);
            Assert.Equal("res1", result.UserPartyContactInfoResources[0].ResourceId);

            Assert.Null(shouldBeEmpty); // Ensure the contact info is deleted
        }

        [Fact]
        public async Task DeleteNotificationAddress_WhenNotExists_ReturnsNull()
        {
            // Arrange
            int userId = 2;
            Guid partyUuid = Guid.NewGuid();

            // Act
            var result = await _repository.DeleteNotificationAddressAsync(userId, partyUuid, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }
    }
}
