#nullable enable

using System.Collections.Immutable;

using Altinn.Profile.Core.Domain;
using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations.Repositories;

/// <summary>
/// Defines a repository for handling person data operations.
/// </summary>
public interface IPersonRepository : IRepository<Person>
{
    /// <summary>
    /// Asynchronously retrieves the contact details for multiple persons by their national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers to look up for.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an <see cref="ImmutableList{T}"/> of <see cref="Person"/> objects representing the contact details of the persons.
    /// </returns>
    Task<ImmutableList<Person>> GetContactDetailsAsync(IEnumerable<string> nationalIdentityNumbers);

    /// <summary>
    /// Gets the last c hanged number.
    /// </summary>
    /// <returns></returns>
    Task<long> GetLastCHangedNumber();

    /// <summary>
    /// Synchronizes the person contact preferences asynchronous.
    /// </summary>
    /// <param name="personContactPreferencesSnapshots">The person contact preferences snapshots.</param>
    /// <returns></returns>
    Task<bool> SyncPersonContactPreferencesAsync(IEnumerable<IPersonContactPreferencesSnapshot> personContactPreferencesSnapshots);
}
