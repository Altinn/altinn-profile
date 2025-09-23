using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Core.ProfessionalNotificationAddresses;
using Altinn.Profile.Core.Telemetry;
using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.ProfessionalNotifications
{
    public class ProfessionalNotificationsRepositoryTests : IDisposable
    {
        private bool _isDisposed;
        private readonly ProfileDbContext _databaseContext;
        private readonly ProfessionalNotificationsRepository _repository;
        private readonly Mock<IDbContextFactory<ProfileDbContext>> _dbContextFactory;
        private readonly Mock<IDbContextOutbox> _dbContextOutboxMock = new();

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
            _repository = new ProfessionalNotificationsRepository(_dbContextFactory.Object, _dbContextOutboxMock.Object, null);
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

        private void MockDbContextOutbox<TEvent>(Action<TEvent, DeliveryOptions> callback)
        {
            DbContext context = null;

            _dbContextOutboxMock
                .Setup(mock => mock.Enroll(It.IsAny<DbContext>()))
                .Callback<DbContext>(ctx =>
                {
                    context = ctx;
                });

            _dbContextOutboxMock
                .Setup(mock => mock.SaveChangesAndFlushMessagesAsync(It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    await context.SaveChangesAsync(CancellationToken.None);
                });

            _dbContextOutboxMock
                .Setup(mock => mock.PublishAsync(It.IsAny<TEvent>(), null))
                .Callback(callback);
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
            var result = await _repository.GetAllNotificationAddressesForUserAsync(userId, CancellationToken.None);

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
            var result = await _repository.AddOrUpdateNotificationAddressAsync(contactInfo, CancellationToken.None);

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

            NotificationSettingsAddedEvent actualEventRaised = null;
            void EventRaisingCallback(NotificationSettingsAddedEvent ev, DeliveryOptions opts) => actualEventRaised = ev;
            MockDbContextOutbox((Action<NotificationSettingsAddedEvent, DeliveryOptions>)EventRaisingCallback);

            await _repository.AddOrUpdateNotificationAddressAsync(contactInfo, CancellationToken.None);

            // Act
            var result = await _repository.AddOrUpdateNotificationAddressAsync(secondCcontactInfo, CancellationToken.None);

            // Assert
            Assert.False(result);
            _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<NotificationSettingsAddedEvent>(), It.IsAny<DeliveryOptions>()), Times.Once);
            _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<NotificationSettingsUpdatedEvent>(), It.IsAny<DeliveryOptions>()), Times.Once);
        }

        [Fact]
        public async Task AddOrUpdateNotificationAddressAsync_WhenRemovingResources_ReturnsFalse()
        {
            // Arrange
            NotificationSettingsAddedEvent actualEventRaised = null;
            void EventRaisingCallback(NotificationSettingsAddedEvent ev, DeliveryOptions opts) => actualEventRaised = ev;
            MockDbContextOutbox((Action<NotificationSettingsAddedEvent, DeliveryOptions>)EventRaisingCallback);

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
            await _repository.AddOrUpdateNotificationAddressAsync(contactInfo, CancellationToken.None);

            // Act
            var result = await _repository.AddOrUpdateNotificationAddressAsync(updatedContactInfo, CancellationToken.None);
            var storedValue = await _repository.GetNotificationAddressAsync(userId, partyUuid, CancellationToken.None);

            // Assert
            Assert.False(result);
            Assert.NotNull(storedValue);
            Assert.Single(storedValue.UserPartyContactInfoResources);
            Assert.Equal("res1", storedValue.UserPartyContactInfoResources[0].ResourceId);
            Assert.Equal("some@value.com", storedValue.EmailAddress);

            _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<NotificationSettingsAddedEvent>(), It.IsAny<DeliveryOptions>()), Times.Once);
            _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<NotificationSettingsUpdatedEvent>(), It.IsAny<DeliveryOptions>()), Times.Once);
        }

        [Fact]
        public async Task AddOrUpdateNotificationAddressAsync_WhenEditingResources_ReturnsFalse()
        {
            NotificationSettingsAddedEvent actualEventRaised = null;
            void EventRaisingCallback(NotificationSettingsAddedEvent ev, DeliveryOptions opts) => actualEventRaised = ev;
            MockDbContextOutbox((Action<NotificationSettingsAddedEvent, DeliveryOptions>)EventRaisingCallback);

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
            await _repository.AddOrUpdateNotificationAddressAsync(contactInfo, CancellationToken.None);

            // Act
            var result = await _repository.AddOrUpdateNotificationAddressAsync(updatedContactInfo, CancellationToken.None);
            var storedValue = await _repository.GetNotificationAddressAsync(userId, partyUuid, CancellationToken.None);

            // Assert
            Assert.False(result);
            Assert.NotNull(storedValue);
            Assert.Single(storedValue.UserPartyContactInfoResources);
            Assert.Equal("urn:altinn:resource:res2", storedValue.UserPartyContactInfoResources[0].ResourceId);
            Assert.Equal("some@value.com", storedValue.EmailAddress);

            _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<NotificationSettingsAddedEvent>(), It.IsAny<DeliveryOptions>()), Times.Once);
            _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<NotificationSettingsUpdatedEvent>(), It.IsAny<DeliveryOptions>()), Times.Once);
        }

        [Fact]
        public async Task DeleteNotificationAddress_WhenExists_ReturnsContactInfoWithResources()
        {
            // Arrange
            NotificationSettingsDeletedEvent actualDeleteEventRaised = null;
            void DeleteEventRaisingCallback(NotificationSettingsDeletedEvent ev, DeliveryOptions opts) => actualDeleteEventRaised = ev;
            MockDbContextOutbox((Action<NotificationSettingsDeletedEvent, DeliveryOptions>)DeleteEventRaisingCallback);

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

            _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<NotificationSettingsDeletedEvent>(), It.IsAny<DeliveryOptions>()), Times.Once);
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

        [Fact]
        public async Task AddOrUpdateNotificationAddressFromSyncAsync_WhenNew_AddsContactInfo()
        {
            // Arrange
            int userId = 10;
            Guid partyUuid = Guid.NewGuid();
            var contactInfo = new UserPartyContactInfo
            {
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = "sync@new.com",
                PhoneNumber = "99999999",
                UserPartyContactInfoResources =
                [
                    new() { ResourceId = "sync-res1" }
                ]
            };

            // Act
            await _repository.AddOrUpdateNotificationAddressFromSyncAsync(contactInfo, CancellationToken.None);
            var stored = await _repository.GetNotificationAddressAsync(userId, partyUuid, CancellationToken.None);

            // Assert
            Assert.NotNull(stored);
            Assert.Equal("sync@new.com", stored.EmailAddress);
            Assert.Equal("99999999", stored.PhoneNumber);
            Assert.Single(stored.UserPartyContactInfoResources);
            Assert.Equal("sync-res1", stored.UserPartyContactInfoResources[0].ResourceId);
        }

        [Fact]
        public async Task AddOrUpdateNotificationAddressFromSyncAsync_WhenExists_UpdatesContactInfo()
        {
            // Arrange
            int userId = 11;
            Guid partyUuid = Guid.NewGuid();
            var original = new UserPartyContactInfo
            {
                LastChanged = DateTime.UtcNow.AddDays(-1),
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = "original@sync.com",
                PhoneNumber = "11111111",
                UserPartyContactInfoResources = new List<UserPartyContactInfoResource>
                {
                    new() { ResourceId = "sync-res2" }
                }
            };
            await _repository.AddOrUpdateNotificationAddressFromSyncAsync(original, CancellationToken.None);

            var updated = new UserPartyContactInfo
            {
                LastChanged = DateTime.UtcNow,
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = "updated@sync.com",
                PhoneNumber = "22222222",
                UserPartyContactInfoResources = new List<UserPartyContactInfoResource>
                {
                    new() { ResourceId = "sync-res3" }
                }
            };

            // Act
            await _repository.AddOrUpdateNotificationAddressFromSyncAsync(updated, CancellationToken.None);
            var stored = await _repository.GetNotificationAddressAsync(userId, partyUuid, CancellationToken.None);

            // Assert
            Assert.NotNull(stored);
            Assert.Equal("updated@sync.com", stored.EmailAddress);
            Assert.Equal("22222222", stored.PhoneNumber);
            Assert.Single(stored.UserPartyContactInfoResources);
            Assert.Equal("sync-res3", stored.UserPartyContactInfoResources[0].ResourceId);
        }

        [Fact]
        public async Task AddOrUpdateNotificationAddressFromSyncAsync_WhenUpdateISOlderThanCurrent_DoesNothing()
        {
            // Arrange
            int userId = 11;
            Guid partyUuid = Guid.NewGuid();
            var original = new UserPartyContactInfo
            {
                LastChanged = DateTime.UtcNow,
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = "original@sync.com",
                PhoneNumber = "11111111",
                UserPartyContactInfoResources = new List<UserPartyContactInfoResource>
                {
                    new() { ResourceId = "sync-res2" }
                }
            };
            await _repository.AddOrUpdateNotificationAddressFromSyncAsync(original, CancellationToken.None);

            var updated = new UserPartyContactInfo
            {
                LastChanged = DateTime.UtcNow.AddDays(-1),
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = "updated@sync.com",
                PhoneNumber = "22222222",
                UserPartyContactInfoResources = new List<UserPartyContactInfoResource>
                {
                    new() { ResourceId = "sync-res3" }
                }
            };

            // Act
            await _repository.AddOrUpdateNotificationAddressFromSyncAsync(updated, CancellationToken.None);
            var stored = await _repository.GetNotificationAddressAsync(userId, partyUuid, CancellationToken.None);

            // Assert
            Assert.NotNull(stored);
            Assert.Equal("original@sync.com", stored.EmailAddress);
            Assert.Equal("11111111", stored.PhoneNumber);
            Assert.Single(stored.UserPartyContactInfoResources);
            Assert.Equal("sync-res2", stored.UserPartyContactInfoResources[0].ResourceId);
        }

        [Fact]
        public async Task AddOrUpdateNotificationAddressFromSyncAsync_WhenRemovingResources_RemovesResource()
        {
            // Arrange
            int userId = 12;
            Guid partyUuid = Guid.NewGuid();
            var contactInfo = new UserPartyContactInfo
            {
                LastChanged = DateTime.UtcNow.AddDays(-1),
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = "remove@sync.com",
                UserPartyContactInfoResources = new List<UserPartyContactInfoResource>
                {
                    new() { ResourceId = "sync-res4" },
                    new() { ResourceId = "sync-res5" }
                }
            };
            await _repository.AddOrUpdateNotificationAddressFromSyncAsync(contactInfo, CancellationToken.None);

            var updated = new UserPartyContactInfo
            {
                LastChanged = DateTime.UtcNow,
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = "remove@sync.com",
                UserPartyContactInfoResources = new List<UserPartyContactInfoResource>
                {
                    new() { ResourceId = "sync-res4" } // "sync-res5" removed
                }
            };

            // Act
            await _repository.AddOrUpdateNotificationAddressFromSyncAsync(updated, CancellationToken.None);
            var stored = await _repository.GetNotificationAddressAsync(userId, partyUuid, CancellationToken.None);

            // Assert
            Assert.NotNull(stored);
            Assert.Single(stored.UserPartyContactInfoResources);
            Assert.Equal("sync-res4", stored.UserPartyContactInfoResources[0].ResourceId);
        }

        [Fact]
        public async Task AddOrUpdateNotificationAddressFromSyncAsync_WhenEditingResources_UpdatesResource()
        {
            // Arrange
            int userId = 13;
            Guid partyUuid = Guid.NewGuid();
            var contactInfo = new UserPartyContactInfo
            {
                UserId = userId,
                PartyUuid = partyUuid,
                LastChanged = DateTime.UtcNow.AddDays(-1),
                EmailAddress = "edit@sync.com",
                UserPartyContactInfoResources = new List<UserPartyContactInfoResource>
                {
                    new() { ResourceId = "sync-res6" }
                }
            };
            await _repository.AddOrUpdateNotificationAddressFromSyncAsync(contactInfo, CancellationToken.None);

            var updated = new UserPartyContactInfo
            {
                LastChanged = DateTime.UtcNow,
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = "edit@sync.com",
                UserPartyContactInfoResources = new List<UserPartyContactInfoResource>
                {
                    new() { ResourceId = "sync-res7" }
                }
            };

            // Act
            await _repository.AddOrUpdateNotificationAddressFromSyncAsync(updated, CancellationToken.None);
            var stored = await _repository.GetNotificationAddressAsync(userId, partyUuid, CancellationToken.None);

            // Assert
            Assert.NotNull(stored);
            Assert.Single(stored.UserPartyContactInfoResources);
            Assert.Equal("sync-res7", stored.UserPartyContactInfoResources[0].ResourceId);
        }

        [Fact]
        public async Task AddOrUpdateNotificationAddressFromSyncAsync_WhenCancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            int userId = 14;
            Guid partyUuid = Guid.NewGuid();
            var contactInfo = new UserPartyContactInfo
            {
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = "cancel@sync.com"
            };
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _repository.AddOrUpdateNotificationAddressFromSyncAsync(contactInfo, cts.Token));
        }

        // --- Telemetry meter tests ---
        [Fact]
        public async Task AddOrUpdateNotificationAddressFromSyncAsync_EmitsNotificationAddressAddedMetric()
        {
            var metricItems = new List<Metric>();
            using var meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter(Telemetry.AppName)
                .AddInMemoryExporter(metricItems)
                .Build();

            using var telemetry = new Telemetry();
            var repository = new ProfessionalNotificationsRepository(_dbContextFactory.Object, _dbContextOutboxMock.Object, telemetry);

            int userId = 1001;
            Guid partyUuid = Guid.NewGuid();
            var contactInfo = new UserPartyContactInfo
            {
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = "added@example.com",
                PhoneNumber = "12345678",
                LastChanged = DateTime.UtcNow,
                UserPartyContactInfoResources = []
            };

            await repository.AddOrUpdateNotificationAddressFromSyncAsync(contactInfo, CancellationToken.None);
            
            meterProvider.ForceFlush();

            var addedMetric = metricItems.Single(item => item.Name == Telemetry.Metrics.CreateName("notificationsettings.added"));
            long sum = 0;
            foreach (ref readonly var p in addedMetric.GetMetricPoints())
            {
                sum += p.GetSumLong();
            }

            Assert.Equal(1, sum);
        }

        [Fact]
        public async Task AddOrUpdateNotificationAddressFromSyncAsync_EmitsNotificationAddressUpdatedMetric()
        {
            var metricItems = new List<Metric>();
            using var meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter(Telemetry.AppName)
                .AddInMemoryExporter(metricItems)
                .Build();

            using var telemetry = new Telemetry();
            var repository = new ProfessionalNotificationsRepository(_dbContextFactory.Object, _dbContextOutboxMock.Object, telemetry);

            int userId = 1002;
            Guid partyUuid = Guid.NewGuid();
            var original = new UserPartyContactInfo
            {
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = "original@example.com",
                PhoneNumber = "11111111",
                LastChanged = DateTime.UtcNow.AddMinutes(-10),
                UserPartyContactInfoResources = new List<UserPartyContactInfoResource>()
            };
            _databaseContext.UserPartyContactInfo.Add(original);
            await _databaseContext.SaveChangesAsync();

            var updated = new UserPartyContactInfo
            {
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = "updated@example.com",
                PhoneNumber = "22222222",
                LastChanged = DateTime.UtcNow,
                UserPartyContactInfoResources = new List<UserPartyContactInfoResource>()
            };

            await repository.AddOrUpdateNotificationAddressFromSyncAsync(updated, CancellationToken.None);

            meterProvider.ForceFlush();

            var updatedMetric = metricItems.Single(item => item.Name == Telemetry.Metrics.CreateName("notificationsettings.updated"));
            long sum = 0;
            foreach (ref readonly var p in updatedMetric.GetMetricPoints())
            {
                sum += p.GetSumLong();
            }

            Assert.Equal(1, sum);
        }

        [Fact]
        public async Task DeleteNotificationAddressFromSyncAsync_EmitsNotificationAddressDeletedMetric()
        {
            var metricItems = new List<Metric>();
            using var meterProvider = Sdk.CreateMeterProviderBuilder()
                .AddMeter(Telemetry.AppName)
                .AddInMemoryExporter(metricItems)
                .Build();

            using var telemetry = new Telemetry();
            var repository = new ProfessionalNotificationsRepository(_dbContextFactory.Object, _dbContextOutboxMock.Object, telemetry);

            int userId = 1003;
            Guid partyUuid = Guid.NewGuid();
            var contactInfo = new UserPartyContactInfo
            {
                UserId = userId,
                PartyUuid = partyUuid,
                EmailAddress = "delete@example.com",
                PhoneNumber = "33333333",
                LastChanged = DateTime.UtcNow,
                UserPartyContactInfoResources = new List<UserPartyContactInfoResource>()
            };
            _databaseContext.UserPartyContactInfo.Add(contactInfo);
            await _databaseContext.SaveChangesAsync();

            await repository.DeleteNotificationAddressFromSyncAsync(userId, partyUuid, CancellationToken.None);

            meterProvider.ForceFlush();

            var notificationsettingsDeleted = metricItems.Single(item => item.Name == Telemetry.Metrics.CreateName("notificationsettings.deleted"));
            long sum = 0;
            foreach (ref readonly var p in notificationsettingsDeleted.GetMetricPoints())
            {
                sum += p.GetSumLong();
            }

            Assert.Equal(1, sum);
        }
    }
}
