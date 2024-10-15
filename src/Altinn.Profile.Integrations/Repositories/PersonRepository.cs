#nullable enable

using System.Collections.Immutable;

using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories;

/// <summary>
/// Defines a repository for handling person data operations.
/// </summary>
/// <seealso cref="IPersonRepository" />
internal class PersonRepository : ProfileRepository<Person>, IPersonRepository
{
    private readonly ProfileDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonRepository"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="context"/> object is null.</exception>
    public PersonRepository(ProfileDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Asynchronously retrieves the contact details for multiple persons by their national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers to look up for.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an <see cref="ImmutableList{T}"/> of <see cref="Person"/> objects representing the contact details of the persons.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="nationalIdentityNumbers"/> is null.</exception>
    public async Task<ImmutableList<Person>> GetContactDetailsAsync(IEnumerable<string> nationalIdentityNumbers)
    {
        ArgumentNullException.ThrowIfNull(nationalIdentityNumbers);

        if (!nationalIdentityNumbers.Any())
        {
            return [];
        }

        var people = await _context.People.Where(e => nationalIdentityNumbers.Contains(e.FnumberAk)).ToListAsync();

        return [.. people];
    }

    /// <summary>
    /// Gets the last c hanged number.
    /// </summary>
    /// <returns></returns>
    public async Task<long> GetLastCHangedNumber()
    {
        var metaData = await _context.Metadata.FirstOrDefaultAsync();

        if (metaData != null)
        {
            return metaData.LatestChangeNumber;
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// Synchronizes the person contact preferences asynchronous.
    /// </summary>
    /// <param name="personContactPreferencesSnapshots">The person contact preferences snapshots.</param>
    /// <returns></returns>
    public async Task<bool> SyncPersonContactPreferencesAsync(IEnumerable<IPersonContactPreferencesSnapshot> personContactPreferencesSnapshots)
    {
        ArgumentNullException.ThrowIfNull(personContactPreferencesSnapshots);

        // Convert the personContactPreferencesSnapshots to list of Persons
        var people = personContactPreferencesSnapshots.Select(p => new Person
        {
            LanguageCode = p.Language,
            FnumberAk = p.PersonIdentifier,
            Reservation = p.Reservation == "JA" ? true : false,
            EmailAddress = p.PersonContactDetailsSnapshot?.EmailAddress,
            MobilePhoneNumber = p.PersonContactDetailsSnapshot?.MobilePhoneNumber,
            EmailAddressLastUpdated = p.PersonContactDetailsSnapshot?.EmailAddressUpdated?.ToUniversalTime(),
            EmailAddressLastVerified = p.PersonContactDetailsSnapshot?.EmailAddressLastVerified?.ToUniversalTime(),
            MobilePhoneNumberLastUpdated = p.PersonContactDetailsSnapshot?.MobilePhoneNumberUpdated?.ToUniversalTime(),
            MobilePhoneNumberLastVerified = p.PersonContactDetailsSnapshot?.MobilePhoneNumberLastVerified?.ToUniversalTime(),
        }).ToList();

        // Add or update the people in the database
        var lastIdentifier = string.Empty;
        for (int i = 0; i < people.Count; i++)
        {
            Person? person = people[i];
            try
            {
                var existingPerson = await _context.People
                    .FirstOrDefaultAsync(e => e.FnumberAk == person.FnumberAk);

                if (existingPerson != null)
                {
                    existingPerson.EmailAddress = person.EmailAddress;
                    existingPerson.MobilePhoneNumber = person.MobilePhoneNumber;
                    _context.People.Update(existingPerson);
                }
                else
                {
                    await _context.People.AddAsync(person);
                }

                if (i == 999)
                {
                    lastIdentifier = person.FnumberAk;
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        try
        {
            var metaData = _context.Metadata.FirstOrDefault();

            if (metaData != null)
            {
                //metaData.LatestChangeNumber = Convert.ToInt64(lastIdentifier);
                _context.Metadata.Update(metaData);
            }
            else
            {
                metaData = new Metadata
                {
                    Exported = DateTime.Now.ToUniversalTime(),
                    LatestChangeNumber = Convert.ToInt64(lastIdentifier)
                };
                await _context.Metadata.AddAsync(metaData);
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw;
        }

        return true;
    }
}
