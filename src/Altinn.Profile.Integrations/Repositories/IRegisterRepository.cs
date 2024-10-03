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
    /// Gets the contact info for multiple users by their national identity numbers asynchronously.
    /// </summary>
    /// <param name="nationalIdentityNumbers">The collection of national identity numbers.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of register data for the users.</returns>
    Task<IEnumerable<Register>> GetUserContactInfoAsync(IEnumerable<string> nationalIdentityNumbers);
}
