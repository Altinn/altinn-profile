#nullable enable

using Altinn.Profile.Core.Domain;
using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations.Repositories;

/// <summary>
/// Repository for handling register data.
/// </summary>
public interface IRegisterRepository : IRepository<Register>
{
    /// <summary>
    /// Gets the contact info for a single user by the national identity number asynchronously.
    /// </summary>
    /// <param name="nationalIdentityNumber">The national identity number.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the register data for the user.</returns>
    Task<Register?> GetUserContactInfoAsync(string nationalIdentityNumber);

    /// <summary>
    /// Gets the contact info for multiple users by their national identity numbers asynchronously.
    /// </summary>
    /// <param name="nationalIdentityNumbers">The collection of national identity numbers.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of register data for the users.</returns>
    Task<IEnumerable<Register>> GetUserContactInfoAsync(IEnumerable<string> nationalIdentityNumbers);
}
