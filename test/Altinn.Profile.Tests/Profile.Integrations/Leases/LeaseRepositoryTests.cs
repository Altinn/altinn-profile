using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using Moq;

using Xunit;

using Lease = Altinn.Profile.Integrations.Leases.Lease;

namespace Altinn.Profile.Tests.Profile.Integrations.Leases;

public class LeaseRepositoryTests
{
    private static DbContextOptions<ProfileDbContext> CreateInMemoryOptions()
        => new DbContextOptionsBuilder<ProfileDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .Options;

    private static LeaseRepository CreateRepository(ProfileDbContext db)
    {
        var factoryMock = new Mock<IDbContextFactory<ProfileDbContext>>();
        factoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(db);
        return new LeaseRepository(factoryMock.Object);
    }

    [Fact]
    public async Task UpsertLease_AddsNewLease_WhenNotExists()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        await using var db = new ProfileDbContext(options);
        var repo = CreateRepository(db);

        var lease = new Lease
        {
            Id = "lease1",
            Token = Guid.NewGuid(),
            Expires = DateTimeOffset.UtcNow.AddMinutes(1),
            Acquired = DateTimeOffset.UtcNow,
            Released = null
        };

        // Act
        var result = await repo.UpsertLease(lease, DateTimeOffset.UtcNow, null, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsLeaseAcquired);
        Assert.NotNull(result.Lease);
        Assert.Equal(lease.Id, result.Lease.LeaseId);
    }

    [Fact]
    public async Task UpsertLease_UpdatesExistingLease_WhenTokenMatches()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        await using var db = new ProfileDbContext(options);
        var repo = CreateRepository(db);

        var lease = new Lease
        {
            Id = "lease2",
            Token = Guid.NewGuid(),
            Expires = DateTimeOffset.UtcNow.AddMinutes(1),
            Acquired = DateTimeOffset.UtcNow,
            Released = null
        };
        db.Lease.Add(lease);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var updatedLease = new Lease
        {
            Id = lease.Id,
            Token = lease.Token, // same token
            Expires = DateTimeOffset.UtcNow.AddMinutes(2),
            Acquired = DateTimeOffset.UtcNow,
            Released = null
        };

        // Act
        var result = await repo.UpsertLease(updatedLease, DateTimeOffset.UtcNow, null, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsLeaseAcquired);
        Assert.NotNull(result.Lease);
        Assert.Equal(updatedLease.Expires, result.Lease.Expires);
    }

    [Fact]
    public async Task UpsertLease_UpdatesExistingLease_WhenExpired()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        await using var db = new ProfileDbContext(options);
        var repo = CreateRepository(db);

        var lease = new Lease
        {
            Id = "lease3",
            Token = Guid.NewGuid(),
            Expires = DateTimeOffset.UtcNow.AddMinutes(-1), // expired
            Acquired = DateTimeOffset.UtcNow.AddMinutes(-2),
            Released = null
        };
        db.Lease.Add(lease);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var newToken = Guid.NewGuid();
        var updatedLease = new Lease
        {
            Id = lease.Id,
            Token = newToken,
            Expires = DateTimeOffset.UtcNow.AddMinutes(5),
            Acquired = DateTimeOffset.UtcNow,
            Released = null
        };

        // Act
        var result = await repo.UpsertLease(updatedLease, DateTimeOffset.UtcNow, null, TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsLeaseAcquired);
        Assert.NotNull(result.Lease);
        Assert.Equal(newToken, result.Lease.Token);
    }

    [Fact]
    public async Task UpsertLease_Fails_WhenLeaseAlreadyExists()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        await using var db = new ProfileDbContext(options);
        var repo = CreateRepository(db);

        var lease = new Lease
        {
            Id = "lease3",
            Token = Guid.NewGuid(),
            Expires = DateTimeOffset.UtcNow.AddMinutes(1),
            Acquired = DateTimeOffset.UtcNow.AddMinutes(-2),
            Released = null
        };
        db.Lease.Add(lease);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        var newToken = Guid.NewGuid();
        var updatedLease = new Lease
        {
            Id = lease.Id,
            Token = newToken,
            Expires = DateTimeOffset.UtcNow.AddMinutes(5),
            Acquired = DateTimeOffset.UtcNow,
            Released = null
        };

        // Act
        var result = await repo.UpsertLease(updatedLease, DateTimeOffset.UtcNow, null, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsLeaseAcquired);
        Assert.Null(result.Lease);
    }

    [Fact]
    public async Task UpsertLease_Fails_WhenFilterReturnsFalse()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        await using var db = new ProfileDbContext(options);
        var repo = CreateRepository(db);

        var lease = new Lease
        {
            Id = "lease4",
            Token = Guid.NewGuid(),
            Expires = DateTimeOffset.UtcNow.AddMinutes(1),
            Acquired = DateTimeOffset.UtcNow,
            Released = null
        };

        // Act
        var result = await repo.UpsertLease(lease, DateTimeOffset.UtcNow, info => false, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsLeaseAcquired);
        Assert.Null(result.Lease);
    }

    [Fact]
    public async Task GetFailedLeaseResult_ReturnsFailedResult_WhenLeaseExists()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        await using var db = new ProfileDbContext(options);
        var repo = CreateRepository(db);

        var lease = new Lease
        {
            Id = "lease5",
            Token = Guid.NewGuid(),
            Expires = DateTimeOffset.UtcNow.AddMinutes(1),
            Acquired = DateTimeOffset.UtcNow,
            Released = null
        };
        db.Lease.Add(lease);
        await db.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await repo.GetFailedLeaseResult(lease.Id, TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsLeaseAcquired);
        Assert.Null(result.Lease);
        Assert.Equal(lease.Expires, result.Expires);
    }

    [Fact]
    public async Task GetFailedLeaseResult_Throws_WhenLeaseDoesNotExist()
    {
        // Arrange
        var options = CreateInMemoryOptions();
        await using var db = new ProfileDbContext(options);
        var repo = CreateRepository(db);

        // Act & Assert
        await Assert.ThrowsAsync<UnreachableException>(() =>
            repo.GetFailedLeaseResult("nonexistent", TestContext.Current.CancellationToken));
    }
}
