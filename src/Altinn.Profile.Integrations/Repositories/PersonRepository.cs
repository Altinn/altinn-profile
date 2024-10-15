#nullable enable

using System.Collections.Immutable;

using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Persistence;

using Microsoft.EntityFrameworkCore;

using Npgsql;

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
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers to look up.</param>
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
    /// Asynchronously retrieves the latest change number.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the latest change number.</returns>
    public async Task<long> GetLatestChangeNumberAsync()
    {
        var metaData = await _context.Metadata.FirstOrDefaultAsync();

        return metaData != null ? metaData.LatestChangeNumber : 0;
    }

    /// <summary>
    /// Asynchronously synchronizes the person contact preferences.
    /// </summary>
    /// <param name="personContactPreferencesSnapshots">The person contact preferences snapshots.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
    public async Task<bool> SyncPersonContactPreferencesAsync(IPersonContactPreferencesChangesLog personContactPreferencesSnapshots)
    {
        ArgumentNullException.ThrowIfNull(personContactPreferencesSnapshots);
        ArgumentNullException.ThrowIfNull(personContactPreferencesSnapshots.PersonContactPreferencesSnapshots);

        var people = personContactPreferencesSnapshots.PersonContactPreferencesSnapshots.Select(e => new PersonContactPreferencesSnapshot
        {
            PersonContactDetailsSnapshot = new PersonContactDetailsSnapshot
            {
                EmailAddress = e.PersonContactDetailsSnapshot?.EmailAddress,
                MobilePhoneNumber = e.PersonContactDetailsSnapshot?.MobilePhoneNumber,
                EmailAddressUpdated = e.PersonContactDetailsSnapshot?.EmailAddressUpdated,
                MobilePhoneNumberUpdated = e.PersonContactDetailsSnapshot?.MobilePhoneNumberUpdated,
                IsEmailAddressDuplicated = e.PersonContactDetailsSnapshot?.IsEmailAddressDuplicated,
                EmailAddressLastVerified = e.PersonContactDetailsSnapshot?.EmailAddressLastVerified,
                MobilePhoneNumberLastVerified = e.PersonContactDetailsSnapshot?.MobilePhoneNumberLastVerified,
                IsMobilePhoneNumberDuplicated = e.PersonContactDetailsSnapshot?.IsMobilePhoneNumberDuplicated
            },
            Status = e.Status,
            Language = e.Language,
            Reservation = e.Reservation,
            LanguageUpdated = e.LanguageUpdated,
            PersonIdentifier = e.PersonIdentifier,
            NotificationStatus = e.NotificationStatus
        }).ToList();

        // Find duplicates and select the item with the largest values for the specified properties
        var distinctPeople = people.GroupBy(p => p.PersonIdentifier)
                                   .Select(g => g.OrderByDescending(p => p.PersonContactDetailsSnapshot?.EmailAddressUpdated)
                                                 .ThenByDescending(p => p.PersonContactDetailsSnapshot?.MobilePhoneNumberUpdated)
                                                 .ThenByDescending(p => p.PersonContactDetailsSnapshot?.EmailAddressLastVerified)
                                                 .ThenByDescending(p => p.PersonContactDetailsSnapshot?.MobilePhoneNumberLastVerified)
                                                 .ThenByDescending(p => p.LanguageUpdated)
                                                 .First())
                                   .ToList();

        // Add or update the people in the database
        foreach (Person? person in distinctPeople.Select(p => new Person
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
        }))
        {
            try
            {
                var existingPerson = await _context.People.FirstOrDefaultAsync(e => e.FnumberAk == person.FnumberAk);

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
            }
            catch (DbUpdateException dbEx) when (dbEx.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
            {
                // Handle duplicate key exception
                // Log the exception or take appropriate action
            }
        }

        try
        {
            var existingMetadata = await _context.Metadata.FirstOrDefaultAsync();

            if (existingMetadata != null)
            {
                _context.Metadata.Remove(existingMetadata);
            }

            var metaData = new Metadata
            {
                Exported = DateTime.Now.ToUniversalTime(),
                LatestChangeNumber = personContactPreferencesSnapshots.ToChangeId ?? existingMetadata?.LatestChangeNumber ?? 0
            };
            await _context.Metadata.AddAsync(metaData);

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log the exception or take appropriate action
            throw;
        }

        return true;
    }
}
