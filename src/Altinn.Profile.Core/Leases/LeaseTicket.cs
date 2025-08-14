#nullable enable

namespace Altinn.Profile.Core.Leases;

/// <summary>
/// A ticket representing a lease.
/// </summary>
public sealed record LeaseTicket
{
    /// <summary>
    /// Gets the lease id.
    /// </summary>
    public string LeaseId { get; }

    /// <summary>
    /// Gets the lease token.
    /// </summary>
    public Guid Token { get; }

    /// <summary>
    /// Gets when the lease expires.
    /// </summary>
    public DateTimeOffset Expires { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LeaseTicket"/> class.
    /// </summary>
    public LeaseTicket(string leaseId, Guid token, DateTimeOffset expires)
    {
        LeaseId = leaseId;
        Token = token;
        Expires = expires;
    }
}
