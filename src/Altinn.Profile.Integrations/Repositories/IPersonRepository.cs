using System.Collections.Immutable;

using Altinn.Profile.Core;
using Altinn.Profile.Core.ContactRegister;
using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations.Repositories;

/// <summary>
/// Defines a repository for handling person data operations.
/// </summary>
public interface IPersonRepository
{
    /// <summary>
    /// Asynchronously retrieves the contact details for multiple persons by their national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers to look up.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object with an <see cref="ImmutableList{T}"/> of <see cref="Person"/> objects representing the contact details of the persons on success, or a <see cref="bool"/> indicating failure.
    /// </returns>
    Task<Result<ImmutableList<Person>, bool>> GetContactDetailsAsync(IEnumerable<string> nationalIdentityNumbers);

    /// <summary>
    /// Asynchronously synchronizes the changes in person contact preferences.
    /// </summary>
    /// <param name="personContactPreferencesSnapshots">The snapshots of person contact preferences to be synchronized.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object with a <see cref="bool"/> indicating success or failure.
    /// </returns>
    Task<int> SyncPersonContactPreferencesAsync(ContactRegisterChangesLog personContactPreferencesSnapshots);
}
