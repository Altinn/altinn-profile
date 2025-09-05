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

        var lastChangeDate = new DateTime(lastSync.LastChangeTicks, DateTimeKind.Utc);
        return lastChangeDate;
    }

    /// <inheritdoc />
    public async Task<DateTime> UpdateLatestChangeTimestampAsync(DateTime updated, DataType dataType)
    {
        var ticks = updated.Ticks; // Get the datetime in ticks to keep precision of 100 nanoseconds

        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();
        var lastSync = await databaseContext.ChangelogSyncMetadata.FirstOrDefaultAsync(e => e.DataType == dataType);
        if (lastSync == null)
        {
            lastSync = new Entities.ChangelogSyncMetadata
            {
                LastChangedId = Guid.NewGuid().ToString("N"),
                DataType = dataType,
                LastChangeTicks = ticks,
            };
            databaseContext.ChangelogSyncMetadata.Add(lastSync);
        }
        else
        {
            lastSync.LastChangeTicks = ticks;
            databaseContext.ChangelogSyncMetadata.Update(lastSync);
        }

        await databaseContext.SaveChangesAsync();
        return updated;
    }
}
