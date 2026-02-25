using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core.PartyGroups;
using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories;

using Microsoft.EntityFrameworkCore;

using Moq;

using Wolverine;
using Wolverine.EntityFrameworkCore;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.UserPreferences
{
    public class PartyGroupRepositoryTests : IDisposable
    {
        private bool _isDisposed;
        private readonly ProfileDbContext _databaseContext;
        private readonly PartyGroupRepository _repository;
        private readonly Mock<IDbContextFactory<ProfileDbContext>> _databaseContextFactory;
        private readonly Mock<IDbContextOutbox> _dbContextOutboxMock = new();

        public PartyGroupRepositoryTests()
        {
            var databaseContextOptions = new DbContextOptionsBuilder<ProfileDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _databaseContextFactory = new Mock<IDbContextFactory<ProfileDbContext>>();

            _databaseContextFactory.Setup(f => f.CreateDbContext())
                .Returns(new ProfileDbContext(databaseContextOptions));

            _databaseContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new ProfileDbContext(databaseContextOptions));

            _repository = new PartyGroupRepository(_databaseContextFactory.Object, _dbContextOutboxMock.Object);

            _databaseContext = _databaseContextFactory.Object.CreateDbContext();
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

        [Fact]
        public async Task GetGroups_WhenUserHasMultipleGroups_ReturnsAll()
        {
            // Arrange
            await SeedTestGroupsAsync();

            // Act
            var groups = await _repository.GetGroups(1, false, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(2, groups.Count);
            Assert.Contains(groups, g => g.Name == "Group A");
            Assert.Contains(groups, g => g.Name == "Group B");
        }

        [Fact]
        public async Task GetGroups_FilterForFavoriteIsTrue_ReturnsOnlyFavorite()
        {
            // Arrange
            await SeedTestGroupsAsync();

            // Act
            var groups = await _repository.GetGroups(1, true, TestContext.Current.CancellationToken);

            // Assert
            Assert.Single(groups);
            Assert.Contains(groups, g => g.Name == "Group A");
        }

        [Fact]
        public async Task GetGroups_WhenUserHasNoGroups_ReturnsEmptyList()
        {
            // Arrange
            await SeedTestGroupsAsync();

            // Act
            var groups = await _repository.GetGroups(5, false, TestContext.Current.CancellationToken);

            // Assert
            Assert.Empty(groups);
        }

        [Fact]
        public async Task GetFavorites_WhenUserHasMultipleGroups_ReturnsOnlyFavorite()
        {
            // Arrange
            var partyUuid = Guid.NewGuid();
            var userId = 1;

            _databaseContext.Groups.AddRange(
                new Group
                {
                    Name = "Group A",
                    GroupId = 1,
                    IsFavorite = true,
                    UserId = userId,
                    Parties = [
                    new PartyGroupAssociation { PartyUuid = partyUuid, AssociationId = 1, Created = DateTime.Now, GroupId = 1 },
                    new PartyGroupAssociation { PartyUuid = Guid.NewGuid(), AssociationId = 2, Created = DateTime.Now, GroupId = 1 }]
                },
                new Group { Name = "Group B", GroupId = 2, IsFavorite = false, UserId = userId },
                new Group { Name = "Group C", GroupId = 3, IsFavorite = false, UserId = 2 });

            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var favorites = await _repository.GetFavorites(userId, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal("Group A", favorites.Name);
            Assert.Equal(1, favorites.GroupId);
            Assert.True(favorites.IsFavorite);
            Assert.Equal(userId, favorites.UserId);
            Assert.NotNull(favorites.Parties);
            Assert.Equal(2, favorites.Parties.Count);
            Assert.Equal(partyUuid, favorites.Parties[0].PartyUuid);
            Assert.Equal(1, favorites.Parties[0].AssociationId);
            Assert.Equal(1, favorites.Parties[0].GroupId);
            Assert.NotEqual(default, favorites.Parties[0].Created);
        }

        [Fact]
        public async Task GetFavorites_WhenUserHasNoGroups_ReturnsNull()
        {
            // Act
            var group = await _repository.GetFavorites(5, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(group);
        }

        [Fact]
        public async Task GetGroups_WhenCancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _repository.GetGroups(1, false, cts.Token));
        }

        [Fact]
        public async Task AddPartyToFavorites_WhenUserHasNoGroups_GroupAndPartyIsAdded()
        {
            // Arrange
            var userId = 1;
            var partyUuid = Guid.NewGuid();

            FavoriteAddedEvent actualEventRaised = null;
            Action<FavoriteAddedEvent, DeliveryOptions> eventRaisingCallback = (ev, opts) => actualEventRaised = ev;
            MockDbContextOutbox(eventRaisingCallback);

            var preRunDateTime = DateTime.UtcNow;

            // Act
            var added = await _repository.AddPartyToFavorites(userId, partyUuid, TestContext.Current.CancellationToken);
            var postRunDateTime = DateTime.UtcNow;

            // Assert
            Assert.True(added);

            var favoriteGroup = await _repository.GetFavorites(userId, TestContext.Current.CancellationToken);

            Assert.NotNull(favoriteGroup);
            Assert.Single(favoriteGroup.Parties);
            Assert.Equal(partyUuid, favoriteGroup.Parties[0].PartyUuid);

            Assert.NotNull(actualEventRaised);
            Assert.Equal(userId, actualEventRaised.UserId);
            Assert.Equal(partyUuid, actualEventRaised.PartyUuid);

            var isRegistrationTimestampInExpectedRange = preRunDateTime < actualEventRaised.RegistrationTimestamp && actualEventRaised.RegistrationTimestamp < postRunDateTime;
            _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<FavoriteAddedEvent>(), It.IsAny<DeliveryOptions>()), Times.Once);
            Assert.True(isRegistrationTimestampInExpectedRange);
        }

        [Fact]
        public async Task AddPartyToFavorites_WhenUserHasExistingGroups_PartyIsAddedToGroup()
        {
            // Arrange
            var userId = 1;
            var partyUuid = Guid.NewGuid();

            _databaseContext.Groups.AddRange(CreateFavoriteGroup(userId, 1));

            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            FavoriteAddedEvent actualEventRaised = null;
            Action<FavoriteAddedEvent, DeliveryOptions> eventRaisingCallback = (ev, opts) => actualEventRaised = ev;
            MockDbContextOutbox(eventRaisingCallback);
            var preRunDateTime = DateTime.UtcNow;

            // Act
            var added = await _repository.AddPartyToFavorites(userId, partyUuid, TestContext.Current.CancellationToken);
            var postRunDateTime = DateTime.UtcNow;

            // Assert
            Assert.True(added);

            var favoriteGroup = await _repository.GetFavorites(userId, TestContext.Current.CancellationToken);

            Assert.NotNull(favoriteGroup);
            Assert.Single(favoriteGroup.Parties);
            Assert.Equal(partyUuid, favoriteGroup.Parties[0].PartyUuid);

            _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<FavoriteAddedEvent>(), It.IsAny<DeliveryOptions>()), Times.Once);
            Assert.NotNull(actualEventRaised);
            Assert.Equal(userId, actualEventRaised.UserId);
            Assert.Equal(partyUuid, actualEventRaised.PartyUuid);

            var isRegistrationTimestampInExpectedRange = preRunDateTime < actualEventRaised.RegistrationTimestamp && actualEventRaised.RegistrationTimestamp < postRunDateTime;
            Assert.True(isRegistrationTimestampInExpectedRange);
        }

        [Fact]
        public async Task AddPartyToFavorites_WhenPartyAlreadyInGroup_ReturnsFalseAndNothingIsAdded()
        {
            // Arrange
            var userId = 1;
            var partyUuid = Guid.NewGuid();
            var association = new PartyGroupAssociation
            {
                PartyUuid = partyUuid,
                AssociationId = 1,
                Created = DateTime.Now,
                GroupId = 1
            };
            _databaseContext.Groups.AddRange(CreateFavoriteGroup(userId, 1, parties: [association]));
            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            FavoriteAddedEvent actualEventRaised = null;
            Action<FavoriteAddedEvent, DeliveryOptions> eventRaisingCallback = (ev, opts) => actualEventRaised = ev;
            MockDbContextOutbox(eventRaisingCallback);

            // Act
            var added = await _repository.AddPartyToFavorites(userId, partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.False(added);
            _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<FavoriteAddedEvent>(), It.IsAny<DeliveryOptions>()), Times.Never);
            Assert.Null(actualEventRaised);
        }

        [Fact]
        public async Task DeleteFromFavorites_WhenUserHasNoGroups_ReturnsFalse()
        {
            // Arrange
            var userId = 1;
            var partyUuid = Guid.NewGuid();

            FavoriteRemovedEvent actualEventRaised = null;
            Action<FavoriteRemovedEvent, DeliveryOptions> eventRaisingCallback = (ev, opts) => actualEventRaised = ev;
            MockDbContextOutbox(eventRaisingCallback);

            // Act
            var deleted = await _repository.DeleteFromFavorites(userId, partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.False(deleted);
            _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<FavoriteRemovedEvent>(), It.IsAny<DeliveryOptions>()), Times.Never);
            Assert.Null(actualEventRaised);
        }

        [Fact]
        public async Task DeleteFromFavorites_WhenPartyNotFound_ReturnsFalse()
        {
            // Arrange
            var userId = 1;
            var partyUuid = Guid.NewGuid();
            _databaseContext.Groups.AddRange(CreateFavoriteGroup(userId, 1, parties: []));
            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            FavoriteRemovedEvent actualEventRaised = null;
            Action<FavoriteRemovedEvent, DeliveryOptions> eventRaisingCallback = (ev, opts) => actualEventRaised = ev;
            MockDbContextOutbox(eventRaisingCallback);

            // Act
            var deleted = await _repository.DeleteFromFavorites(userId, partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.False(deleted);
            _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<FavoriteRemovedEvent>(), It.IsAny<DeliveryOptions>()), Times.Never);
            Assert.Null(actualEventRaised);
        }

        [Fact]
        public async Task DeleteFromFavorites_WhenSuccessFullyDeleted_ReturnsTrueAndIsDeleted()
        {
            // Arrange
            var userId = 1;
            var partyUuid = Guid.NewGuid();
            var fooCreationTime = DateTime.Now;
            var association = new PartyGroupAssociation
            {
                PartyUuid = partyUuid,
                AssociationId = 1,
                Created = fooCreationTime,
                GroupId = 1
            };
            _databaseContext.Groups.AddRange(CreateFavoriteGroup(userId, 1, parties: [association]));
            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            FavoriteRemovedEvent actualEventRaised = null;
            Action<FavoriteRemovedEvent, DeliveryOptions> eventRaisingCallback = (ev, opts) => actualEventRaised = ev;
            MockDbContextOutbox(eventRaisingCallback);

            // Act
            var deleted = await _repository.DeleteFromFavorites(userId, partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.True(deleted);

            var favoriteGroup = await _repository.GetFavorites(userId, TestContext.Current.CancellationToken);

            Assert.NotNull(favoriteGroup);
            Assert.Empty(favoriteGroup.Parties);

            _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<FavoriteRemovedEvent>(), It.IsAny<DeliveryOptions>()), Times.Once);
            Assert.NotNull(actualEventRaised);
            Assert.Equal(userId, actualEventRaised.UserId);
            Assert.Equal(partyUuid, actualEventRaised.PartyUuid);
            Assert.Equal(fooCreationTime, actualEventRaised.CreationTimestamp);
        }

        [Fact]
        public async Task CreateGroup_WhenCalled_PersistsGroupToDatabase()
        {
            // Arrange
            const int UserId = 1;
            const string GroupName = "Persisted Group";

            // Act
            var createdGroup = await _repository.CreateGroup(UserId, GroupName, TestContext.Current.CancellationToken);

            // Assert
            var retrievedGroups = await _repository.GetGroups(UserId, false, TestContext.Current.CancellationToken);
            Assert.Single(retrievedGroups);
            Assert.Equal(createdGroup.GroupId, retrievedGroups[0].GroupId);
            Assert.Equal(GroupName, retrievedGroups[0].Name);
            Assert.Equal(UserId, retrievedGroups[0].UserId);
            Assert.False(retrievedGroups[0].IsFavorite);
        }

        [Fact]
        public async Task CreateGroup_WhenCreatingMultipleGroupsForSameUser_AllGroupsArePersisted()
        {
            // Arrange
            const int UserId = 1;
            const string GroupName1 = "Group 1";
            const string GroupName2 = "Group 2";
            const string GroupName3 = "Group 3";

            // Act
            var group1 = await _repository.CreateGroup(UserId, GroupName1, TestContext.Current.CancellationToken);
            var group2 = await _repository.CreateGroup(UserId, GroupName2, TestContext.Current.CancellationToken);
            var group3 = await _repository.CreateGroup(UserId, GroupName3, TestContext.Current.CancellationToken);

            // Assert
            var retrievedGroups = await _repository.GetGroups(UserId, false, TestContext.Current.CancellationToken);
            Assert.Equal(3, retrievedGroups.Count);
            Assert.Contains(retrievedGroups, g => g.Name == GroupName1);
            Assert.Contains(retrievedGroups, g => g.Name == GroupName2);
            Assert.Contains(retrievedGroups, g => g.Name == GroupName3);
            Assert.Equal(3, new HashSet<int> { group1.GroupId, group2.GroupId, group3.GroupId }.Count);
        }

        [Fact]
        public async Task CreateGroup_WhenCreatingGroupsForDifferentUsers_EachUserHasTheirOwnGroups()
        {
            // Arrange
            const int UserId1 = 1;
            const int UserId2 = 2;
            const string GroupName1 = "User 1 Group";
            const string GroupName2 = "User 2 Group";

            // Act
            await _repository.CreateGroup(UserId1, GroupName1, TestContext.Current.CancellationToken);
            await _repository.CreateGroup(UserId2, GroupName2, TestContext.Current.CancellationToken);

            // Assert
            var user1Groups = await _repository.GetGroups(UserId1, false, TestContext.Current.CancellationToken);
            var user2Groups = await _repository.GetGroups(UserId2, false, TestContext.Current.CancellationToken);

            Assert.Single(user1Groups);
            Assert.Single(user2Groups);
            Assert.Equal(GroupName1, user1Groups[0].Name);
            Assert.Equal(GroupName2, user2Groups[0].Name);
            Assert.NotEqual(user1Groups[0].GroupId, user2Groups[0].GroupId);
        }

        [Fact]
        public async Task CreateGroup_WhenCreated_PartiesCollectionIsEmptyAndISFavoritesIsFalse()
        {
            // Arrange
            const int UserId = 1;
            const string GroupName = "Empty Parties Group";

            // Act
            var createdGroup = await _repository.CreateGroup(UserId, GroupName, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(createdGroup.Parties);
            Assert.Empty(createdGroup.Parties);
            Assert.False(createdGroup.IsFavorite);

            var retrievedGroups = await _repository.GetGroups(UserId, false, TestContext.Current.CancellationToken);
            Assert.Single(retrievedGroups);
            Assert.NotNull(retrievedGroups[0].Parties);
            Assert.Empty(retrievedGroups[0].Parties);
            Assert.False(retrievedGroups[0].IsFavorite);
        }

        [Fact]
        public async Task CreateGroup_WhenCancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            const int UserId = 1;
            const string GroupName = "Cancelled Group";
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<TaskCanceledException>(
                () => _repository.CreateGroup(UserId, GroupName, cts.Token));
        }

        private static Group CreateFavoriteGroup(int userId, int groupId, string name = "Group A", List<PartyGroupAssociation> parties = null)
        {
            return new Group
            {
                Name = name,
                GroupId = groupId,
                IsFavorite = true,
                UserId = userId,
                Parties = parties ?? []
            };
        }

        private async Task SeedTestGroupsAsync()
        {
            _databaseContext.Groups.AddRange(
                new Group { Name = "Group A", GroupId = 1, IsFavorite = true, UserId = 1 },
                new Group { Name = "Group B", GroupId = 2, IsFavorite = false, UserId = 1 },
                new Group { Name = "Group C", GroupId = 3, IsFavorite = false, UserId = 2 });

            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);
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
                    await context.SaveChangesAsync(TestContext.Current.CancellationToken);
                });

            _dbContextOutboxMock
                .Setup(mock => mock.PublishAsync(It.IsAny<TEvent>(), null))
                .Callback(callback);
        }
    }
}
