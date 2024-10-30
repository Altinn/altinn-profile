namespace Altinn.Profile.Integrations.Repositories;

/// <summary>
/// Defines a repository for handling metadata operations.
/// </summary>
public interface IMetadataRepository
{
    /// <summary>
    /// Asynchronously retrieves the latest change number from the metadata repository.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    Task<long> GetLatestChangeNumberAsync();

    /// <summary>
    /// Asynchronously updates the latest change number from the metadata repository.
    /// </summary>
    /// <param name="newNumber">The new changed number.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    Task<long> UpdateLatestChangeNumberAsync(long newNumber);
}
