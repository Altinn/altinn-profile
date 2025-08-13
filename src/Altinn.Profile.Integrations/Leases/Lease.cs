namespace Altinn.Profile.Integrations.Leases
{
    /// <summary>
    /// A record representing a lease in the system.
    /// </summary>
    public record Lease
    {
        /// <summary>
        /// Gets or sets the unique identifier for the lease.
        /// </summary>
        public required string Id { get; set; }

        /// <summary>
        /// Gets or sets the cancellationToken associated with the lease.
        /// </summary>
        public Guid Token { get; set; }

        /// <summary>
        /// Gets or sets the expiration timestamp of the lease.
        /// </summary>
        public DateTimeOffset Expires { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the lease was acquired, if applicable.
        /// </summary>
        public DateTimeOffset? Acquired { get; set; }

        /// <summary>
        /// Gets or sets the timestamp when the lease was released, if applicable.
        /// </summary>
        public DateTimeOffset? Released { get; set; }
    }
}
