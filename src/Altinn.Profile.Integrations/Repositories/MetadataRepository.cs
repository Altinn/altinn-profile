using Altinn.Profile.Core;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories;

/// <summary>
/// Provides methods for handling metadata operations in the profile database.
/// </summary>
public class MetadataRepository : IMetadataRepository
{
    private readonly IDbContextFactory<ProfileDbContext> _contextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="MetadataRepository"/> class.
    /// </summary>
    /// <param name="contextFactory">The factory for creating database context instances.</param>
    /// <exception cref="ArgumentNullException"> Thrown when the <paramref name="contextFactory"/> is null. </exception>
    public MetadataRepository(IDbContextFactory<ProfileDbContext> contextFactory)
    {
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    /// <summary>
    /// Asynchronously retrieves the latest change number from the metadata repository.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object with a <see cref="long"/> value on success, or a <see cref="bool"/> value indicating failure.
    /// </returns>
    public async Task<Result<long, bool>> GetLatestChangeNumberAsync()
    {
        using ProfileDbContext databaseContext = _contextFactory.CreateDbContext();

        Metadata? metadataSingleRow = await databaseContext.Metadata.FirstOrDefaultAsync();

        return metadataSingleRow != null ? metadataSingleRow.LatestChangeNumber : 0;
    }

    /// <summary>
    /// Asynchronously updates the latest change number from the metadata repository.
    /// </summary>
    /// <param name="newNumber">The new changed number.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object with a <see cref="long"/> value on success, or a <see cref="bool"/> value indicating failure.
    /// </returns>
    public async Task<Result<long, bool>> UpdateLatestChangeNumberAsync(long newNumber)
    {
        if (newNumber < 0)
        {
            throw new ArgumentException("The new change number must be non-negative.", nameof(newNumber));
        }

        using ProfileDbContext databaseContext = _contextFactory.CreateDbContext();
        Metadata? existingMetadata = await databaseContext.Metadata.FirstOrDefaultAsync();

        if (existingMetadata != null)
        {
            databaseContext.Metadata.Remove(existingMetadata);
        }

        Metadata metadata = new()
        {
            Exported = DateTime.UtcNow,
            LatestChangeNumber = newNumber
        };

        await databaseContext.Metadata.AddAsync(metadata);

        return await databaseContext.SaveChangesAsync() > 0 ? newNumber : -1;
    }
}
