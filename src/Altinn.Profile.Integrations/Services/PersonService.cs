#nullable enable

using System.Collections.Immutable;

using Altinn.Profile.Core;
using Altinn.Profile.Core.ContactRegister;
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
    private readonly IContactRegisterService _changesLogService;
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
    public PersonService(IMapper mapper, IPersonRepository personRepository, IMetadataRepository metadataRepository, INationalIdentityNumberChecker nationalIdentityNumberChecker, IContactRegisterService changesLogService)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _personRepository = personRepository ?? throw new ArgumentNullException(nameof(personRepository));
        _metadataRepository = metadataRepository ?? throw new ArgumentNullException(nameof(metadataRepository));
        _changesLogService = changesLogService ?? throw new ArgumentNullException(nameof(changesLogService));
        _nationalIdentityNumberChecker = nationalIdentityNumberChecker ?? throw new ArgumentNullException(nameof(nationalIdentityNumberChecker));
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

        HashSet<string>? matchedNationalIdentityNumbers;
        IEnumerable<string>? unmatchedNationalIdentityNumbers = null;
        IEnumerable<PersonContactPreferences>? matchedPersonContactDetails = null;

        matchedContactDetails.Match(
            e =>
            {
                matchedNationalIdentityNumbers = new HashSet<string>(e.Select(e => e.FnumberAk));
                matchedPersonContactDetails = e.Select(_mapper.Map<PersonContactPreferences>).ToImmutableList();
                unmatchedNationalIdentityNumbers = nationalIdentityNumbers.Where(e => !matchedNationalIdentityNumbers.Contains(e));
            },
            _ =>
            {
                matchedPersonContactDetails = [];
                matchedNationalIdentityNumbers = [];
                unmatchedNationalIdentityNumbers = [];
            });

        return new PersonContactPreferencesLookupResult
        {
            MatchedPersonContactPreferences = matchedPersonContactDetails?.ToImmutableList(),
            UnmatchedNationalIdentityNumbers = unmatchedNationalIdentityNumbers?.ToImmutableList()
        };
    }

    /// <summary>
    /// Asynchronously synchronizes the person contact preferences.
    /// </summary>
    public async void SyncPersonContactPreferencesAsync()
    {
        long latestChangeNumber = 0;
        var latestChangeNumberGetter = await _metadataRepository.GetLatestChangeNumberAsync();
        latestChangeNumberGetter.Match(e => latestChangeNumber = e, x => latestChangeNumber = 0);

        var changes = await _changesLogService.RetrieveContactDetailsChangesAsync(latestChangeNumber);

        changes.Match(
            async e =>
            {
                await _personRepository.SyncPersonContactPreferencesAsync(e);
            },
            _ =>
            {
            });
    }
}
