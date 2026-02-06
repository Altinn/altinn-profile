using System;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Authorization.ServiceDefaults.Leases;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Core.Leases;

public class OwnedLeaseTests
{
    private readonly Mock<ILeaseProvider> _leaseProviderMock = new();
    private readonly Mock<ILogger<OwnedLease>> _loggerMock = new();
    private readonly Mock<ITimer> _timerMock = new();

    private class TestTimeProvider : TimeProvider
    {
        private readonly ITimer _timer;

        public TestTimeProvider(ITimer timer) => _timer = timer;

        public override ITimer CreateTimer(TimerCallback callback, object state, TimeSpan dueTime, TimeSpan period)
            => _timer;

        public override DateTimeOffset GetUtcNow() => DateTimeOffset.UtcNow;
    }

    private OwnedLease CreateOwnedLease(
        LeaseTicket ticket = null,
        CancellationToken? token = null)
    {
        ticket ??= new LeaseTicket("lease1", Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(2));
        var timeProvider = new TestTimeProvider(_timerMock.Object);
        return new OwnedLease(
            _leaseProviderMock.Object,
            _loggerMock.Object,
            timeProvider,
            ticket,
            null,
            token ?? TestContext.Current.CancellationToken);
    }

    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange
        var ticket = new LeaseTicket("lease1", Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(2));

        // Act
        var lease = CreateOwnedLease(ticket);

        // Assert
        Assert.Equal(ticket.LeaseId, lease.LeaseId);
        Assert.Equal(ticket.Token, lease.LeaseToken);
        Assert.Equal(ticket.Expires, lease.CurrentExpiry);
        Assert.False(lease.Token.IsCancellationRequested);
    }

    [Fact]
    public async Task Tick_RenewsLease_WhenRenewalSucceeds()
    {
        // Arrange
        var ticket = new LeaseTicket("lease2", Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(2));
        var renewedTicket = new LeaseTicket(ticket.LeaseId, ticket.Token, DateTimeOffset.UtcNow.AddMinutes(4));
        _leaseProviderMock
            .Setup(p => p.TryRenewLease(It.IsAny<LeaseTicket>(), OwnedLease.LeaseRenewalInterval, It.IsAny<CancellationToken>()))
            .ReturnsAsync(LeaseAcquireResult.Acquired(renewedTicket, DateTimeOffset.UtcNow, null));

        var lease = CreateOwnedLease(ticket);

        // Act
        // Simulate timer callback
        lease.Tick();

        await lease.TickTask; // Wait for renewal

        // Assert
        Assert.Equal(renewedTicket.Expires, lease.CurrentExpiry);
    }

    [Fact]
    public async Task Tick_CancelsToken_WhenRenewalFails()
    {
        // Arrange
        var ticket = new LeaseTicket("lease3", Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(2));
        _leaseProviderMock
            .Setup(p => p.TryRenewLease(It.IsAny<LeaseTicket>(), OwnedLease.LeaseRenewalInterval, It.IsAny<CancellationToken>()))
            .ReturnsAsync(LeaseAcquireResult.Failed(DateTimeOffset.UtcNow.AddMinutes(2), null, null));

        var lease = CreateOwnedLease(ticket);

        // Act
        lease.Tick();

        await lease.TickTask;

        // Assert
        Assert.True(lease.Token.IsCancellationRequested);
    }

    [Fact]
    public async Task Release_CallsDisposeAndReturnsReleaseResult()
    {
        // Arrange
        var ticket = new LeaseTicket("lease4", Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(2));
        var releaseResult = new LeaseReleaseResult
        {
            IsReleased = true,
            Expires = ticket.Expires,
            LastAcquiredAt = DateTimeOffset.UtcNow,
            LastReleasedAt = DateTimeOffset.UtcNow
        };
        _leaseProviderMock
            .Setup(p => p.ReleaseLease(ticket, It.IsAny<CancellationToken>()))
            .ReturnsAsync(releaseResult);

        var lease = CreateOwnedLease(ticket);

        // Act
        var result = await lease.Release(TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsReleased);
        Assert.Equal(ticket.Expires, result.Expires);
    }

    [Fact]
    public async Task DisposeAsync_CallsReleaseLeaseAndDisposesTimer()
    {
        // Arrange
        var ticket = new LeaseTicket("lease5", Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(2));
        var releaseResult = new LeaseReleaseResult
        {
            IsReleased = true,
            Expires = ticket.Expires,
            LastAcquiredAt = DateTimeOffset.UtcNow,
            LastReleasedAt = DateTimeOffset.UtcNow
        };
        _leaseProviderMock
            .Setup(p => p.ReleaseLease(ticket, It.IsAny<CancellationToken>()))
            .ReturnsAsync(releaseResult);

        var lease = CreateOwnedLease(ticket);

        // Act
        await lease.DisposeAsync();

        // Assert
        _timerMock.Verify(t => t.DisposeAsync(), Times.Once);
        _leaseProviderMock.Verify(p => p.ReleaseLease(ticket, It.IsAny<CancellationToken>()), Times.Once);
    }
}
