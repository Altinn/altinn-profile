using System;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Integrations.SblBridge.Changelog;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Repositories;

/// <summary>
/// Contains unit tests for the <see cref="ChangelogSyncMetadataRepository"/> class.
/// </summary>
public class ChangelogSyncMetadataRepositoryTests : IDisposable
{
    private bool _isDisposed;
    private readonly ProfileDbContext _databaseContext;
    private readonly ChangelogSyncMetadataRepository _repository;
    private readonly Mock<IDbContextFactory<ProfileDbContext>> _databaseContextFactory;

    public ChangelogSyncMetadataRepositoryTests()
    {
        var databaseContextOptions = new DbContextOptionsBuilder<ProfileDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _databaseContextFactory = new Mock<IDbContextFactory<ProfileDbContext>>();

        _databaseContextFactory.Setup(f => f.CreateDbContext())
            .Returns(new ProfileDbContext(databaseContextOptions));

        _databaseContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ProfileDbContext(databaseContextOptions));

        _repository = new ChangelogSyncMetadataRepository(_databaseContextFactory.Object);

        _databaseContext = _databaseContextFactory.Object.CreateDbContext();

        _databaseContext.SaveChanges();
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
    public async Task GetLatestSyncTimestampAsync_WhenNoEntries_ReturnsNull()
    {
        // Act
        var timestamp = await _repository.GetLatestSyncTimestampAsync(DataType.Favorites, CancellationToken.None);

        // Assert
        Assert.Null(timestamp);
    }

    [Fact]
    public async Task UpdateLatestChangeTimestampAsync_AddsAndReturnsUpdatedTime()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var dataType = DataType.Favorites;
        var oldTime = await _repository.GetLatestSyncTimestampAsync(dataType, CancellationToken.None);

        // Act
        var updatedTime = await _repository.UpdateLatestChangeTimestampAsync(timestamp, dataType);

        // Assert
        Assert.Equal(timestamp, updatedTime);
        Assert.NotEqual(timestamp, oldTime);
    }
}
