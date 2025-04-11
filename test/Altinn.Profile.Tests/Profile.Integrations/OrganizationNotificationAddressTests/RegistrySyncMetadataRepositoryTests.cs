using System;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.OrganizationNotificationAddressTests;

/// <summary>
/// Contains unit tests for the <see cref="RegistrySyncMetadataRepository"/> class.
/// </summary>
public class RegistrySyncMetadataRepositoryTests : IDisposable
{
    private bool _isDisposed;
    private readonly ProfileDbContext _databaseContext;
    private readonly RegistrySyncMetadataRepository _repository;
    private readonly Mock<IDbContextFactory<ProfileDbContext>> _databaseContextFactory;

    public RegistrySyncMetadataRepositoryTests()
    {
        var databaseContextOptions = new DbContextOptionsBuilder<ProfileDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _databaseContextFactory = new Mock<IDbContextFactory<ProfileDbContext>>();

        _databaseContextFactory.Setup(f => f.CreateDbContext())
            .Returns(new ProfileDbContext(databaseContextOptions));

        _databaseContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ProfileDbContext(databaseContextOptions));

        _repository = new RegistrySyncMetadataRepository(_databaseContextFactory.Object);

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
    public async Task UpdateLatestChangeTimestampAsync_ReturnsUpdatedTime()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var oldTime = await _repository.GetLatestSyncTimestampAsync();

        // Act
        var updatedTime = await _repository.UpdateLatestChangeTimestampAsync(timestamp);

        // Assert
        Assert.Equal(timestamp, updatedTime);
        Assert.NotEqual(timestamp, oldTime);
    }

    [Fact]
    public async Task GetLatestSyncTimestampAsync_WhenNoEntries_ReturnsNull()
    {
        // Act
        var timestamp = await _repository.GetLatestSyncTimestampAsync();

        // Assert
        Assert.Null(timestamp);
    }

    [Fact]
    public async Task GetLatestSyncTimestampAsync_ReturnsUpdatedTime()
    {
        // Arrange
        var timestamp = DateTime.UtcNow;
        var oldTime = await _repository.GetLatestSyncTimestampAsync();

        // Act
        await _repository.UpdateLatestChangeTimestampAsync(timestamp);
        var updatedTime = await _repository.GetLatestSyncTimestampAsync();

        // Assert
        Assert.Equal(timestamp, updatedTime);
        Assert.NotEqual(timestamp, oldTime);
    }
}
