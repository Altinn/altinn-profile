#nullable enable

using Altinn.Profile.Core;
using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations.Services;

/// <summary>
/// Defines a service for handling operations related to person data.
/// </summary>
public interface IPersonService
{
    /// <summary>
    /// Asynchronously retrieves the contact preferences for multiple persons based on their national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object, where <see cref="IPersonContactPreferencesLookupResult"/> represents the successful lookup result and <see cref="bool"/> indicates a failure.
    /// </returns>
    Task<Result<IPersonContactPreferencesLookupResult, bool>> GetContactPreferencesAsync(IEnumerable<string> nationalIdentityNumbers);
}
