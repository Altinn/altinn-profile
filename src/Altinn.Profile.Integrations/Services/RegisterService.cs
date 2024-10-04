using Altinn.Profile.Core.Extensions;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Repositories;

using AutoMapper;

namespace Altinn.Profile.Integrations.Services;

/// <summary>
/// Service for handling operations related to user registration and contact points.
/// </summary>
public class RegisterService : IRegisterService
{
    private readonly IMapper _mapper;
    private readonly IRegisterRepository _registerRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterService"/> class.
    /// </summary>
    /// <param name="mapper">The mapper used for object mapping.</param>
    /// <param name="registerRepository">The repository used for accessing register data.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="mapper"/> or <paramref name="registerRepository"/> object is null.</exception>
    public RegisterService(IMapper mapper, IRegisterRepository registerRepository)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _registerRepository = registerRepository ?? throw new ArgumentNullException(nameof(registerRepository));
    }

    /// <summary>
    /// Asynchronously retrieves the contact information for a user based on their national identity number.
    /// </summary>
    /// <param name="nationalIdentityNumber">The national identity number of the user.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the user's contact information, or <c>null</c> if not found.
    /// </returns>
    public async Task<IUserContactInfo?> GetUserContactInfoAsync(string nationalIdentityNumber)
    {
        if (!nationalIdentityNumber.IsValidSocialSecurityNumber())
        {
            return null;
        }

        var userContactInfo = await _registerRepository.GetUserContactInfoAsync([nationalIdentityNumber]);
        return _mapper.Map<IUserContactInfo>(userContactInfo);
    }

    /// <summary>
    /// Asynchronously retrieves the contact information for multiple users based on their national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of user contact information, or an empty collection if none are found.
    /// </returns>
    public async Task<IEnumerable<IUserContactInfo>> GetUserContactInfoAsync(IEnumerable<string> nationalIdentityNumbers)
    {
        if (nationalIdentityNumbers == null || !nationalIdentityNumbers.Any())
        {
            return [];
        }

        // Filter out invalid national identity numbers
        var validNationalIdentityNumbers = nationalIdentityNumbers.Where(e => e.IsValidSocialSecurityNumber());
        if (!validNationalIdentityNumbers.Any())
        {
            return [];
        }

        var userContactInfo = await _registerRepository.GetUserContactInfoAsync(validNationalIdentityNumbers);

        return _mapper.Map<IEnumerable<UserContactInfo>>(userContactInfo);
    }
}
