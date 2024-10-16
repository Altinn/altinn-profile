#nullable enable

using Altinn.Profile.Core;
using Altinn.Profile.Core.Person.ContactPreferences;
using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations.Services;

/// <summary>
/// Defines a service for handling operations related to person data.
/// </summary>
public interface IPersonService
{
    /// <summary>
    /// Asynchronously retrieves the contact preferences for a single person based on their national identity number.
    /// </summary>
    /// <param name="nationalIdentityNumber">The national identity number of the person.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the person's contact preferences, or <c>null</c> if not found.
    /// </returns>
    Task<IPersonContactPreferences?> GetContactPreferencesAsync(string nationalIdentityNumber);

    /// <summary>
    /// Asynchronously retrieves the contact preferences for multiple persons based on their national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object, where <see cref="IPersonContactPreferencesLookupResult"/> represents the successful lookup result and <see cref="bool"/> indicates a failure.
    /// </returns>
    Task<Result<IPersonContactPreferencesLookupResult, bool>> GetContactPreferencesAsync(IEnumerable<string> nationalIdentityNumbers);

    /// <summary>
    /// Asynchronously synchronizes the person contact preferences.
    /// </summary>
    /// <returns></returns>
    Task<bool> SyncPersonContactPreferencesAsync();
}
