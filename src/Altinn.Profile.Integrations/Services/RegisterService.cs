#nullable enable

using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Repositories;

namespace Altinn.Profile.Integrations.Services;

/// <summary>
/// Service for handling operations related to user registration and contact points.
/// </summary>
public class RegisterService : IRegisterService
{
    private readonly IRegisterRepository _registerRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterService"/> class.
    /// </summary>
    /// <param name="registerRepository">The repository used for accessing register data.</param>
    public RegisterService(IRegisterRepository registerRepository)
    {
        _registerRepository = registerRepository;
    }

    /// <summary>
    /// Asynchronously retrieves the user contact point by national identity number.
    /// </summary>
    /// <param name="nationalIdentityNumber">The national identity number of the user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user contact point if found; otherwise, null.</returns>
    public async Task<Register?> GetUserContactPointAsync(string nationalIdentityNumber)
    {
        return await _registerRepository.GetUserContactPointAsync(nationalIdentityNumber);
    }

    /// <summary>
    /// Asynchronously retrieves user contact points by a collection of national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of user contact points.</returns>
    public async Task<IEnumerable<Register>> GetUserContactPointAsync(IEnumerable<string> nationalIdentityNumbers)
    {
        return await _registerRepository.GetUserContactPointAsync(nationalIdentityNumbers);
    }
}
