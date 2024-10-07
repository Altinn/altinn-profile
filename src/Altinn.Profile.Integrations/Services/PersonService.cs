using System.Collections.Immutable;

using Altinn.Profile.Core;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Repositories;

using AutoMapper;

namespace Altinn.Profile.Integrations.Services;

/// <summary>
/// Service for handling operations related to user registration and contact points.
/// </summary>
public class PersonService : IPersonService
{
    private readonly IMapper _mapper;
    private readonly IPersonRepository _registerRepository;
    private readonly INationalIdentityNumberChecker _nationalIdentityNumberChecker;

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonService"/> class.
    /// </summary>
    /// <param name="mapper">The mapper used for object mapping.</param>
    /// <param name="registerRepository">The repository used for accessing register data.</param>
    /// <param name="nationalIdentityNumberChecker">The service used for checking the validity of national identity numbers.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="mapper"/>, <paramref name="registerRepository"/>, or <paramref name="nationalIdentityNumberChecker"/> is <c>null</c>.</exception>
    public PersonService(IMapper mapper, IPersonRepository registerRepository, INationalIdentityNumberChecker nationalIdentityNumberChecker)
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
    public async Task<IUserContactInfo?> GetUserContactInfoAsync(string nationalIdentityNumber)
    {
        if (!_nationalIdentityNumberChecker.IsValid(nationalIdentityNumber))
        {
            return null;
        }

        var userContactInfoEntity = await _registerRepository.GetUserContactInfoAsync([nationalIdentityNumber]);
        return _mapper.Map<IUserContactInfo>(userContactInfoEntity);
    }

    /// <summary>
    /// Asynchronously retrieves the contact information for multiple users based on their national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of user contact information, or an empty collection if none are found.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="nationalIdentityNumbers"/> is <c>null</c>.</exception>
    public async Task<Result<IUserContactInfoLookupResult, bool>> GetUserContactAsync(IEnumerable<string> nationalIdentityNumbers)
    {
        ArgumentNullException.ThrowIfNull(nationalIdentityNumbers);

        var (validnNtionalIdentityNumbers, _) = _nationalIdentityNumberChecker.Categorize(nationalIdentityNumbers);

        var usersContactInfo = await _registerRepository.GetUserContactInfoAsync(validnNtionalIdentityNumbers);

        var matchedUserContact = usersContactInfo.Select(_mapper.Map<UserContactInfo>);

        var matchedNationalIdentityNumbers = new HashSet<string>(usersContactInfo.Select(e => e.FnumberAk));
        var unmatchedNationalIdentityNumbers = nationalIdentityNumbers.Where(e => !matchedNationalIdentityNumbers.Contains(e));

        return new UserContactInfoLookupResult
        {
            MatchedUserContact = matchedUserContact.ToImmutableList<IUserContactInfo>(),
            UnmatchedNationalIdentityNumbers = unmatchedNationalIdentityNumbers.ToImmutableList()
        };
    }
}
