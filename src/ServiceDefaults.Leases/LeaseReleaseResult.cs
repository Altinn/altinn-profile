#nullable enable

namespace Altinn.Authorization.ServiceDefaults.Leases;

/// <summary>
/// The result of calling <see cref="ILeaseProvider.ReleaseLease(LeaseTicket, CancellationToken)"/>.
/// </summary>
public sealed class LeaseReleaseResult
{
    /// <summary>
    /// Gets a value indicating whether or not the lease was released.
    /// </summary>
    public required bool IsReleased { get; init; }

    /// <summary>
    /// Gets when the lease will expire (regardless of whether it was acquired).
    /// </summary>
    public required DateTimeOffset Expires { get; init; }

    /// <summary>
    /// Gets when the lease was last acquired at.
    /// </summary>
    /// <remarks>
    /// This does not signify that the lease is currently held.
    /// </remarks>
    public required DateTimeOffset? LastAcquiredAt { get; init; }

    /// <summary>
    /// Gets when the lease was last released at.
    /// </summary>
    /// <remarks>
    /// This does not signify that the lease is not currently held.
    /// </remarks>
    public required DateTimeOffset? LastReleasedAt { get; init; }
}
