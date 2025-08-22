using Altinn.Profile.Integrations.SblBridge.Changelog;

namespace Altinn.Profile.Integrations.Repositories;

/// <summary>
/// Defines a repository for operations related to Changelog sync metadata.
/// </summary>
public interface IChangelogSyncMetadataRepository
{
    /// <summary>
    /// Asynchronously retrieves the latest sync timestamp from the metadata repository.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    Task<DateTime?> GetLatestSyncTimestampAsync(DataType dataType);

    /// <summary>
    /// Asynchronously updates the latest sync timestamp in the metadata repository.
    /// </summary>
    /// <param name="updated">The new timestamp for last sync.</param>
    /// <param name="dataType">The type of data for which the sync timestamp is being updated.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    Task<DateTime> UpdateLatestChangeTimestampAsync(DateTime updated, DataType dataType);
}
