#nullable enable

namespace Altinn.Authorization.ServiceDefaults.Leases;

/// <summary>
/// Information about a lease.
/// </summary>
public record struct LeaseInfo
{
    /// <summary>
    /// Gets the lease id.
    /// </summary>
    public required string LeaseId { get; init; }

    /// <summary>
    /// Gets when the lease was last acquired at.
    /// </summary>
    public required DateTimeOffset? LastAcquiredAt { get; init; }

    /// <summary>
    /// Gets when the lease was last released at.
    /// </summary>
    public required DateTimeOffset? LastReleasedAt { get; init; }
}
