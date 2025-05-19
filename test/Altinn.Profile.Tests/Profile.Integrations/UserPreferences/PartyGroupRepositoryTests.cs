using System;
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

        [Fact]
        public async Task GetGroups_WhenUserHAsMultipleGroups_ReturnsAll()
        {
            // Arrange
            _databaseContext.Groups.AddRange(
                new Group { Name = "Group A", GroupId = 1, IsFavorite = true, UserId = 1 },
                new Group { Name = "Group B", GroupId = 2, IsFavorite = false, UserId = 1 },
                new Group { Name = "Group C", GroupId = 3, IsFavorite = false, UserId = 2 });

            await _databaseContext.SaveChangesAsync();

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
            _databaseContext.Groups.AddRange(
                new Group { Name = "Group A", GroupId = 1, IsFavorite = true, UserId = 1 },
                new Group { Name = "Group B", GroupId = 2, IsFavorite = false, UserId = 1 },
                new Group { Name = "Group C", GroupId = 3, IsFavorite = false, UserId = 2 });

            await _databaseContext.SaveChangesAsync();

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
            _databaseContext.Groups.AddRange(
                new Group { Name = "Group A", GroupId = 1, IsFavorite = true, UserId = 1 },
                new Group { Name = "Group B", GroupId = 2, IsFavorite = false, UserId = 1 },
                new Group { Name = "Group C", GroupId = 3, IsFavorite = false, UserId = 2 });

            await _databaseContext.SaveChangesAsync();

            // Act
            var groups = await _repository.GetGroups(5, false, CancellationToken.None);

            // Assert
            Assert.Empty(groups);
        }

        [Fact]
        public async Task GetFavorites_WhenUserHasMultipleGroups_ReturnsOnlyFavorite()
        {
            // Arrange
            _databaseContext.Groups.AddRange(
                new Group { Name = "Group A", GroupId = 1, IsFavorite = true, UserId = 1, Parties = [
                    new PartyGroupAssociation { PartyId = 1, AssociationId = 1, Created = DateTime.Now, GroupId = 1 },
                    new PartyGroupAssociation { PartyId = 2, AssociationId = 2, Created = DateTime.Now, GroupId = 1 }] },
                new Group { Name = "Group B", GroupId = 2, IsFavorite = false, UserId = 1 },
                new Group { Name = "Group C", GroupId = 3, IsFavorite = false, UserId = 2 });

            await _databaseContext.SaveChangesAsync();

            // Act
            var favorites = await _repository.GetFavorites(1, CancellationToken.None);

            // Assert
            Assert.Equal("Group A", favorites.Name);
            Assert.Equal(1, favorites.GroupId);
            Assert.True(favorites.IsFavorite);
            Assert.Equal(1, favorites.UserId);
            Assert.NotNull(favorites.Parties);
            Assert.Equal(2, favorites.Parties.Count);
            Assert.Equal(1, favorites.Parties[0].PartyId);
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
    }
}
