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
    /// Asynchronously retrieves the contact details for a single person based on their national identity number.
    /// </summary>
    /// <param name="nationalIdentityNumber">The national identity number of the person.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the person's contact details, or <c>null</c> if not found.
    /// </returns>
    Task<IPersonContactDetails?> GetContactDetailsAsync(string nationalIdentityNumber);

    /// <summary>
    /// Asynchronously retrieves the contact details for multiple persons based on their national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object, where <see cref="IPersonContactDetailsLookupResult"/> represents the successful lookup result and <see cref="bool"/> indicates a failure.
    /// </returns>
    Task<Result<IPersonContactDetailsLookupResult, bool>> GetContactDetailsAsync(IEnumerable<string> nationalIdentityNumbers);
}
