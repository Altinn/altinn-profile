using System.Diagnostics;

using Altinn.Profile.Core.Leases;
using Altinn.Profile.Integrations.Persistence;

using Microsoft.EntityFrameworkCore;

using Lease = Altinn.Profile.Integrations.Leases.Lease;

namespace Altinn.Profile.Integrations.Repositories
{
    /// <summary>
    /// Defines a repository for operations related to a users groups of parties.
    /// </summary>
    public class LeaseRepository(IDbContextFactory<ProfileDbContext> contextFactory) : ILeaseRepository
    {
        private readonly IDbContextFactory<ProfileDbContext> _contextFactory = contextFactory;

        /// <inheritdoc />
        public async Task<LeaseAcquireResult> UpsertLease(Lease lease, DateTimeOffset now, Func<LeaseInfo, bool>? filter, CancellationToken cancellationToken)
        {
            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
            Lease? existingLease = await databaseContext.Lease.FirstOrDefaultAsync(l => l.Id == lease.Id, cancellationToken);
            LeaseAcquireResult result;

            if (existingLease is not null)
            {
                if (existingLease.Token == lease.Token || existingLease.Expires <= now)
                {
                    existingLease.Token = lease.Token;
                    existingLease.Expires = lease.Expires;
                    existingLease.Acquired = lease.Acquired ?? existingLease.Acquired;
                    existingLease.Released = lease.Released ?? existingLease.Released;

                    databaseContext.Lease.Update(existingLease);
                }
            }
            else
            {
                databaseContext.Lease.Add(lease);
            }

            await databaseContext.SaveChangesAsync(cancellationToken);

            if (filter is null || filter(new LeaseInfo { LastAcquiredAt = existingLease?.Acquired, LastReleasedAt = existingLease?.Released, LeaseId = lease.Id }))
            {
                result = GetLeaseAcquireResult(lease);
            }
            else
            {
                result = LeaseAcquireResult.Failed(existingLease?.Expires ?? DateTimeOffset.MinValue, existingLease?.Acquired, existingLease?.Released);
            }

            return result;
        }

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
        public async Task<LeaseAcquireResult> GetFailedLeaseResult(string id, CancellationToken cancellationToken)
        {
            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

            var lease = await databaseContext.Lease.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);

            if (lease == null)
            {
                throw new UnreachableException("Lease was not acquired, but no lease was found in the database");
            }

            return LeaseAcquireResult.Failed(lease.Expires, lease.Acquired, lease.Released);
        }

        private LeaseAcquireResult GetLeaseAcquireResult(Lease lease)
        {
            var leaseTicket = new LeaseTicket(lease.Id, lease.Token, lease.Expires);
            return LeaseAcquireResult.Acquired(leaseTicket, lease.Acquired, lease.Released);
        }
    }
}
