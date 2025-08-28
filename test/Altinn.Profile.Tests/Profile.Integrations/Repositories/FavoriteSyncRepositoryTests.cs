using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Core.PartyGroups;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Repositories;

public class FavoriteSyncRepositoryTests : IDisposable
{
    private bool _isDisposed;
    private readonly ProfileDbContext _databaseContext;
    private readonly FavoriteSyncRepository _repository;
    private readonly Mock<IDbContextFactory<ProfileDbContext>> _databaseContextFactory;

    public FavoriteSyncRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ProfileDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _databaseContext = new ProfileDbContext(options);

        _databaseContextFactory = new Mock<IDbContextFactory<ProfileDbContext>>();

        _databaseContextFactory.Setup(f => f.CreateDbContext())
            .Returns(new ProfileDbContext(options));

        _databaseContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ProfileDbContext(options));

        _repository = new FavoriteSyncRepository(_databaseContextFactory.Object);

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

    private static IDbContextFactory<ProfileDbContext> CreateDbContextFactory(ProfileDbContext context)
    {
        var factory = new Mock<IDbContextFactory<ProfileDbContext>>();
        factory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(context);
        return factory.Object;
    }

    [Fact]
    public async Task AddPartyToFavorites_AddsToExistingGroup()
    {
        // Arrange
        var userId = 1;
        var partyUuid = Guid.NewGuid();
        var created = DateTime.UtcNow;

        var group = new Group
        {
            GroupId = 1,
            UserId = userId,
            IsFavorite = true,
            Name = "Favorites",
            Parties = new List<PartyGroupAssociation>()
        };

        _databaseContext.Groups.Add(group);
        await _databaseContext.SaveChangesAsync();

        // Act
        await _repository.AddPartyToFavorites(userId, partyUuid, created, CancellationToken.None);

        // Assert
        var updatedGroup = await _databaseContext.Groups.Include(g => g.Parties).FirstAsync();
        Assert.Single(updatedGroup.Parties);
        Assert.Equal(partyUuid, updatedGroup.Parties[0].PartyUuid);
    }

    [Fact]
    public async Task AddPartyToFavorites_CreatesGroupIfNotExists()
    {
        // Arrange
        var userId = 2;
        var partyUuid = Guid.NewGuid();
        var created = DateTime.UtcNow;

        // Act
        await _repository.AddPartyToFavorites(userId, partyUuid, created, CancellationToken.None);

        // Assert
        var group = await _databaseContext.Groups.Include(g => g.Parties).FirstOrDefaultAsync(g => g.UserId == userId && g.IsFavorite);
        Assert.NotNull(group);
        Assert.Single(group.Parties);
        Assert.Equal(partyUuid, group.Parties[0].PartyUuid);
    }

    [Fact]
    public async Task AddPartyToFavorites_DoesNotAddDuplicate()
    {
        // Arrange
        var userId = 3;
        var partyUuid = Guid.NewGuid();
        var created = DateTime.UtcNow;

        var group = new Group
        {
            GroupId = 1,
            UserId = userId,
            IsFavorite = true,
            Name = "Favorites",
            Parties = new List<PartyGroupAssociation>
            {
                new PartyGroupAssociation { PartyUuid = partyUuid, Created = created }
            }
        };

        _databaseContext.Groups.Add(group);
        await _databaseContext.SaveChangesAsync();

        // Act
        await _repository.AddPartyToFavorites(userId, partyUuid, created, CancellationToken.None);

        // Assert
        var updatedGroup = await _databaseContext.Groups.Include(g => g.Parties).FirstAsync();
        Assert.Single(updatedGroup.Parties);
    }

    [Fact]
    public async Task DeleteFromFavorites_RemovesParty()
    {
        // Arrange
        var userId = 4;
        var partyUuid = Guid.NewGuid();
        var created = DateTime.UtcNow;

        var group = new Group
        {
            GroupId = 1,
            UserId = userId,
            IsFavorite = true,
            Name = "Favorites",
            Parties = new List<PartyGroupAssociation>
            {
                new PartyGroupAssociation { PartyUuid = partyUuid, Created = created }
            }
        };

        _databaseContext.Groups.Add(group);
        await _databaseContext.SaveChangesAsync();

        // Act
        await _repository.DeleteFromFavorites(userId, partyUuid, CancellationToken.None);

        // Assert
        var updatedGroup = await _repository.GetFavorites(userId, CancellationToken.None);
        Assert.Empty(updatedGroup.Parties);
    }

    [Fact]
    public async Task DeleteFromFavorites_IfGroupNotFound_ShouldNotThrow()
    {
        // Arrange
        var userId = 5;
        var partyUuid = Guid.NewGuid();

        // Act & Assert (should not throw)
        try
        {
            await _repository.DeleteFromFavorites(userId, partyUuid, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Assert.Fail("Expected no exception, but got: " + ex.Message);
        }
    }

    [Fact]
    public async Task DeleteFromFavorites_IfPartyNotFound_ShuldNotThrow()
    {
        // Arrange
        var userId = 6;
        var partyUuid = Guid.NewGuid();

        var group = new Group
        {
            GroupId = 1,
            UserId = userId,
            IsFavorite = true,
            Name = "Favorites",
            Parties = new List<PartyGroupAssociation>()
        };

        _databaseContext.Groups.Add(group);
        await _databaseContext.SaveChangesAsync();

        // Act & Assert (should not throw)
        try
        {
            await _repository.DeleteFromFavorites(userId, partyUuid, CancellationToken.None);
        }
        catch (Exception ex)
        {
            Assert.Fail("Expected no exception, but got: " + ex.Message);
        }
    }
}
