using Altinn.Profile.Core;

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
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object with a <see cref="long"/> value on success, or a <see cref="bool"/> value indicating failure.
    /// </returns>
    Task<Result<long, bool>> GetLatestChangeNumberAsync();

    /// <summary>
    /// Asynchronously updates the latest change number from the metadata repository.
    /// </summary>
    /// <param name="newNumber">The new changed number.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object with a <see cref="long"/> value on success, or a <see cref="bool"/> value indicating failure.
    /// </returns>
    Task<Result<long, bool>> UpdateLatestChangeNumberAsync(long newNumber);
}
