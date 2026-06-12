using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core.ProfessionalNotificationAddresses;
using Altinn.Profile.Core.Telemetry;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories;

using Microsoft.EntityFrameworkCore;

using Moq;

using OpenTelemetry;
using OpenTelemetry.Metrics;

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
            _repository = new ProfessionalNotificationsRepository(_dbContextFactory.Object, null);
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
            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);
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
            var result = await _repository.GetNotificationAddressAsync(userId, partyUuid, TestContext.Current.CancellationToken);

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
            var result = await _repository.GetNotificationAddressAsync(userId, partyUuid, TestContext.Current.CancellationToken);

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
        public async Task GetAllNotificationAddressesForUserAsync_WhenMultipleExist_ReturnsAllContactInfos()
        {
            // Arrange
            int userId = 42;
            Guid partyUuid1 = Guid.NewGuid();
            Guid partyUuid2 = Guid.NewGuid();

            var resources1 = new List<UserPartyContactInfoResource>
            {
                new UserPartyContactInfoResource { ResourceId = "resA" }
            };
            var resources2 = new List<UserPartyContactInfoResource>
            {
                new UserPartyContactInfoResource { ResourceId = "resB" }
            };

            await SeedUserPartyContactInfo(userId, partyUuid1, "first@example.com", "11111111", resources1);
            await SeedUserPartyContactInfo(userId, partyUuid2, "second@example.com", "22222222", resources2);

            // Act
            var result = await _repository.GetAllNotificationAddressesForUserAsync(userId, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var first = result.FirstOrDefault(c => c.PartyUuid == partyUuid1);
            var second = result.FirstOrDefault(c => c.PartyUuid == partyUuid2);

            Assert.NotNull(first);
            Assert.Equal("first@example.com", first.EmailAddress);
            Assert.Equal("11111111", first.PhoneNumber);
            Assert.Single(first.UserPartyContactInfoResources);
            Assert.Equal("resA", first.UserPartyContactInfoResources[0].ResourceId);

            Assert.NotNull(second);
            Assert.Equal("second@example.com", second.EmailAddress);
            Assert.Equal("22222222", second.PhoneNumber);
            Assert.Single(second.UserPartyContactInfoResources);
            Assert.Equal("resB", second.UserPartyContactInfoResources[0].ResourceId);
        }

        [Fact]
        public async Task GetAllNotificationAddressesForPartyAsync_WhenMultipleUsersExist_ReturnsAllContactInfos()
        {
            // Arrange
            Guid partyUuid = Guid.NewGuid();
            int userId1 = 100;
            int userId2 = 200;
            var resources1 = new List<UserPartyContactInfoResource>
            {
                new UserPartyContactInfoResource { ResourceId = "res1" }
            };
            var resources2 = new List<UserPartyContactInfoResource>
            {
                new UserPartyContactInfoResource { ResourceId = "res2" }
            };

            // Both users have valid addresses
            await SeedUserPartyContactInfo(userId1, partyUuid, "user1@example.com", "11111111", resources1);
            await SeedUserPartyContactInfo(userId2, partyUuid, "user2@example.com", "22222222", resources2);

            // Add a user with no addresses (should be filtered out)
            int userId3 = 300;
            await SeedUserPartyContactInfo(userId3, partyUuid, null, string.Empty, null);

            // Act
            var result = await _repository.GetAllNotificationAddressesForPartyAsync(partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var first = result.FirstOrDefault(c => c.UserId == userId1);
            var second = result.FirstOrDefault(c => c.UserId == userId2);

            Assert.NotNull(first);
            Assert.Equal("user1@example.com", first.EmailAddress);
            Assert.Equal("11111111", first.PhoneNumber);
            Assert.Single(first.UserPartyContactInfoResources);
            Assert.Equal("res1", first.UserPartyContactInfoResources[0].ResourceId);

            Assert.NotNull(second);
            Assert.Equal("user2@example.com", second.EmailAddress);
            Assert.Equal("22222222", second.PhoneNumber);
            Assert.Single(second.UserPartyContactInfoResources);
            Assert.Equal("res2", second.UserPartyContactInfoResources[0].ResourceId);

            // Ensure user with no addresses is not included
            Assert.DoesNotContain(result, c => c.UserId == userId3);
        }

        [Fact]
        public async Task GetAllNotificationAddressesForPartyAsync_WhenEmailAndPhoneIsEmpty_ReturnsEmptyList()
        {
            // Arrange
            Guid partyUuid = Guid.NewGuid();
            int userId1 = 100;

            // Seed empty email/phone and no addresses; method should return an empty list
            await SeedUserPartyContactInfo(userId1, partyUuid, string.Empty, string.Empty, null);

            // Act
            var result = await _repository.GetAllNotificationAddressesForPartyAsync(partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.Empty(result);
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
            var result = await _repository.AddOrUpdateNotificationAddressAsync(contactInfo, TestContext.Current.CancellationToken);

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

            await _repository.AddOrUpdateNotificationAddressAsync(contactInfo, TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.AddOrUpdateNotificationAddressAsync(secondCcontactInfo, TestContext.Current.CancellationToken);

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
            await _repository.AddOrUpdateNotificationAddressAsync(contactInfo, TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.AddOrUpdateNotificationAddressAsync(updatedContactInfo, TestContext.Current.CancellationToken);
            var storedValue = await _repository.GetNotificationAddressAsync(userId, partyUuid, TestContext.Current.CancellationToken);

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
            await _repository.AddOrUpdateNotificationAddressAsync(contactInfo, TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.AddOrUpdateNotificationAddressAsync(updatedContactInfo, TestContext.Current.CancellationToken);
            var storedValue = await _repository.GetNotificationAddressAsync(userId, partyUuid, TestContext.Current.CancellationToken);

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
            var result = await _repository.DeleteNotificationAddressAsync(userId, partyUuid, TestContext.Current.CancellationToken);

            var shouldBeEmpty = await _repository.GetNotificationAddressAsync(userId, partyUuid, TestContext.Current.CancellationToken);

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
            var result = await _repository.DeleteNotificationAddressAsync(userId, partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetAllContactInfoByEmailAddressAsync_WhenMultipleUsersExist_ReturnsAllContactInfos()
        {
            // Arrange
            string email = "shared@example.com";
            Guid partyUuid1 = Guid.NewGuid();
            Guid partyUuid2 = Guid.NewGuid();
            int userId1 = 101;
            int userId2 = 202;

            await SeedUserPartyContactInfo(userId1, partyUuid1, email, "11111111", new List<UserPartyContactInfoResource> { new() { ResourceId = "res1" } });
            await SeedUserPartyContactInfo(userId2, partyUuid2, email, "22222222", new List<UserPartyContactInfoResource> { new() { ResourceId = "res2" } });

            // Act
            var result = await _repository.GetAllContactInfoByEmailAddressAsync(email, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.UserId == userId1 && r.PartyUuid == partyUuid1 && r.EmailAddress == email);
            Assert.Contains(result, r => r.UserId == userId2 && r.PartyUuid == partyUuid2 && r.EmailAddress == email);
        }

        [Fact]
        public async Task GetAllContactInfoByEmailAddressAsync_WhenNoContacts_ReturnsEmptyList()
        {
            // Arrange
            string email = "noone@example.com";

            // Act
            var result = await _repository.GetAllContactInfoByEmailAddressAsync(email, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllContactInfoByEmailAddressAsync_ExcludesNullOrEmptyEmails()
        {
            // Arrange
            string matchingEmail = "match@example.com";
            Guid partyUuid = Guid.NewGuid();
            int userIdMatching = 303;
            int userIdNull = 404;

            await SeedUserPartyContactInfo(userIdMatching, partyUuid, matchingEmail, "33333333", null);
            await SeedUserPartyContactInfo(userIdNull, partyUuid, null, "44444444", null);

            // Act
            var result = await _repository.GetAllContactInfoByEmailAddressAsync(matchingEmail, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(userIdMatching, result[0].UserId);
            Assert.Equal(matchingEmail, result[0].EmailAddress);

            var emptyResult = await _repository.GetAllContactInfoByEmailAddressAsync(string.Empty, TestContext.Current.CancellationToken);
            Assert.NotNull(emptyResult);
            Assert.Empty(emptyResult);
        }

        [Fact]
        public async Task GetAllContactInfoByEmailAddressAsync_IsCaseInsensitive_ReturnsAllCasingMatches()
        {
            // Arrange
            string search = "User@Example.COM";
            Guid partyUuid1 = Guid.NewGuid();
            Guid partyUuid2 = Guid.NewGuid();
            int userId1 = 700;
            int userId2 = 800;

            // Seed two records that differ only by case
            await SeedUserPartyContactInfo(userId1, partyUuid1, "user@example.com", "11111111", null);
            await SeedUserPartyContactInfo(userId2, partyUuid2, "User@Example.com", "22222222", null);

            // Act
            var result = await _repository.GetAllContactInfoByEmailAddressAsync(search, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.UserId == userId1);
            Assert.Contains(result, r => r.UserId == userId2);
        }

        [Fact]
        public async Task GetAllContactInfoByEmailAddressAsync_WhenEmailIsNull_ReturnsEmptyList()
        {
            // Act
            var result = await _repository.GetAllContactInfoByEmailAddressAsync(null, TestContext.Current.CancellationToken);
            
            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllContactInfoByPhoneNumberAsync_WhenMultipleUsersExist_ReturnsAllContactInfos()
        {
            // Arrange
            string email = "testUser@test.no";
            string fullPhoneNumber = "+4792929292";

            Guid partyUuid1 = Guid.NewGuid();
            Guid partyUuid2 = Guid.NewGuid();
            int userId1 = 101;
            int userId2 = 202;

            await SeedUserPartyContactInfo(userId1, partyUuid1, email, fullPhoneNumber, new List<UserPartyContactInfoResource> { new() { ResourceId = "res1" } });
            await SeedUserPartyContactInfo(userId2, partyUuid2, email, fullPhoneNumber, new List<UserPartyContactInfoResource> { new() { ResourceId = "res2" } });

            // Act
            var result = await _repository.GetAllContactInfoByPhoneNumberAsync(fullPhoneNumber, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, r => r.UserId == userId1 && r.PartyUuid == partyUuid1 && r.PhoneNumber == fullPhoneNumber);
            Assert.Contains(result, r => r.UserId == userId2 && r.PartyUuid == partyUuid2 && r.PhoneNumber == fullPhoneNumber);
        }

        [Theory]
        [InlineData("+4798765432")]
        [InlineData("+4698765432")]
        [InlineData("004798765432")]
        [InlineData("98765432")]
        [InlineData("98765")]
        public async Task GetAllContactInfoByPhoneNumberAsync_WhenPhoneNumberHasAllowedValues_ReturnsValidResult(string inputPhoneNumber)
        {
            // Arrange           
            Guid partyUuid = Guid.NewGuid();
            int userId = 404;

            await SeedUserPartyContactInfo(userId, partyUuid, null, inputPhoneNumber, null);

            // Act
            var result = await _repository.GetAllContactInfoByPhoneNumberAsync(inputPhoneNumber, TestContext.Current.CancellationToken);
           
            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(inputPhoneNumber, result[0].PhoneNumber);
        }

        [Fact]
        public async Task GetAllContactInfoByPhoneNumberAsync_WhenNoContacts_ReturnsEmptyList()
        {
            // Arrange
            string fullPhoneNumber = "+4792929292";

            // Act
            var result = await _repository.GetAllContactInfoByPhoneNumberAsync(fullPhoneNumber, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllContactInfoByPhoneNumberAsync_ExcludesNullOrEmptyPhoneNumbers()
        {
            // Arrange
            string matchingPhoneNumber = "+4792929292";
            Guid partyUuid = Guid.NewGuid();
            int userIdMatching = 303;
            int userIdNull = 404;

            await SeedUserPartyContactInfo(userIdMatching, partyUuid, "test@test.no", matchingPhoneNumber, null);
            await SeedUserPartyContactInfo(userIdNull, partyUuid, "test@test.no", null, null);

            // Act
            var result = await _repository.GetAllContactInfoByPhoneNumberAsync(matchingPhoneNumber, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(userIdMatching, result[0].UserId);
            Assert.Equal(matchingPhoneNumber, result[0].PhoneNumber);

            var emptyResult = await _repository.GetAllContactInfoByPhoneNumberAsync(string.Empty, TestContext.Current.CancellationToken);
            Assert.NotNull(emptyResult);
            Assert.Empty(emptyResult);
        }

        [Fact]
        public async Task GetAllContactInfoByPhoneNumberAsync_WhenPhoneNumberIsNull_ReturnsEmptyList()
        {
            // Act
            var result = await _repository.GetAllContactInfoByPhoneNumberAsync(null, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}
