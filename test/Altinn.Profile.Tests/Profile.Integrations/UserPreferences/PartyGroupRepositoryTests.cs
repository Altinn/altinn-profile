using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core.User.PartyGroups;
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

        [Fact]
        public async Task GetGroup_WhenGroupExists_ReturnsGroup()
        {
            // Arrange
            const int UserId = 1;
            const int GroupId = 10;
            var partyUuid1 = Guid.NewGuid();
            var partyUuid2 = Guid.NewGuid();

            _databaseContext.Groups.Add(new Group
            {
                GroupId = GroupId,
                UserId = UserId,
                Name = "Test Group",
                IsFavorite = false,
                Parties = [
                    new PartyGroupAssociation { PartyUuid = partyUuid1, AssociationId = 1, Created = DateTime.Now, GroupId = GroupId },
                    new PartyGroupAssociation { PartyUuid = partyUuid2, AssociationId = 2, Created = DateTime.Now, GroupId = GroupId }
                ]
            });

            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var group = await _repository.GetGroup(UserId, GroupId, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(group);
            Assert.Equal(GroupId, group.GroupId);
            Assert.Equal(UserId, group.UserId);
            Assert.Equal("Test Group", group.Name);
            Assert.False(group.IsFavorite);
            Assert.Equal(2, group.Parties.Count);
            Assert.Contains(group.Parties, p => p.PartyUuid == partyUuid1);
            Assert.Contains(group.Parties, p => p.PartyUuid == partyUuid2);
        }

        [Fact]
        public async Task GetGroup_WhenGroupDoesNotExist_ReturnsNull()
        {
            // Arrange
            const int UserId = 1;
            const int NonExistentGroupId = 999;

            await SeedTestGroupsAsync();

            // Act
            var group = await _repository.GetGroup(UserId, NonExistentGroupId, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(group);
        }

        [Fact]
        public async Task GetGroup_WhenGroupBelongsToDifferentUser_ReturnsNull()
        {
            // Arrange
            const int UserId1 = 1;
            const int UserId2 = 2;
            const int GroupId = 15;

            _databaseContext.Groups.Add(new Group
            {
                GroupId = GroupId,
                UserId = UserId2,
                Name = "User 2 Group",
                IsFavorite = false,
                Parties = []
            });

            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var group = await _repository.GetGroup(UserId1, GroupId, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(group);
        }

        [Fact]
        public async Task GetGroup_WhenGroupHasNoParties_ReturnsGroupWithEmptyParties()
        {
            // Arrange
            const int UserId = 1;
            const int GroupId = 20;

            _databaseContext.Groups.Add(new Group
            {
                GroupId = GroupId,
                UserId = UserId,
                Name = "Empty Group",
                IsFavorite = false,
                Parties = []
            });

            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var group = await _repository.GetGroup(UserId, GroupId, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(group);
            Assert.Equal(GroupId, group.GroupId);
            Assert.NotNull(group.Parties);
            Assert.Empty(group.Parties);
        }

        [Fact]
        public async Task GetGroup_WhenGroupIsFavorite_ReturnsFavoriteGroup()
        {
            // Arrange
            const int UserId = 1;
            const int GroupId = 25;

            _databaseContext.Groups.Add(new Group
            {
                GroupId = GroupId,
                UserId = UserId,
                Name = "Favorites",
                IsFavorite = true,
                Parties = [new PartyGroupAssociation { PartyUuid = Guid.NewGuid(), AssociationId = 1, Created = DateTime.Now, GroupId = GroupId }]
            });

            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var group = await _repository.GetGroup(UserId, GroupId, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(group);
            Assert.True(group.IsFavorite);
            Assert.Equal("Favorites", group.Name);
            Assert.Single(group.Parties);
        }

        [Fact]
        public async Task GetGroup_WhenCancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            const int UserId = 1;
            const int GroupId = 1;
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _repository.GetGroup(UserId, GroupId, cts.Token));
        }

        [Fact]
        public async Task GetGroup_WhenMultipleGroupsExist_ReturnsOnlyRequestedGroup()
        {
            // Arrange
            const int UserId = 1;
            const int TargetGroupId = 30;

            _databaseContext.Groups.AddRange(
                new Group { GroupId = 28, UserId = UserId, Name = "Group A", IsFavorite = false, Parties = [] },
                new Group { GroupId = 29, UserId = UserId, Name = "Group B", IsFavorite = false, Parties = [] },
                new Group { GroupId = TargetGroupId, UserId = UserId, Name = "Target Group", IsFavorite = false, Parties = [] },
                new Group { GroupId = 31, UserId = UserId, Name = "Group C", IsFavorite = false, Parties = [] });

            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var group = await _repository.GetGroup(UserId, TargetGroupId, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(group);
            Assert.Equal(TargetGroupId, group.GroupId);
            Assert.Equal("Target Group", group.Name);
        }

        [Fact]
        public async Task UpdateGroupName_WhenGroupExists_UpdatesNameAndReturnsGroup()
        {
            // Arrange
            const int UserId = 1;
            const int GroupId = 40;
            const string OriginalName = "Original Name";
            const string UpdatedName = "Updated Name";
            var partyUuid1 = Guid.NewGuid();
            var partyUuid2 = Guid.NewGuid();

            _databaseContext.Groups.Add(new Group
            {
                GroupId = GroupId,
                UserId = UserId,
                Name = OriginalName,
                IsFavorite = false,
                Parties = [
                    new PartyGroupAssociation { PartyUuid = partyUuid1, AssociationId = 1, Created = DateTime.Now, GroupId = GroupId },
                    new PartyGroupAssociation { PartyUuid = partyUuid2, AssociationId = 2, Created = DateTime.Now, GroupId = GroupId }
                ]
            });

            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.UpdateGroupName(UserId, GroupId, UpdatedName, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(GroupOperationResult.Success, result.Result);
            Assert.NotNull(result.Group);
            Assert.Equal(GroupId, result.Group.GroupId);
            Assert.Equal(UpdatedName, result.Group.Name);
            Assert.Equal(UserId, result.Group.UserId);
            Assert.False(result.Group.IsFavorite);
            Assert.Equal(2, result.Group.Parties.Count);

            var retrievedGroup = await _repository.GetGroup(UserId, GroupId, TestContext.Current.CancellationToken);
            Assert.NotNull(retrievedGroup);
            Assert.Equal(UpdatedName, retrievedGroup.Name);
        }

        [Fact]
        public async Task UpdateGroupName_WhenGroupDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            const int UserId = 1;
            const int NonExistentGroupId = 999;
            const string UpdatedName = "New Name";

            await SeedTestGroupsAsync();

            // Act
            var result = await _repository.UpdateGroupName(UserId, NonExistentGroupId, UpdatedName, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(GroupOperationResult.NotFound, result.Result);
            Assert.Null(result.Group);
        }

        [Fact]
        public async Task UpdateGroupName_WhenGroupBelongsToDifferentUser_ReturnsNotFound()
        {
            // Arrange
            const int UserId1 = 1;
            const int UserId2 = 2;
            const int GroupId = 45;
            const string OriginalName = "User 2 Group";
            const string UpdatedName = "Attempted Update";

            _databaseContext.Groups.Add(new Group
            {
                GroupId = GroupId,
                UserId = UserId2,
                Name = OriginalName,
                IsFavorite = false,
                Parties = []
            });

            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.UpdateGroupName(UserId1, GroupId, UpdatedName, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(GroupOperationResult.NotFound, result.Result);
            Assert.Null(result.Group);

            var originalGroup = await _repository.GetGroup(UserId2, GroupId, TestContext.Current.CancellationToken);
            Assert.NotNull(originalGroup);
            Assert.Equal(OriginalName, originalGroup.Name);
        }

        [Fact]
        public async Task UpdateGroupName_PreservesPartiesAndOtherProperties()
        {
            // Arrange
            const int UserId = 1;
            const int GroupId = 50;
            const string OriginalName = "Original Name";
            const string UpdatedName = "Updated Name";
            var partyUuid1 = Guid.NewGuid();
            var partyUuid2 = Guid.NewGuid();
            var partyUuid3 = Guid.NewGuid();

            _databaseContext.Groups.Add(new Group
            {
                GroupId = GroupId,
                UserId = UserId,
                Name = OriginalName,
                IsFavorite = false,
                Parties = [
                    new PartyGroupAssociation { PartyUuid = partyUuid1, AssociationId = 1, Created = DateTime.Now, GroupId = GroupId },
                    new PartyGroupAssociation { PartyUuid = partyUuid2, AssociationId = 2, Created = DateTime.Now, GroupId = GroupId },
                    new PartyGroupAssociation { PartyUuid = partyUuid3, AssociationId = 3, Created = DateTime.Now, GroupId = GroupId }
                ]
            });

            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.UpdateGroupName(UserId, GroupId, UpdatedName, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(GroupOperationResult.Success, result.Result);
            Assert.NotNull(result.Group);
            Assert.Equal(UpdatedName, result.Group.Name);
            Assert.Equal(3, result.Group.Parties.Count);
            Assert.Contains(result.Group.Parties, p => p.PartyUuid == partyUuid1);
            Assert.Contains(result.Group.Parties, p => p.PartyUuid == partyUuid2);
            Assert.Contains(result.Group.Parties, p => p.PartyUuid == partyUuid3);
            Assert.False(result.Group.IsFavorite);
            Assert.Equal(UserId, result.Group.UserId);
        }

        [Fact]
        public async Task UpdateGroupName_WhenGroupIsFavorite_ReturnsForbidden()
        {
            // Arrange
            const int UserId = 1;
            const int GroupId = 55;
            const string OriginalName = "Favorites";
            const string UpdatedName = "My Favorites";

            _databaseContext.Groups.Add(new Group
            {
                GroupId = GroupId,
                UserId = UserId,
                Name = OriginalName,
                IsFavorite = true,
                Parties = [new PartyGroupAssociation { PartyUuid = Guid.NewGuid(), AssociationId = 1, Created = DateTime.Now, GroupId = GroupId }]
            });

            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.UpdateGroupName(UserId, GroupId, UpdatedName, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(GroupOperationResult.Forbidden, result.Result);
            Assert.Null(result.Group);

            var originalGroup = await _repository.GetGroup(UserId, GroupId, TestContext.Current.CancellationToken);
            Assert.NotNull(originalGroup);
            Assert.Equal(OriginalName, originalGroup.Name);
            Assert.True(originalGroup.IsFavorite);
        }

        [Fact]
        public async Task UpdateGroupName_WhenCancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            const int UserId = 1;
            const int GroupId = 60;
            const string UpdatedName = "New Name";
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _repository.UpdateGroupName(UserId, GroupId, UpdatedName, cts.Token));
        }

        [Fact]
        public async Task DeleteGroup_WhenGroupExists_DeletesGroupAndReturnsSuccess()
        {
            // Arrange
            const int UserId = 1;
            const int GroupId = 70;
            var partyUuid1 = Guid.NewGuid();
            var partyUuid2 = Guid.NewGuid();

            _databaseContext.Groups.Add(new Group
            {
                GroupId = GroupId,
                UserId = UserId,
                Name = "Group To Delete",
                IsFavorite = false,
                Parties = [
                    new PartyGroupAssociation { PartyUuid = partyUuid1, AssociationId = 1, Created = DateTime.Now, GroupId = GroupId },
                    new PartyGroupAssociation { PartyUuid = partyUuid2, AssociationId = 2, Created = DateTime.Now, GroupId = GroupId }
                ]
            });

            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.DeleteGroup(UserId, GroupId, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(GroupOperationResult.Success, result);

            var retrievedGroup = await _repository.GetGroup(UserId, GroupId, TestContext.Current.CancellationToken);
            Assert.Null(retrievedGroup);

            var allGroups = await _repository.GetGroups(UserId, false, TestContext.Current.CancellationToken);
            Assert.DoesNotContain(allGroups, g => g.GroupId == GroupId);
        }

        [Fact]
        public async Task DeleteGroup_WhenGroupDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            const int UserId = 1;
            const int NonExistentGroupId = 999;

            await SeedTestGroupsAsync();

            // Act
            var result = await _repository.DeleteGroup(UserId, NonExistentGroupId, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(GroupOperationResult.NotFound, result);
        }

        [Fact]
        public async Task DeleteGroup_WhenGroupBelongsToDifferentUser_ReturnsNotFound()
        {
            // Arrange
            const int UserId1 = 1;
            const int UserId2 = 2;
            const int GroupId = 75;

            _databaseContext.Groups.Add(new Group
            {
                GroupId = GroupId,
                UserId = UserId2,
                Name = "User 2 Group",
                IsFavorite = false,
                Parties = []
            });

            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.DeleteGroup(UserId1, GroupId, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(GroupOperationResult.NotFound, result);

            var originalGroup = await _repository.GetGroup(UserId2, GroupId, TestContext.Current.CancellationToken);
            Assert.NotNull(originalGroup);
        }

        [Fact]
        public async Task DeleteGroup_WhenGroupIsFavorite_ReturnsForbidden()
        {
            // Arrange
            const int UserId = 1;
            const int GroupId = 80;

            _databaseContext.Groups.Add(new Group
            {
                GroupId = GroupId,
                UserId = UserId,
                Name = "Favorites",
                IsFavorite = true,
                Parties = [new PartyGroupAssociation { PartyUuid = Guid.NewGuid(), AssociationId = 1, Created = DateTime.Now, GroupId = GroupId }]
            });

            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.DeleteGroup(UserId, GroupId, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(GroupOperationResult.Forbidden, result);

            var originalGroup = await _repository.GetGroup(UserId, GroupId, TestContext.Current.CancellationToken);
            Assert.NotNull(originalGroup);
            Assert.True(originalGroup.IsFavorite);
        }

        [Fact]
        public async Task DeleteGroup_WhenGroupHasParties_DeletesGroupAndAssociations()
        {
            // Arrange
            const int UserId = 1;
            const int GroupId = 85;
            var partyUuid1 = Guid.NewGuid();
            var partyUuid2 = Guid.NewGuid();
            var partyUuid3 = Guid.NewGuid();

            _databaseContext.Groups.Add(new Group
            {
                GroupId = GroupId,
                UserId = UserId,
                Name = "Group With Parties",
                IsFavorite = false,
                Parties = [
                    new PartyGroupAssociation { PartyUuid = partyUuid1, AssociationId = 1, Created = DateTime.Now, GroupId = GroupId },
                    new PartyGroupAssociation { PartyUuid = partyUuid2, AssociationId = 2, Created = DateTime.Now, GroupId = GroupId },
                    new PartyGroupAssociation { PartyUuid = partyUuid3, AssociationId = 3, Created = DateTime.Now, GroupId = GroupId }
                ]
            });

            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.DeleteGroup(UserId, GroupId, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(GroupOperationResult.Success, result);

            var retrievedGroup = await _repository.GetGroup(UserId, GroupId, TestContext.Current.CancellationToken);
            Assert.Null(retrievedGroup);
        }

        [Fact]
        public async Task DeleteGroup_WhenCancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            const int UserId = 1;
            const int GroupId = 90;
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _repository.DeleteGroup(UserId, GroupId, cts.Token));
        }

        [Fact]
        public async Task DeleteGroup_WhenMultipleGroupsExist_DeletesOnlySpecifiedGroup()
        {
            // Arrange
            const int UserId = 1;
            const int GroupIdToDelete = 95;

            _databaseContext.Groups.AddRange(
                new Group { GroupId = 93, UserId = UserId, Name = "Group A", IsFavorite = false, Parties = [] },
                new Group { GroupId = 94, UserId = UserId, Name = "Group B", IsFavorite = false, Parties = [] },
                new Group { GroupId = GroupIdToDelete, UserId = UserId, Name = "Group To Delete", IsFavorite = false, Parties = [] },
                new Group { GroupId = 96, UserId = UserId, Name = "Group C", IsFavorite = false, Parties = [] });

            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var result = await _repository.DeleteGroup(UserId, GroupIdToDelete, TestContext.Current.CancellationToken);

            // Assert
            Assert.Equal(GroupOperationResult.Success, result);

            var allGroups = await _repository.GetGroups(UserId, false, TestContext.Current.CancellationToken);
            Assert.Equal(3, allGroups.Count);
            Assert.DoesNotContain(allGroups, g => g.GroupId == GroupIdToDelete);
            Assert.Contains(allGroups, g => g.GroupId == 93);
            Assert.Contains(allGroups, g => g.GroupId == 94);
            Assert.Contains(allGroups, g => g.GroupId == 96);
        }

        [Fact]
        public async Task AddPartyToGroup_WhenGroupExists_AddsPartyAndReturnsGroup()
        {
            // Arrange
            const int UserId = 1;
            const int GroupId = 100;
            var partyUuid = Guid.NewGuid();

            _databaseContext.Groups.Add(new Group
            {
                GroupId = GroupId,
                UserId = UserId,
                Name = "Test Group",
                IsFavorite = false,
                Parties = []
            });

            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var group = await _repository.AddPartyToGroup(UserId, GroupId, partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(group);
            Assert.Equal(GroupId, group.GroupId);
            Assert.Single(group.Parties);
            Assert.Equal(partyUuid, group.Parties[0].PartyUuid);

            var retrievedGroup = await _repository.GetGroup(UserId, GroupId, TestContext.Current.CancellationToken);
            Assert.NotNull(retrievedGroup);
            Assert.Single(retrievedGroup.Parties);
            Assert.Equal(partyUuid, retrievedGroup.Parties[0].PartyUuid);
        }

        [Fact]
        public async Task AddPartyToGroup_WhenGroupDoesNotExist_ReturnsNull()
        {
            // Arrange
            const int UserId = 1;
            const int NonExistentGroupId = 999;
            var partyUuid = Guid.NewGuid();

            await SeedTestGroupsAsync();

            // Act
            var group = await _repository.AddPartyToGroup(UserId, NonExistentGroupId, partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(group);
        }

        [Fact]
        public async Task AddPartyToGroup_WhenGroupBelongsToDifferentUser_ReturnsNull()
        {
            // Arrange
            const int UserId1 = 1;
            const int UserId2 = 2;
            const int GroupId = 105;
            var partyUuid = Guid.NewGuid();

            _databaseContext.Groups.Add(new Group
            {
                GroupId = GroupId,
                UserId = UserId2,
                Name = "User 2 Group",
                IsFavorite = false,
                Parties = []
            });

            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var group = await _repository.AddPartyToGroup(UserId1, GroupId, partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.Null(group);

            var originalGroup = await _repository.GetGroup(UserId2, GroupId, TestContext.Current.CancellationToken);
            Assert.NotNull(originalGroup);
            Assert.Empty(originalGroup.Parties);
        }

        [Fact]
        public async Task AddPartyToGroup_WhenPartyAlreadyInGroup_ReturnsGroupIdempotent()
        {
            // Arrange
            const int UserId = 1;
            const int GroupId = 110;
            var partyUuid = Guid.NewGuid();

            _databaseContext.Groups.Add(new Group
            {
                GroupId = GroupId,
                UserId = UserId,
                Name = "Test Group",
                IsFavorite = false,
                Parties = [
                    new PartyGroupAssociation { PartyUuid = partyUuid, AssociationId = 1, Created = DateTime.Now, GroupId = GroupId }
                ]
            });

            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var group = await _repository.AddPartyToGroup(UserId, GroupId, partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(group);
            Assert.Equal(GroupId, group.GroupId);
            Assert.Single(group.Parties);
            Assert.Equal(partyUuid, group.Parties[0].PartyUuid);
        }

        [Fact]
        public async Task AddPartyToGroup_WhenGroupIsFavorite_AddsPartySuccessfully()
        {
            // Arrange
            const int UserId = 1;
            const int GroupId = 115;
            var partyUuid = Guid.NewGuid();

            _databaseContext.Groups.Add(new Group
            {
                GroupId = GroupId,
                UserId = UserId,
                Name = "Favorites",
                IsFavorite = true,
                Parties = []
            });

            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var group = await _repository.AddPartyToGroup(UserId, GroupId, partyUuid, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(group);
            Assert.Equal(GroupId, group.GroupId);
            Assert.True(group.IsFavorite);
            Assert.Single(group.Parties);
            Assert.Equal(partyUuid, group.Parties[0].PartyUuid);
        }

        [Fact]
        public async Task AddPartyToGroup_AddsMultiplePartiesToGroup()
        {
            // Arrange
            const int UserId = 1;
            const int GroupId = 120;
            var partyUuid1 = Guid.NewGuid();
            var partyUuid2 = Guid.NewGuid();
            var partyUuid3 = Guid.NewGuid();

            _databaseContext.Groups.Add(new Group
            {
                GroupId = GroupId,
                UserId = UserId,
                Name = "Test Group",
                IsFavorite = false,
                Parties = []
            });

            await _databaseContext.SaveChangesAsync(TestContext.Current.CancellationToken);

            // Act
            var group1 = await _repository.AddPartyToGroup(UserId, GroupId, partyUuid1, TestContext.Current.CancellationToken);
            var group2 = await _repository.AddPartyToGroup(UserId, GroupId, partyUuid2, TestContext.Current.CancellationToken);
            var group3 = await _repository.AddPartyToGroup(UserId, GroupId, partyUuid3, TestContext.Current.CancellationToken);

            // Assert
            Assert.NotNull(group3);
            Assert.Equal(3, group3.Parties.Count);
            Assert.Contains(group3.Parties, p => p.PartyUuid == partyUuid1);
            Assert.Contains(group3.Parties, p => p.PartyUuid == partyUuid2);
            Assert.Contains(group3.Parties, p => p.PartyUuid == partyUuid3);
        }

        [Fact]
        public async Task AddPartyToGroup_WhenCancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            const int UserId = 1;
            const int GroupId = 125;
            var partyUuid = Guid.NewGuid();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(
                () => _repository.AddPartyToGroup(UserId, GroupId, partyUuid, cts.Token));
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
