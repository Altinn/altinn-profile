using System.Diagnostics;

using Altinn.Authorization.ServiceDefaults.Leases;
using Altinn.Profile.Integrations.Persistence;

using Microsoft.EntityFrameworkCore;

using Lease = Altinn.Profile.Integrations.Leases.Lease;

namespace Altinn.Profile.Integrations.Repositories
{
    /// <summary>
    /// A repository for lease operations.
    /// </summary>
    public class LeaseRepository(IDbContextFactory<ProfileDbContext> contextFactory) : ILeaseRepository
    {
        private readonly IDbContextFactory<ProfileDbContext> _contextFactory = contextFactory;

        /// <inheritdoc />
        public async Task<LeaseAcquireResult> UpsertLease(Lease lease, DateTimeOffset now, Func<LeaseInfo, bool>? filter, CancellationToken cancellationToken)
        {
            using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
            
            // Start transaction with RepeatableRead isolation level (equivalent to Register)
            using var transaction = await databaseContext.Database.BeginTransactionAsync(System.Data.IsolationLevel.RepeatableRead, cancellationToken);
            
            try
            {
                // Get existing lease
                Lease? existingLease = await databaseContext.Lease
                    .FirstOrDefaultAsync(l => l.Id == lease.Id, cancellationToken);

                LeaseAcquireResult result;

                if (existingLease is not null)
                {
                    // Update only if token matches or lease has expired
                    if (existingLease.Token == lease.Token || existingLease.Expires <= now)
                    {
                        existingLease.Token = lease.Token;
                        existingLease.Expires = lease.Expires;
                        existingLease.Acquired = lease.Acquired ?? existingLease.Acquired;
                        existingLease.Released = lease.Released ?? existingLease.Released;

                        databaseContext.Lease.Update(existingLease);
                    }
                    else
                    {
                        // Lease is held by another token and has not expired
                        result = LeaseAcquireResult.Failed(existingLease.Expires, existingLease.Acquired, existingLease.Released);
                        return result;
                    }
                }
                else
                {
                    databaseContext.Lease.Add(lease);
                }

                await databaseContext.SaveChangesAsync(cancellationToken);

                // Check filter before commit
                if (filter is null || filter(new LeaseInfo 
                { 
                    LastAcquiredAt = existingLease?.Acquired, 
                    LastReleasedAt = existingLease?.Released, 
                    LeaseId = lease.Id 
                }))
                {
                    // Use updated values from database if lease exists
                    if (existingLease is not null)
                    {
                        result = GetLeaseAcquireResult(existingLease);
                    }
                    else
                    {
                        result = GetLeaseAcquireResult(lease);
                    }
                }
                else
                {
                    // Rollback transaction if filter fails
                    await transaction.RollbackAsync(cancellationToken);
                    
                    result = LeaseAcquireResult.Failed(
                        existingLease?.Expires ?? DateTimeOffset.MinValue, 
                        existingLease?.Acquired, 
                        existingLease?.Released);
                    
                    return result;
                }

                // Commit transaction if everything is OK
                await transaction.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                // Rollback on error
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
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

        private static LeaseAcquireResult GetLeaseAcquireResult(Lease lease)
        {
            var leaseTicket = new LeaseTicket(lease.Id, lease.Token, lease.Expires);
            return LeaseAcquireResult.Acquired(leaseTicket, lease.Acquired, lease.Released);
        }
    }
}
