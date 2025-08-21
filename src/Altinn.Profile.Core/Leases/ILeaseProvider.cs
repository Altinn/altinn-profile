#nullable enable

namespace Altinn.Profile.Core.Leases;

/// <summary>
/// A provider of leases. Leases are a synchronization primitive that can be used to ensure
/// that only one process is performing a certain task at a time.
/// </summary>
public interface ILeaseProvider 
{
    /// <summary>
    /// Attempts to acquire a lease.
    /// </summary>
    /// <param name="leaseId">The lease id.</param>
    /// <param name="duration">
    /// The duration the lease should be valid for. Prefer shorter durations to avoid
    /// leases being held for a very long time if the process crashes.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="LeaseAcquireResult"/>.</returns>
    Task<LeaseAcquireResult> TryAcquireLease(
        string leaseId,
        TimeSpan duration,
        CancellationToken cancellationToken)
        => TryAcquireLease(leaseId, duration, filter: null, cancellationToken);

    /// <summary>
    /// Attempts to acquire a lease.
    /// </summary>
    /// <param name="leaseId">The lease id.</param>
    /// <param name="duration">
    /// The duration the lease should be valid for. Prefer shorter durations to avoid
    /// leases being held for a very long time if the process crashes.
    /// </param>
    /// <param name="filter">A filter that can be used to reject leases based on the state of the lease provider.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="LeaseAcquireResult"/>.</returns>
    Task<LeaseAcquireResult> TryAcquireLease(
        string leaseId,
        TimeSpan duration,
        Func<LeaseInfo, bool>? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to renew a lease.
    /// </summary>
    /// <param name="lease">The <see cref="LeaseTicket"/> of the lease to renew.</param>
    /// <param name="duration">
    /// The duration the lease should be valid for. Prefer shorter durations to avoid
    /// leases being held for a very long time if the process crashes.
    /// </param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="LeaseAcquireResult"/>.</returns>
    Task<LeaseAcquireResult> TryRenewLease(LeaseTicket lease, TimeSpan duration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases a lease.
    /// </summary>
    /// <param name="lease">The <see cref="LeaseTicket"/> of the lease to release.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="LeaseAcquireResult"/>.</returns>
    Task<LeaseReleaseResult> ReleaseLease(LeaseTicket lease, CancellationToken cancellationToken = default);
}
