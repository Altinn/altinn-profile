#nullable enable

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ServiceDefaults.Leases;

/// <summary>
/// Result from calling <see cref="LeaseManager.AcquireLease(string, CancellationToken)"/>, either a
/// acquired lease or a failed lease-acquire result. Check <see cref="Acquired"/> to see if the lease
/// was acquired or not.
/// </summary>
public sealed class Lease
    : IAsyncDisposable
{
    private static readonly CancellationToken CancelledTokenSingleton = CreateCancelledToken();

    private static CancellationToken CreateCancelledToken()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        return cts.Token;
    }

    private readonly string _leaseId;
    private readonly CancellationToken _cancellationToken;
    private readonly DateTimeOffset _expires;
    private readonly DateTimeOffset? _lastAcquiredAt;
    private readonly DateTimeOffset? _lastReleasedAt;
    private readonly OwnedLease? _lease;

    /// <summary>
    /// Initializes a new instance of the <see cref="Lease"/> class.
    /// </summary>
    /// <param name="leaseId">The lease id.</param>
    /// <param name="lease">The <see cref="OwnedLease"/> if a lease is held.</param>
    /// <param name="expires">When the lease expires.</param>
    /// <param name="lastAcquiredAt">When the lease was last acquired.</param>
    /// <param name="lastReleasedAt">When the lease was last released.</param>
    internal Lease(
        string leaseId,
        OwnedLease? lease,
        DateTimeOffset expires,
        DateTimeOffset? lastAcquiredAt,
        DateTimeOffset? lastReleasedAt)
    {
        if (lease is not null)
        {
            Debug.Assert(leaseId == lease.LeaseId, $"Expected {leaseId}, got {lease.LeaseId}");
        }

        _leaseId = leaseId;
        _lease = lease;
        _expires = expires;
        _lastAcquiredAt = lastAcquiredAt;
        _lastReleasedAt = lastReleasedAt;
        _cancellationToken = lease?.Token ?? CancelledTokenSingleton;
    }

    /// <summary>
    /// Gets the lease id.
    /// </summary>
    public string LeaseId
        => _leaseId;

    /// <summary>
    /// Gets a value indicating whether or not the lease was acquired.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Inner))]
    public bool Acquired
        => _lease is not null;

    /// <summary>
    /// Gets the expiry of the current lease.
    /// </summary>
    public DateTimeOffset Expires
        => _lease?.CurrentExpiry ?? _expires;

    /// <summary>
    /// Gets a <see cref="CancellationToken"/> that is valid for as long as the lease is held.
    /// </summary>
    public CancellationToken Token
        => _cancellationToken;

    /// <summary>
    /// Gets the owned lease.
    /// </summary>
    internal OwnedLease? Inner
        => _lease;

    /// <summary>
    /// Gets when the lease was last acquired.
    /// </summary>
    public DateTimeOffset? LastAcquiredAt
        => _lastAcquiredAt;

    /// <summary>
    /// Gets when the lease was last released.
    /// </summary>
    public DateTimeOffset? LastReleasedAt
        => _lastReleasedAt;

    /// <summary>
    /// Releases the lease (if one is held).
    /// </summary>
    /// <remarks>
    /// If the lease has already been released, this returns a cached result.
    /// </remarks>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="LeaseReleaseResult"/> if a lease was held, otherwise <see langword="null"/>.</returns>
    public ValueTask<LeaseReleaseResult?> Release(CancellationToken cancellationToken = default)
    {
        if (_lease is { } lease)
        {
            return lease.Release(cancellationToken)!;
        }

        return default;
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
        => _lease?.DisposeAsync() ?? ValueTask.CompletedTask;
}
