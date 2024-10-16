#nullable enable

using System.Collections.Immutable;
using Altinn.Profile.Core.ContactRegister;
using Altinn.Profile.Core.Person.ContactPreferences;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Persistence;

using AutoMapper;

using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace Altinn.Profile.Integrations.Repositories;

/// <summary>
/// Defines a repository for handling person data operations.
/// </summary>
/// <seealso cref="IPersonRepository" />
internal class PersonRepository : IPersonRepository
{
    private readonly ProfileDbContext _context;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonRepository"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="context"/> object is null.</exception>
    public PersonRepository(ProfileDbContext context, IMapper mapper)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
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
    public async Task<bool> SyncPersonContactPreferencesAsync(IContactRegisterChangesLog personContactPreferencesSnapshots)
    {
        ArgumentNullException.ThrowIfNull(personContactPreferencesSnapshots);
        ArgumentNullException.ThrowIfNull(personContactPreferencesSnapshots.ContactPreferencesSnapshots);

        var people = _mapper.Map<List<PersonContactPreferencesSnapshot>>(personContactPreferencesSnapshots.ContactPreferencesSnapshots);

        // Find duplicates and select the item with the largest values for the specified properties
        var distinctPeople = people.GroupBy(p => p.PersonIdentifier)
                                   .Select(g => g.OrderByDescending(p => p.ContactDetailsSnapshot?.EmailLastUpdated)
                                                 .ThenByDescending(p => p.ContactDetailsSnapshot?.MobileNumberLastUpdated)
                                                 .ThenByDescending(p => p.ContactDetailsSnapshot?.EmailLastVerified)
                                                 .ThenByDescending(p => p.ContactDetailsSnapshot?.MobileNumberLastVerified)
                                                 .ThenByDescending(p => p.LanguageLastUpdated)
                                                 .First())
                                   .ToList();

        // Add or update the people in the database
        foreach (var snapshot in distinctPeople)
        {
            var person = _mapper.Map<Person>(snapshot);

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
                LatestChangeNumber = personContactPreferencesSnapshots.EndingIdentifier ?? existingMetadata?.LatestChangeNumber ?? 0
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
