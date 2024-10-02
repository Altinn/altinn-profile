#nullable enable

using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations.Services;

/// <summary>
/// Register service for handling register data
/// </summary>
public interface IRegisterService
{
    /// <summary>
    /// Gets the by national identity number asynchronous.
    /// </summary>
    /// <param name="nationalIdentityNumber">The national identity number.</param>
    /// <returns></returns>
    Task<Register?> GetUserContactPointAsync(string nationalIdentityNumber);

    /// <summary>
    /// Gets the by national identity number asynchronous.
    /// </summary>
    /// <param name="nationalIdentityNumber">The national identity number.</param>
    /// <returns></returns>
    Task<IEnumerable<Register>> GetUserContactPointAsync(IEnumerable<string> nationalIdentityNumber);
}
