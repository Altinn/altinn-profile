﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Core.PartyGroups;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.UserPreferences
{
    public class PartyGroupRepositoryTests : IDisposable
    {
        private bool _isDisposed;
        private readonly ProfileDbContext _databaseContext;
        private readonly PartyGroupRepository _repository;
        private readonly Mock<IDbContextFactory<ProfileDbContext>> _databaseContextFactory;

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

            _repository = new PartyGroupRepository(_databaseContextFactory.Object);

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

        private async Task SeedTestGroups()
        {
            _databaseContext.Groups.AddRange(
                new Group { Name = "Group A", GroupId = 1, IsFavorite = true, UserId = 1 },
                new Group { Name = "Group B", GroupId = 2, IsFavorite = false, UserId = 1 },
                new Group { Name = "Group C", GroupId = 3, IsFavorite = false, UserId = 2 });

            await _databaseContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetGroups_WhenUserHasMultipleGroups_ReturnsAll()
        {
            // Arrange
            await SeedTestGroups();

            // Act
            var groups = await _repository.GetGroups(1, false, CancellationToken.None);

            // Assert
            Assert.Equal(2, groups.Count);
            Assert.Contains(groups, g => g.Name == "Group A");
            Assert.Contains(groups, g => g.Name == "Group B");
        }

        [Fact]
        public async Task GetGroups_FilterForFavoriteIsTrue_ReturnsOnlyFavorite()
        {
            // Arrange
            await SeedTestGroups();

            // Act
            var groups = await _repository.GetGroups(1, true, CancellationToken.None);

            // Assert
            Assert.Single(groups);
            Assert.Contains(groups, g => g.Name == "Group A");
        }

        [Fact]
        public async Task GetGroups_WhenUserHasNoGroups_ReturnsEmptyList()
        {
            // Arrange
            await SeedTestGroups();

            // Act
            var groups = await _repository.GetGroups(5, false, CancellationToken.None);

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
                    Name = "Group A", GroupId = 1, IsFavorite = true, UserId = userId, Parties = [
                    new PartyGroupAssociation { PartyUuid = partyUuid, AssociationId = 1, Created = DateTime.Now, GroupId = 1 },
                    new PartyGroupAssociation { PartyUuid = Guid.NewGuid(), AssociationId = 2, Created = DateTime.Now, GroupId = 1 }] 
                },
                new Group { Name = "Group B", GroupId = 2, IsFavorite = false, UserId = userId },
                new Group { Name = "Group C", GroupId = 3, IsFavorite = false, UserId = 2 });

            await _databaseContext.SaveChangesAsync();

            // Act
            var favorites = await _repository.GetFavorites(userId, CancellationToken.None);

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
            var group = await _repository.GetFavorites(5, CancellationToken.None);

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
    }
}
