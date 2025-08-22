using System;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Authorization.ServiceDefaults.Leases;

using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Core.Leases;

public class LeaseManagerTests
{
    private readonly Mock<ILeaseProvider> _leaseProviderMock = new();
    private readonly Mock<IServiceProvider> _serviceProviderMock = new();

    private LeaseManager CreateManager()
    {
        return new LeaseManager(_leaseProviderMock.Object, _serviceProviderMock.Object);
    }

    [Fact]
    public async Task AcquireLease_ShouldReturnLease_WhenLeaseIsAcquired()
    {
        // Arrange
        var leaseId = "lease1";
        var token = Guid.NewGuid();
        var expires = DateTimeOffset.UtcNow.AddMinutes(2);
        var ticket = new LeaseTicket(leaseId, token, expires);
        var acquireResult = LeaseAcquireResult.Acquired(ticket, DateTimeOffset.UtcNow, null);

        _leaseProviderMock
            .Setup(p => p.TryAcquireLease(
                leaseId,
                OwnedLease.LeaseRenewalInterval,
                It.IsAny<Func<LeaseInfo, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(acquireResult);

        // Setup service provider to return required services for OwnedLease constructor
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ILogger<OwnedLease>)))
            .Returns(Mock.Of<ILogger<OwnedLease>>());
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(TimeProvider)))
            .Returns(TimeProvider.System);

        // Act
        var manager = CreateManager();
        var lease = await manager.AcquireLease(leaseId, cancellationToken: CancellationToken.None);

        // Assert
        Assert.NotNull(lease);
        Assert.Equal(leaseId, lease.LeaseId);
        Assert.True(lease.Acquired);
        Assert.Equal(expires, lease.Expires);
    }

    [Fact]
    public async Task AcquireLease_ShouldReturnLeaseWithNullOwnedLease_WhenLeaseNotAcquired()
    {
        // Arrange
        var leaseId = "lease2";
        var expires = DateTimeOffset.UtcNow.AddMinutes(2);
        var acquireResult = LeaseAcquireResult.Failed(expires, null, null);

        _leaseProviderMock
            .Setup(p => p.TryAcquireLease(
                leaseId,
                OwnedLease.LeaseRenewalInterval,
                It.IsAny<Func<LeaseInfo, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(acquireResult);

        // Act
        var manager = CreateManager();
        var lease = await manager.AcquireLease(leaseId, cancellationToken: CancellationToken.None);

        // Assert
        Assert.NotNull(lease);
        Assert.Equal(leaseId, lease.LeaseId);
        Assert.False(lease.Acquired);
        Assert.Equal(expires, lease.Expires);
    }

    [Fact]
    public async Task AcquireLease_ShouldReleaseLeaseOnException()
    {
        // Arrange
        var leaseId = "lease3";
        var token = Guid.NewGuid();
        var expires = DateTimeOffset.UtcNow.AddMinutes(2);
        var ticket = new LeaseTicket(leaseId, token, expires);
        var acquireResult = LeaseAcquireResult.Acquired(ticket, DateTimeOffset.UtcNow, null);

        _leaseProviderMock
            .Setup(p => p.TryAcquireLease(
                leaseId,
                OwnedLease.LeaseRenewalInterval,
                It.IsAny<Func<LeaseInfo, bool>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(acquireResult);

        // Simulate exception in factory (OwnedLease constructor dependencies)
        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(ILogger<OwnedLease>)))
            .Throws(new InvalidOperationException("Factory failure"));

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(TimeProvider)))
            .Returns(TimeProvider.System);

        _leaseProviderMock
            .Setup(p => p.ReleaseLease(ticket, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LeaseReleaseResult
            {
                IsReleased = true,
                Expires = expires,
                LastAcquiredAt = DateTimeOffset.UtcNow,
                LastReleasedAt = DateTimeOffset.UtcNow
            });

        var manager = CreateManager();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => manager.AcquireLease(leaseId, cancellationToken: CancellationToken.None));
        _leaseProviderMock.Verify(p => p.ReleaseLease(ticket, It.IsAny<CancellationToken>()), Times.Once);
    }
}
