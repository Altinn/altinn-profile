using Altinn.Profile.Core;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories;

/// <summary>
/// Provides methods for handling metadata operations in the profile database.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MetadataRepository"/> class.
/// </remarks>
/// <param name="contextFactory">The factory for creating database context instances.</param>
public class MetadataRepository(IDbContextFactory<ProfileDbContext> contextFactory) : IMetadataRepository
{
    private readonly IDbContextFactory<ProfileDbContext> _contextFactory = contextFactory;

    /// <summary>
    /// Asynchronously retrieves the latest change number from the metadata repository.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    public async Task<long> GetLatestChangeNumberAsync()
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
    /// A task that represents the asynchronous operation.
    /// </returns>
    public async Task<long> UpdateLatestChangeNumberAsync(long newNumber)
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
