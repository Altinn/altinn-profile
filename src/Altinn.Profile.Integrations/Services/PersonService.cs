#nullable enable

using System.Collections.Immutable;

using Altinn.Profile.Core;
using Altinn.Profile.Core.Person.ContactPreferences;
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
    private readonly IMetadataRepository _metadataRepository;
    private readonly INationalIdentityNumberChecker _nationalIdentityNumberChecker;

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonService"/> class.
    /// </summary>
    /// <param name="mapper">The objects mapper.</param>
    /// <param name="personRepository">The repository used for accessing the person data.</param>
    /// <param name="metadataRepository">The repository used for accessing metadata.</param>
    /// <param name="nationalIdentityNumberChecker">The service used for checking the validity of national identity numbers.</param>
    public PersonService(
        IMapper mapper, 
        IPersonRepository personRepository, 
        IMetadataRepository metadataRepository, 
        INationalIdentityNumberChecker nationalIdentityNumberChecker)
    {
        _mapper = mapper;
        _personRepository = personRepository;
        _metadataRepository = metadataRepository;
        _nationalIdentityNumberChecker = nationalIdentityNumberChecker;
    }

    /// <summary>
    /// Asynchronously retrieves the contact preferences for multiple persons based on their national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> 
    /// object, where <see cref="IPersonContactPreferencesLookupResult"/> represents the successful lookup result and <see cref="bool"/> indicates a failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="nationalIdentityNumbers"/> is <c>null</c>.</exception>
    public async Task<Result<IPersonContactPreferencesLookupResult, bool>> GetContactPreferencesAsync(IEnumerable<string> nationalIdentityNumbers)
    {
        ArgumentNullException.ThrowIfNull(nationalIdentityNumbers);

        var validNationalIdentityNumbers = _nationalIdentityNumberChecker.GetValid(nationalIdentityNumbers);
        if (validNationalIdentityNumbers == null || validNationalIdentityNumbers.Count == 0)
        {
            return false;
        }

        Result<ImmutableList<Person>, bool> matchedContactDetails = await _personRepository.GetContactDetailsAsync(validNationalIdentityNumbers);

        HashSet<string> matchedNationalIdentityNumbers = [];
        IEnumerable<string> unmatchedNationalIdentityNumbers = [];
        IEnumerable<PersonContactPreferences> matchedPersonContactDetails = [];

        matchedContactDetails.Match(
            e =>
            {
                if (e is not null && e.Count > 0)
                {
                    matchedNationalIdentityNumbers = new HashSet<string>(e.Select(e => e.FnumberAk));
                    matchedPersonContactDetails = e.Select(_mapper.Map<PersonContactPreferences>).ToImmutableList();
                    unmatchedNationalIdentityNumbers = nationalIdentityNumbers.Where(e => !matchedNationalIdentityNumbers.Contains(e));
                }
            },
            _ => { });

        return new PersonContactPreferencesLookupResult
        {
            MatchedPersonContactPreferences = matchedPersonContactDetails.Any() ? matchedPersonContactDetails.ToImmutableList() : null,
            UnmatchedNationalIdentityNumbers = unmatchedNationalIdentityNumbers.Any() ? unmatchedNationalIdentityNumbers.ToImmutableList() : null
        };
    }
}
