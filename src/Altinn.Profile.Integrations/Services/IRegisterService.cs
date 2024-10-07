#nullable enable

using Altinn.Profile.Core;
using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations.Services;

/// <summary>
/// Defines a service for handling operations related to user register data.
/// </summary>
public interface IRegisterService
{
    /// <summary>
    /// Asynchronously retrieves the contact information for a user based on their national identity number.
    /// </summary>
    /// <param name="nationalIdentityNumber">The national identity number of the user.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the user's contact information, or <c>null</c> if not found.
    /// </returns>
    Task<IUserContactInfo?> GetUserContactAsync(string nationalIdentityNumber);

    /// <summary>
    /// Asynchronously retrieves the contact information for multiple users based on their national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of user contact information, or an empty collection if none are found.
    /// </returns>
    Task<Result<IUserContactInfoLookupResult, bool>> GetUserContactAsync(IEnumerable<string> nationalIdentityNumbers);
}
