#nullable enable

using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations.Services;

/// <summary>
/// Interface for a service that handles operations related to user register data.
/// </summary>
public interface IRegisterService
{
    /// <summary>
    /// Asynchronously retrieves the contact point information for a user based on their national identity number.
    /// </summary>
    /// <param name="nationalIdentityNumber">The national identity number of the user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user's register information, or null if not found.</returns>
    Task<Register?> GetUserContactPointAsync(string nationalIdentityNumber);

    /// <summary>
    /// Asynchronously retrieves the contact point information for multiple users based on their national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of user register information, or null if none are found.</returns>
    Task<IEnumerable<Register>> GetUserContactPointAsync(IEnumerable<string> nationalIdentityNumbers);
}
