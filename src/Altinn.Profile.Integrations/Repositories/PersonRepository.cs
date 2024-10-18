#nullable enable

using System.Collections.Immutable;

using Altinn.Profile.Core;
using Altinn.Profile.Core.ContactRegister;
using Altinn.Profile.Core.Person.ContactPreferences;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Persistence;

using AutoMapper;

using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories;

/// <summary>
/// Defines a repository for handling person data operations.
/// </summary>
/// <seealso cref="IPersonRepository" />
internal class PersonRepository : IPersonRepository
{
    private readonly IMapper _mapper;
    private readonly IDbContextFactory<ProfileDbContext> _contextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonRepository"/> class.
    /// </summary>
    /// <param name="mapper">The mapper instance used for object-object mapping.</param>
    /// <param name="contextFactory">The factory for creating database context instances.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the <paramref name="mapper"/>, or <paramref name="contextFactory"/> is null.
    /// </exception>
    public PersonRepository(IMapper mapper, IDbContextFactory<ProfileDbContext> contextFactory)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    /// <summary>
    /// Asynchronously retrieves the contact details for multiple persons by their national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers to look up.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object with an <see cref="ImmutableList{T}"/> of <see cref="Person"/> objects representing the contact details of the persons on success, or a <see cref="bool"/> indicating failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="nationalIdentityNumbers"/> is null.</exception>
    public async Task<Result<ImmutableList<Person>, bool>> GetContactDetailsAsync(IEnumerable<string> nationalIdentityNumbers)
    {
        ArgumentNullException.ThrowIfNull(nationalIdentityNumbers);

        if (!nationalIdentityNumbers.Any())
        {
            return ImmutableList<Person>.Empty;
        }

        using var databaseContext = await _contextFactory.CreateDbContextAsync();

        var people = await databaseContext.People.Where(e => nationalIdentityNumbers.Contains(e.FnumberAk)).ToListAsync();

        return people.ToImmutableList();
    }

    /// <summary>
    /// Asynchronously synchronizes the changes in person contact preferences.
    /// </summary>
    /// <param name="personContactPreferencesSnapshots">The snapshots of person contact preferences to be synchronized.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object with a <see cref="bool"/> indicating success or failure.
    /// </returns>
    public async Task<Result<int, bool>> SyncPersonContactPreferencesAsync(IContactRegisterChangesLog personContactPreferencesSnapshots)
    {
        ArgumentNullException.ThrowIfNull(personContactPreferencesSnapshots);
        ArgumentNullException.ThrowIfNull(personContactPreferencesSnapshots.ContactPreferencesSnapshots);

        var distinctContactPreferences = GetDistinctContactPreferences(personContactPreferencesSnapshots.ContactPreferencesSnapshots);

        using var databaseContext = await _contextFactory.CreateDbContextAsync();

        foreach (var contactPreference in distinctContactPreferences)
        {
            var person = _mapper.Map<Person>(contactPreference);

            var existingPerson = await databaseContext.People.FirstOrDefaultAsync(e => e.FnumberAk.Trim() == person.FnumberAk.Trim());

            if (existingPerson == null)
            {
                await databaseContext.People.AddAsync(person);
            }
            else
            {
                existingPerson.EmailAddress = person.EmailAddress;
                existingPerson.MobilePhoneNumber = person.MobilePhoneNumber;
                databaseContext.People.Update(existingPerson);
            }
        }

        return await databaseContext.SaveChangesAsync();
    }

    /// <summary>
    /// Finds distinct contact preferences by selecting the item with the largest values for the specified properties.
    /// </summary>
    /// <param name="contactPreferencesSnapshots">The collection of contact preferences snapshots.</param>
    /// <returns>A list of distinct contact preferences.</returns>
    private static ImmutableList<PersonContactPreferencesSnapshot> GetDistinctContactPreferences(IEnumerable<PersonContactPreferencesSnapshot> contactPreferencesSnapshots)
    {
        return contactPreferencesSnapshots.GroupBy(p => p.PersonIdentifier)
                                          .Select(g => g.OrderByDescending(p => p.ContactDetailsSnapshot?.EmailLastUpdated)
                                                        .ThenByDescending(p => p.ContactDetailsSnapshot?.MobileNumberLastUpdated)
                                                        .ThenByDescending(p => p.ContactDetailsSnapshot?.EmailLastVerified)
                                                        .ThenByDescending(p => p.ContactDetailsSnapshot?.MobileNumberLastVerified)
                                                        .ThenByDescending(p => p.LanguageLastUpdated)
                                                        .First())
                                          .ToImmutableList();
    }
}
