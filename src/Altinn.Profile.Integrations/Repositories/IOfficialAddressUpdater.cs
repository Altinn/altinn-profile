using Altinn.Profile.Core;
using Altinn.Profile.Integrations.OfficialAddressRegister;

namespace Altinn.Profile.Integrations.Repositories;

/// <summary>
/// Defines a repository for handling metadata operations.
/// </summary>
public interface IOfficialAddressUpdater
{
    /// <summary>
    /// Asynchronously synchronizes the changes in official contact preferences.
    /// </summary>
    /// <param name="officialAddressChanges">The snapshots of offical contact addresses to be synchronized.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object with a <see cref="bool"/> indicating success or failure.
    /// </returns>
    Task<int> SyncOfficialContactsAsync(OfficialAddressRegisterChangesLog officialAddressChanges);
}
