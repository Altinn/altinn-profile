using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ServiceDefaults.Leases;

/// <summary>
/// A result of attempting to acquire a lease.
/// </summary>
public sealed record LeaseAcquireResult
{
    /// <summary>
    /// Gets a <see cref="LeaseTicket"/>, if the lease was acquired.
    /// </summary>
    public LeaseTicket? Lease { get; }

    /// <summary>
    /// Gets when the lease will expire (regardless of whether it was acquired).
    /// </summary>
    public DateTimeOffset Expires { get; }

    /// <summary>
    /// Gets when the lease was last acquired at.
    /// </summary>
    /// <remarks>
    /// This does not signify that the lease is currently held.
    /// </remarks>
    public DateTimeOffset? LastAcquiredAt { get; }

    /// <summary>
    /// Gets when the lease was last released at.
    /// </summary>
    /// <remarks>
    /// This does not signify that the lease is not currently held.
    /// </remarks>
    public DateTimeOffset? LastReleasedAt { get; }

    /// <summary>
    /// Gets a value indicating whether a lease was acquired.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Lease))]
    public bool IsLeaseAcquired => Lease != null;

    /// <summary>
    /// Initializes a new instance of the <see cref="LeaseAcquireResult"/> class.
    /// </summary>
    private LeaseAcquireResult(
        LeaseTicket? lease, 
        DateTimeOffset expires,
        DateTimeOffset? lastAcquiredAt,
        DateTimeOffset? lastReleasedAt)
    {
        Lease = lease;
        Expires = expires;
        LastAcquiredAt = lastAcquiredAt;
        LastReleasedAt = lastReleasedAt;
    }

    /// <summary>
    /// Creates a <see cref="LeaseAcquireResult"/> representing a lease that was acquired.
    /// </summary>
    /// <param name="ticket">The lease ticket.</param>
    /// <param name="lastAcquiredAt">When the lease was last acquired at.</param>
    /// <param name="lastReleasedAt">When the lease was last released at.</param>
    public static LeaseAcquireResult Acquired(LeaseTicket ticket, DateTimeOffset? lastAcquiredAt, DateTimeOffset? lastReleasedAt)
        => new(ticket, ticket.Expires, lastAcquiredAt, lastReleasedAt);

    /// <summary>
    /// Creates a <see cref="LeaseAcquireResult"/> representing a lease that was not acquired.
    /// </summary>
    /// <param name="expires">When the lease expires.</param>
    /// <param name="lastAcquiredAt">When the lease was last acquired at.</param>
    /// <param name="lastReleasedAt">When the lease was last released at.</param>
    public static LeaseAcquireResult Failed(DateTimeOffset expires, DateTimeOffset? lastAcquiredAt, DateTimeOffset? lastReleasedAt)
        => new(null, expires, lastAcquiredAt, lastReleasedAt);
}
