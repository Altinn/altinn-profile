using System.Collections.Generic;
using System.Threading.Tasks;

using Altinn.Profile.Models;

namespace Altinn.Profile.Repositories;

/// <summary>
/// Repository for handling register data
/// </summary>
public interface IRegisterRepository : IRepository<Register>
{
    /// <summary>
    /// Gets the by national identity number asynchronous.
    /// </summary>
    /// <param name="nationalIdentityNumber">The national identity number.</param>
    /// <returns></returns>
    Task<Register> GetUserContactPointAsync(string nationalIdentityNumber);

    /// <summary>
    /// Gets the by national identity number asynchronous.
    /// </summary>
    /// <param name="nationalIdentityNumber">The national identity number.</param>
    /// <returns></returns>
    Task<IEnumerable<Register>> GetUserContactPointAsync(IEnumerable<string> nationalIdentityNumber);
}
