using System.Diagnostics;

using Altinn.Authorization.ServiceDefaults.Leases;

using Lease = Altinn.Profile.Integrations.Leases.Lease;

namespace Altinn.Profile.Integrations.Repositories.A2Sync
{
    /// <summary>
    /// Defines a repository for Lease operations.
    /// </summary>
    /// <remarks>Can be removed when Altinn2 is decommissioned</remarks>
    public interface ILeaseRepository
    {
        /// <summary>
        /// Attempts to acquire a lease for the specified lease object at the given time.
        /// </summary>
        /// <param name="lease">The lease object to acquire.</param>
        /// <param name="now">The current time used for lease acquisition.</param>
        /// <param name="filter">A filter that can be used to reject leases based on the state of the lease provider.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>
        /// A <see cref="LeaseAcquireResult"/> representing the result of the lease acquisition operation.
        /// </returns>
        Task<LeaseAcquireResult> UpsertLease(Lease lease, DateTimeOffset now, Func<LeaseInfo, bool>? filter, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves a lease by its identifier.
        /// </summary>
        /// <param name="id">The identifier of the lease to retrieve.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>
        /// A <see cref="LeaseAcquireResult"/> representing the result of the lease retrieval operation.
        /// </returns>
        /// <exception cref="UnreachableException">
        /// Thrown if the lease was not acquired and no lease was found in the database.
        /// </exception>
        Task<LeaseAcquireResult> GetFailedLeaseResult(string id, CancellationToken cancellationToken);
    }
}
