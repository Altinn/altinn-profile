using Altinn.Profile.Integrations.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories;

/// <summary>
/// Defines a repository for operations related to registry sync metadata.
/// </summary>
public class RegistrySyncMetadataRepository(IDbContextFactory<ProfileDbContext> contextFactory) : IRegistrySyncMetadataRepository
{
    private readonly IDbContextFactory<ProfileDbContext> _contextFactory = contextFactory;

    /// <inheritdoc />
    public async Task<DateTime?> GetLatestSyncTimestampAsync()
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

        var lastSync = await databaseContext.RegistrySyncMetadata.FirstOrDefaultAsync();
        if (lastSync == null)
        {
            return null;
        }

        return lastSync.LastChangedDateTime;
    }

    /// <inheritdoc />
    public async Task<DateTime> UpdateLatestChangeTimestampAsync(DateTime updated)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();
        var lastSync = await databaseContext.RegistrySyncMetadata.FirstOrDefaultAsync();
        if (lastSync == null)
        {
            lastSync = new Entities.RegistrySyncMetadata
            {
                LastChangedId = Guid.NewGuid().ToString("N"),
                LastChangedDateTime = updated
            };
            databaseContext.RegistrySyncMetadata.Add(lastSync);
        }
        else
        {
            lastSync.LastChangedDateTime = updated;

            databaseContext.RegistrySyncMetadata.Update(lastSync);
        }

        await databaseContext.SaveChangesAsync();
        return updated;
    }
}
