using System;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Authorization.ServiceDefaults.Leases;
using Altinn.Profile.Integrations.Leases;
using Altinn.Profile.Integrations.Repositories.A2Sync;

using Humanizer.Localisation;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

using Lease = Altinn.Profile.Integrations.Leases.Lease;

namespace Altinn.Profile.Tests.Profile.Integrations.Leases;

public class PostgresqlLeaseProviderTests
{
    private readonly Mock<ILeaseRepository> _leaseRepositoryMock = new();
    private readonly Mock<ILogger<PostgresqlLeaseProvider>> _loggerMock = new();
    private readonly TimeProvider _timeProvider = TimeProvider.System;

    private PostgresqlLeaseProvider CreateProvider()
    {
        return new PostgresqlLeaseProvider(_timeProvider, _loggerMock.Object, _leaseRepositoryMock.Object);
    }

    [Fact]
    public async Task TryAcquireLease_ShouldReturnAcquired_WhenRepositoryReturnsAcquired()
    {
        // Arrange
        var provider = CreateProvider();
        var leaseId = "lease1";
        var duration = TimeSpan.FromSeconds(10);
        var now = _timeProvider.GetUtcNow();
        var token = Guid.NewGuid();
        var ticket = new LeaseTicket(leaseId, token, now + duration);
        var expectedResult = LeaseAcquireResult.Acquired(ticket, now, now);

        _leaseRepositoryMock
            .Setup(r => r.UpsertLease(It.IsAny<Lease>(), It.IsAny<DateTimeOffset>(), It.IsAny<Func<LeaseInfo, bool>>(), It.IsAny<CancellationToken>()))
            .Callback<Lease, DateTimeOffset, Func<LeaseInfo, bool>, CancellationToken>((lease, now, filter, ct) =>
            {
                Assert.Equal(leaseId, lease.Id);
                Assert.Equal(now + duration, lease.Expires);
                Assert.NotNull(lease.Acquired);
                Assert.Null(lease.Released);
            })
            .ReturnsAsync(expectedResult);

        // Act
        var result = await provider.TryAcquireLease(leaseId, duration);

        // Assert
        Assert.True(result.IsLeaseAcquired);
        Assert.Equal(ticket, result.Lease);
    }

    [Fact]
    public async Task TryAcquireLease_ShouldReturnFailed_WhenRepositoryReturnsFailed()
    {
        // Arrange
        var provider = CreateProvider();
        var leaseId = "lease2";
        var duration = TimeSpan.FromSeconds(10);
        var now = _timeProvider.GetUtcNow();
        var expectedResult = LeaseAcquireResult.Failed(now + duration, now, now);

        _leaseRepositoryMock
            .Setup(r => r.UpsertLease(It.IsAny<Lease>(), It.IsAny<DateTimeOffset>(), It.IsAny<Func<LeaseInfo, bool>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await provider.TryAcquireLease(leaseId, duration);

        // Assert
        Assert.False(result.IsLeaseAcquired);
        Assert.Null(result.Lease);
    }

    [Fact]
    public async Task TryRenewLease_ShouldReturnAcquired_WhenRepositoryReturnsAcquired()
    {
        // Arrange
        var provider = CreateProvider();
        var leaseId = "lease3";
        var duration = TimeSpan.FromSeconds(10);
        var now = _timeProvider.GetUtcNow();
        var ticket = new LeaseTicket(leaseId, Guid.NewGuid(), now + duration);
        var expectedResult = LeaseAcquireResult.Acquired(ticket, now, now);

        _leaseRepositoryMock
            .Setup(r => r.UpsertLease(It.IsAny<Lease>(), It.IsAny<DateTimeOffset>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await provider.TryRenewLease(ticket, duration);

        // Assert
        Assert.True(result.IsLeaseAcquired);
        Assert.Equal(ticket, result.Lease);
    }

    [Fact]
    public async Task ReleaseLease_ShouldReturnReleased_WhenRepositoryReturnsAcquired()
    {
        // Arrange
        var provider = CreateProvider();
        var leaseId = "lease4";
        var now = _timeProvider.GetUtcNow();
        var ticket = new LeaseTicket(leaseId, Guid.NewGuid(), now.AddSeconds(10));
        var acquireResult = LeaseAcquireResult.Acquired(ticket, now, now);

        _leaseRepositoryMock
            .Setup(r => r.UpsertLease(It.IsAny<Lease>(), It.IsAny<DateTimeOffset>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(acquireResult);

        // Act
        var result = await provider.ReleaseLease(ticket);

        // Assert
        Assert.True(result.IsReleased);
        Assert.Equal(acquireResult.Expires, result.Expires);
    }

    [Fact]
    public async Task ReleaseLease_ShouldReturnNotReleased_WhenRepositoryReturnsFailed()
    {
        // Arrange
        var provider = CreateProvider();
        var leaseId = "lease5";
        var now = _timeProvider.GetUtcNow();
        var ticket = new LeaseTicket(leaseId, Guid.NewGuid(), now.AddSeconds(10));
        var failedResult = LeaseAcquireResult.Failed(now, now, now);

        _leaseRepositoryMock
            .Setup(r => r.UpsertLease(It.IsAny<Lease>(), It.IsAny<DateTimeOffset>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedResult);

        // Act
        var result = await provider.ReleaseLease(ticket);

        // Assert
        Assert.False(result.IsReleased);
    }
}
