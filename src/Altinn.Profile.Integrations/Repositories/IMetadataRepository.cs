using Altinn.Profile.Core;

namespace Altinn.Profile.Integrations.Repositories;

/// <summary>
/// Defines a repository for handling metadata.
/// </summary>
public interface IMetadataRepository
{
    /// <summary>
    /// Asynchronously retrieves the latest change number.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object with a <see cref="long"/> on success, or a boolean indicating failure.</returns>
    Task<Result<long, bool>> GetLatestChangeNumberAsync();
}
