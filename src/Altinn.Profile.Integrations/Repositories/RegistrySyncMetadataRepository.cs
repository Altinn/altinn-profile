using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Entities;
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
    public async Task<DateTime?> GetLatestSyncTimestampAsync(CancellationToken cancellationToken = default)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

        var lastSync = await databaseContext.RegistrySyncMetadata.FirstOrDefaultAsync(cancellationToken);
        if (lastSync == null)
        {
            return null;
        }

        return lastSync.LastChangedDateTime;
    }

    /// <inheritdoc />
    public async Task<DateTime> UpdateLatestChangeTimestampAsync(DateTime updated, CancellationToken cancellationToken = default)
    {
        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var lastSync = await databaseContext.RegistrySyncMetadata.FirstOrDefaultAsync(cancellationToken);
        if (lastSync == null)
        {
            lastSync = new RegistrySyncMetadata
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

        await databaseContext.SaveChangesAsync(cancellationToken);
        return updated;
    }
}
