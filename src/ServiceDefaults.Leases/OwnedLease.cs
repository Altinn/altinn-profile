#nullable enable

using System.Diagnostics;
using System.Runtime.CompilerServices;

using Microsoft.Extensions.Logging;

[assembly: InternalsVisibleTo("Altinn.Profile.Tests")]

namespace Altinn.Authorization.ServiceDefaults.Leases;

/// <summary>
/// An owned lease that auto-renews and releases the lease when disposed.
/// </summary>
public sealed partial class OwnedLease
    : IAsyncDisposable
{
    /// <summary>
    /// How often the lease should be renewed.
    /// </summary>
    public static readonly TimeSpan LeaseRenewalInterval = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan RenewalSafetyMargin = TimeSpan.FromSeconds(5);

    private static readonly TimerCallback _timerCallback = static (object? state) =>
    {
        Debug.Assert(state is TimerState, $"Expected {typeof(TimerState)}, got {state}");
        ((TimerState)state!).Tick();
    };

    private readonly Lock _lock = new();
    private readonly ILeaseProvider _leaseProvider;
    private readonly ILogger<OwnedLease> _logger;
    private readonly TimeProvider _timeProvider;
    private readonly CancellationTokenSource _cts;
    private readonly StackTrace? _source;
    private readonly ITimer _timer;

    private Task? _disposed = null;
    private LeaseTicket _ticket;
    private LeaseReleaseResult? _releaseResult;

    // For testing purposes
    private Task _tickTask = Task.CompletedTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="OwnedLease"/> class.
    /// </summary>
    public OwnedLease(
        ILeaseProvider leaseProvider,
        ILogger<OwnedLease> logger,
        TimeProvider timeProvider,
        LeaseTicket ticket,
        StackTrace? source,
        CancellationToken outerScopeToken)
    {
        _leaseProvider = leaseProvider;
        _logger = logger;
        _timeProvider = timeProvider;
        _ticket = ticket;
        _source = source;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(outerScopeToken);

        // creates the timer in an inert state
        var state = new TimerState(this);
        _timer = timeProvider.CreateTimer(
            callback: _timerCallback,
            state: state,
            dueTime: Timeout.InfiniteTimeSpan,
            period: Timeout.InfiniteTimeSpan);
        state.SetTimer(_timer);

        UpdateTimer();
    }

#if DEBUG
    /// <summary>
    /// Destructor.
    /// </summary>
    ~OwnedLease()
    {
        if (!IsDisposed)
        {
            Log.LeaseNotDisposed(_logger, _ticket.LeaseId, _source);
        }
    }
#endif

    /// <summary>
    /// Gets a <see cref="CancellationToken"/> that is triggered when the lease is disposed or expires.
    /// </summary>
    public CancellationToken Token
        => _cts.Token;

    /// <summary>
    /// Gets the lease id.
    /// </summary>
    public string LeaseId
        => _ticket.LeaseId;

    /// <summary>
    /// Gets the current lease token.
    /// </summary>
    public Guid LeaseToken
        => Volatile.Read(ref _ticket).Token;

    /// <summary>
    /// Gets the task that represents the lease renewal.
    /// Only used for testing purposes.
    /// </summary>
    internal Task TickTask
        => Volatile.Read(ref _tickTask);

    /// <summary>
    /// Gets the current expiry of the lease.
    /// </summary>
    internal DateTimeOffset CurrentExpiry
        => Volatile.Read(ref _ticket).Expires;

    private bool IsDisposed
        => Volatile.Read(ref _disposed) is not null;

    /// <summary>
    /// Renews the lease if it is still active, or cancels the lease if renewal fails.
    /// </summary>
    internal void Tick()
    {
        if (IsDisposed || Token.IsCancellationRequested)
        {
            return;
        }

        Volatile.Write(ref _tickTask, Task.Run(UpdateLease, Token));

        async Task UpdateLease()
        {
            var result = await _leaseProvider.TryRenewLease(_ticket, LeaseRenewalInterval, Token);

            if (!result.IsLeaseAcquired)
            {
                // failed to renew the lease - it's likely that the lease has been lost
                // either due to time drift or network latency - signal using the cancellation token
                await _cts.CancelAsync();
                return;
            }

            Volatile.Write(ref _ticket, result.Lease);
            UpdateTimer();
        }
    }

    private void UpdateTimer()
    {
        var ticket = Volatile.Read(ref _ticket);
        var expiry = ticket.Expires;
        var now = _timeProvider.GetUtcNow();
        var duration = (expiry - now) - RenewalSafetyMargin;

        if (duration <= TimeSpan.Zero)
        {
            _timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            Tick();
            return;
        }

        _timer.Change(duration, Timeout.InfiniteTimeSpan);
    }

    /// <summary>
    /// Releases the lease, disposes the <see cref="OwnedLease"/>, and returns the <see cref="LeaseReleaseResult"/>
    /// from the release operation.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
    /// <returns>The <see cref="LeaseReleaseResult"/> from the release operation.</returns>
    public ValueTask<LeaseReleaseResult> Release(CancellationToken cancellationToken = default)
    {
        if (_releaseResult is { } result)
        {
            return new(result);
        }

        return ReleaseInner(this, cancellationToken);

        static async ValueTask<LeaseReleaseResult> ReleaseInner(OwnedLease lease, CancellationToken cancellationToken)
        {
            await lease.DisposeAsync().AsTask().WaitAsync(cancellationToken);
            
            // After disposal, _releaseResult should be set
            if (lease._releaseResult is { } releaseResult)
            {
                return releaseResult;
            }

            throw new InvalidOperationException("Release result was not set after disposal");
        }
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
#if DEBUG
        GC.SuppressFinalize(this);
#endif

        Task? result;
        lock (_lock)
        {
            result = _disposed;
            if (result is null)
            {
                _disposed = result = DisposeInner(this);
            }
        }

        return new(result);

        static async Task DisposeInner(OwnedLease lease)
        {
            await lease._timer.DisposeAsync();
            await lease._cts.CancelAsync();
            lease._cts.Dispose();

            var ticket = Volatile.Read(ref lease._ticket);
            lease._releaseResult = await lease._leaseProvider.ReleaseLease(ticket, cancellationToken: CancellationToken.None);
        }
    }

    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Warning, "Lease '{LeaseId}' was not disposed. Created at: {Source}")]
        public static partial void LeaseNotDisposed(ILogger logger, string leaseId, StackTrace? source);
    }

    private sealed class TimerState
    {
        private readonly WeakReference<OwnedLease> _lease;
        private ITimer? _timer;

        public TimerState(OwnedLease lease)
        {
            _lease = new(lease);
        }

        public void SetTimer(ITimer timer)
        {
            _timer = timer;
        }

        public void Tick()
        {
            if (_lease.TryGetTarget(out var lease))
            {
                lease.Tick();
            }
            else
            {
                _timer?.Dispose();
            }
        }
    }
}
