namespace Altinn.Profile.Integrations.Repositories;

/// <summary>
/// Defines a repository for operations related to registry sync metadata.
/// </summary>
public interface IRegistrySyncMetadataRepository
{
    /// <summary>
    /// Asynchronously retrieves the latest sync timestamp from the metadata repository.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    Task<DateTime?> GetLatestSyncTimestampAsync();

    /// <summary>
    /// Asynchronously updates the latest sync timestamp in the metadata repository.
    /// </summary>
    /// <param name="updated">The new timestamp for last sync.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    Task<DateTime> UpdateLatestChangeTimestampAsync(DateTime updated);
}
