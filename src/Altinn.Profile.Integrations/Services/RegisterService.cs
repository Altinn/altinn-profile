using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Extensions;
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
        _registerRepository = registerRepository ?? throw new ArgumentNullException(nameof(registerRepository));
    }

    /// <summary>
    /// Asynchronously retrieves the contact point information for a user based on their national identity number.
    /// </summary>
    /// <param name="nationalIdentityNumber">The national identity number of the user.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the user's register information, or null if not found.</returns>
    public async Task<Register?> GetUserContactPointAsync(string nationalIdentityNumber)
    {
        if (!IsValidNationalIdentityNumber(nationalIdentityNumber))
        {
            return null;
        }

        return await _registerRepository.GetUserContactPointAsync(nationalIdentityNumber);
    }

    /// <summary>
    /// Asynchronously retrieves the contact point information for multiple users based on their national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of user register information, or an empty collection if none are found.</returns>
    public async Task<IEnumerable<Register>> GetUserContactPointAsync(IEnumerable<string> nationalIdentityNumbers)
    {
        if (nationalIdentityNumbers == null || !nationalIdentityNumbers.Any())
        {
            return [];
        }

        if (!nationalIdentityNumbers.All(IsValidNationalIdentityNumber))
        {
            return [];
        }

        return await _registerRepository.GetUserContactPointAsync(nationalIdentityNumbers);
    }

    /// <summary>
    /// Validates the national identity number.
    /// </summary>
    /// <param name="nationalIdentityNumber">The national identity number to validate.</param>
    /// <returns>True if the national identity number is valid; otherwise, false.</returns>
    private bool IsValidNationalIdentityNumber(string? nationalIdentityNumber)
    {
        return !string.IsNullOrWhiteSpace(nationalIdentityNumber) && nationalIdentityNumber.IsDigitsOnly();
    }
}
