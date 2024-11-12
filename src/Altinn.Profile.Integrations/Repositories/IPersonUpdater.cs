using Altinn.Profile.Core;
using Altinn.Profile.Integrations.ContactRegister;

namespace Altinn.Profile.Integrations.Repositories;

/// <summary>
/// Defines a repository for updating person data.
/// </summary>
public interface IPersonUpdater
{
    /// <summary>
    /// Asynchronously synchronizes the changes in person contact preferences.
    /// </summary>
    /// <param name="personContactPreferencesSnapshots">The snapshots of person contact preferences to be synchronized.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object with a <see cref="bool"/> indicating success or failure.
    /// </returns>
    Task<int> SyncPersonContactPreferencesAsync(ContactRegisterChangesLog personContactPreferencesSnapshots);
}
