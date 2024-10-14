#nullable enable

using System.Collections.Immutable;

using Altinn.Profile.Core;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Repositories;

using AutoMapper;

namespace Altinn.Profile.Integrations.Services;

/// <summary>
/// Provides a service for handling operations related to person data.
/// </summary>
public class PersonService : IPersonService
{
    private readonly IMapper _mapper;
    private readonly IPersonRepository _personRepository;
    private readonly INationalIdentityNumberChecker _nationalIdentityNumberChecker;

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonService"/> class.
    /// </summary>
    /// <param name="mapper">The mapper used for object mapping.</param>
    /// <param name="personRepository">The repository used for accessing the person data.</param>
    /// <param name="nationalIdentityNumberChecker">The service used for checking the validity of national identity numbers.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="mapper"/>, <paramref name="personRepository"/>, or <paramref name="nationalIdentityNumberChecker"/> is <c>null</c>.
    /// </exception>
    public PersonService(IMapper mapper, IPersonRepository personRepository, INationalIdentityNumberChecker nationalIdentityNumberChecker)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _nationalIdentityNumberChecker = nationalIdentityNumberChecker ?? throw new ArgumentNullException(nameof(nationalIdentityNumberChecker));
    }

    /// <summary>
    /// Asynchronously retrieves the contact preferences for a single person based on their national identity number.
    /// </summary>
    /// <param name="nationalIdentityNumber">The national identity number of the person.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the person's contact preferences, or <c>null</c> if not found.
    /// </returns>
    public async Task<IPersonContactPreferences?> GetContactPreferencesAsync(string nationalIdentityNumber)
    {
        if (!_nationalIdentityNumberChecker.IsValid(nationalIdentityNumber))
        {
            return null;
        }

        var personContactDetails = await _personRepository.GetContactDetailsAsync([nationalIdentityNumber]);
        return _mapper.Map<IPersonContactPreferences>(personContactDetails.FirstOrDefault());
    }

    /// <summary>
    /// Asynchronously retrieves the contact preferences for multiple persons based on their national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object, where <see cref="IPersonContactPreferencesLookupResult"/> represents the successful lookup result and <see cref="bool"/> indicates a failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="nationalIdentityNumbers"/> is <c>null</c>.</exception>
    public async Task<Result<IPersonContactPreferencesLookupResult, bool>> GetContactPreferencesAsync(IEnumerable<string> nationalIdentityNumbers)
    {
        ArgumentNullException.ThrowIfNull(nationalIdentityNumbers);

        var validNationalIdentityNumbers = _nationalIdentityNumberChecker.GetValid(nationalIdentityNumbers);

        var matchedContactDetails = await _personRepository.GetContactDetailsAsync(validNationalIdentityNumbers);

        var matchedNationalIdentityNumbers = matchedContactDetails != null ? new HashSet<string>(matchedContactDetails.Select(e => e.FnumberAk)) : [];

        var unmatchedNationalIdentityNumbers = nationalIdentityNumbers.Where(e => !matchedNationalIdentityNumbers.Contains(e)).ToImmutableList();

        var matchedPersonContactDetails = matchedContactDetails != null ? matchedContactDetails.Select(_mapper.Map<IPersonContactPreferences>).ToImmutableList() : [];

        return new PersonContactPreferencesLookupResult
        {
            MatchedPersonContactPreferences = matchedPersonContactDetails,
            UnmatchedNationalIdentityNumbers = unmatchedNationalIdentityNumbers
        };
    }
}
