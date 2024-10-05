using System.Collections.Immutable;

using Altinn.Profile.Core;
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
    private readonly INationalIdentityNumberChecker _nationalIdentityNumberChecker;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterService"/> class.
    /// </summary>
    /// <param name="mapper">The mapper used for object mapping.</param>
    /// <param name="registerRepository">The repository used for accessing register data.</param>
    /// <param name="nationalIdentityNumberChecker">The service used for checking the validity of national identity numbers.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="mapper"/>, <paramref name="registerRepository"/>, or <paramref name="nationalIdentityNumberChecker"/> is <c>null</c>.</exception>
    public RegisterService(IMapper mapper, IRegisterRepository registerRepository, INationalIdentityNumberChecker nationalIdentityNumberChecker)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _registerRepository = registerRepository ?? throw new ArgumentNullException(nameof(registerRepository));
        _nationalIdentityNumberChecker = nationalIdentityNumberChecker ?? throw new ArgumentNullException(nameof(nationalIdentityNumberChecker));
    }

    /// <summary>
    /// Asynchronously retrieves the contact information for a user based on their national identity number.
    /// </summary>
    /// <param name="nationalIdentityNumber">The national identity number of the user.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the user's contact information, or <c>null</c> if not found.
    /// </returns>
    public async Task<IUserContact?> GetUserContactAsync(string nationalIdentityNumber)
    {
        if (!_nationalIdentityNumberChecker.IsValid(nationalIdentityNumber))
        {
            return null;
        }

        var userContactInfoEntity = await _registerRepository.GetUserContactInfoAsync([nationalIdentityNumber]);
        return _mapper.Map<IUserContact>(userContactInfoEntity);
    }

    /// <summary>
    /// Asynchronously retrieves the contact information for multiple users based on their national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of user contact information, or an empty collection if none are found.
    /// </returns>
    public async Task<Result<IUserContactResult, bool>> GetUserContactAsync(IEnumerable<string> nationalIdentityNumbers)
    {
        var (validSocialSecurityNumbers, invalidSocialSecurityNumbers) = _nationalIdentityNumberChecker.Categorize(nationalIdentityNumbers);

        var userContactInfoEntities = await _registerRepository.GetUserContactInfoAsync(validSocialSecurityNumbers);

        var matchedUserContact = userContactInfoEntities.Select(_mapper.Map<UserContact>);

        var unmatchedUserContact = nationalIdentityNumbers
            .Except(userContactInfoEntities.Select(e => e.FnumberAk))
            .Select(e => new UserContact { NationalIdentityNumber = e })
            .Select(_mapper.Map<UserContact>);

        return new UserContactResult
        {
            MatchedUserContact = matchedUserContact?.ToImmutableList<IUserContact>(),
            UnmatchedUserContact = unmatchedUserContact?.ToImmutableList<IUserContact>(),
        };
    }
}
