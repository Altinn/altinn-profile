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
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a an <see cref="ImmutableList{T}"/> of <see cref="PersonContactPreferences"/> objects representing the contact details of the persons.
    /// <summary>
/// Retrieves contact preference details for multiple persons identified by their national identity numbers.
/// </summary>
/// <param name="nationalIdentityNumbers">Collection of national identity numbers to retrieve contact preferences for.</param>
/// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
/// <returns>An immutable list of PersonContactPreferences for the specified persons; entries are included only for identities that could be resolved.</returns>
    Task<ImmutableList<PersonContactPreferences>> GetContactPreferencesAsync(IEnumerable<string> nationalIdentityNumbers, CancellationToken cancellationToken);
}
