using System.Collections.Immutable;

using Altinn.Profile.Core.Person.ContactPreferences;

namespace Altinn.Profile.Core.Integrations;

/// <summary>
/// Defines a service for read-operations related to person data.
/// </summary>
public interface IPersonService
{
    /// <summary>
    /// Asynchronously retrieves the contact details for multiple persons by their national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers to look up.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a an <see cref="ImmutableList{T}"/> of <see cref="PersonContactPreferences"/> objects representing the contact details of the persons.
    /// </returns>
    Task<ImmutableList<PersonContactPreferences>> GetContactPreferencesAsync(IEnumerable<string> nationalIdentityNumbers);
}
