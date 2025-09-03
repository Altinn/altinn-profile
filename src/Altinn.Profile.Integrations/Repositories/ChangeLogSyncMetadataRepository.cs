using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.SblBridge.Changelog;

using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories;

/// <summary>
/// Defines a repository for operations related to Changelog sync metadata.
/// </summary>
public class ChangelogSyncMetadataRepository(IDbContextFactory<ProfileDbContext> contextFactory) : IChangelogSyncMetadataRepository
{
    private readonly IDbContextFactory<ProfileDbContext> _contextFactory = contextFactory;

    /// <inheritdoc />
    public async Task<DateTime?> GetLatestSyncTimestampAsync(DataType dataType, CancellationToken cancellationToken)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var lastSync = await databaseContext.ChangelogSyncMetadata
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.DataType == dataType, cancellationToken);

        if (lastSync == null)
        {
            return null;
        }

        // Reconstruct the DateTime with nanoseconds
        // AddTicks takes 100 nanoseconds per tick
        var ticks = lastSync.Nanosecond / 100;
        lastSync.LastChangedDateTime = lastSync.LastChangedDateTime.AddTicks(ticks);
        return lastSync.LastChangedDateTime;
    }

    /// <inheritdoc />
    public async Task<DateTime> UpdateLatestChangeTimestampAsync(DateTime updated, DataType dataType)
    {
        var nano = updated.Nanosecond;
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();
        var lastSync = await databaseContext.ChangelogSyncMetadata.FirstOrDefaultAsync(e => e.DataType == dataType);
        if (lastSync == null)
        {
            lastSync = new Entities.ChangelogSyncMetadata
            {
                LastChangedId = Guid.NewGuid().ToString("N"),
                LastChangedDateTime = updated,
                DataType = dataType,
                Nanosecond = nano
            };
            databaseContext.ChangelogSyncMetadata.Add(lastSync);
        }
        else
        {
            lastSync.LastChangedDateTime = updated;
            lastSync.Nanosecond = nano;
            databaseContext.ChangelogSyncMetadata.Update(lastSync);
        }

        await databaseContext.SaveChangesAsync();
        return updated;
    }
}
