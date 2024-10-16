#nullable enable

using System.Collections.Immutable;

using Altinn.Profile.Core;
using Altinn.Profile.Core.ContactRegister;
using Altinn.Profile.Core.Person.ContactPreferences;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Persistence;

using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Npgsql;

namespace Altinn.Profile.Integrations.Repositories;

/// <summary>
/// Defines a repository for handling person data operations.
/// </summary>
/// <seealso cref="IPersonRepository" />
internal class PersonRepository : IPersonRepository
{
    private readonly IMapper _mapper;
    private readonly ProfileDbContext _context;
    private readonly ILogger<PersonRepository> _logger;
    private readonly IDbContextFactory<ProfileDbContext> _contextFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="PersonRepository"/> class.
    /// </summary>
    /// <param name="logger">The logger instance used for logging operations.</param>
    /// <param name="mapper">The mapper instance used for object-object mapping.</param>
    /// <param name="context">The database context for accessing profile data.</param>
    /// <param name="contextFactory">The factory for creating database context instances.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the <paramref name="logger"/>, <paramref name="mapper"/>, <paramref name="context"/>, or <paramref name="contextFactory"/> is null.
    /// </exception>
    public PersonRepository(ILogger<PersonRepository> logger, IMapper mapper, ProfileDbContext context, IDbContextFactory<ProfileDbContext> contextFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    }

    /// <summary>
    /// Asynchronously retrieves the contact details for multiple persons by their national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers to look up.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an <see cref="ImmutableList{T}"/> of <see cref="Person"/> objects representing the contact details of the persons.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="nationalIdentityNumbers"/> is null.</exception>
    public async Task<Result<ImmutableList<Person>, bool>> GetContactDetailsAsync(IEnumerable<string> nationalIdentityNumbers)
    {
        ArgumentNullException.ThrowIfNull(nationalIdentityNumbers);

        try
        {
            if (!nationalIdentityNumbers.Any())
            {
                return ImmutableList<Person>.Empty;
            }

            var people = await _context.People.Where(e => nationalIdentityNumbers.Contains(e.FnumberAk)).ToListAsync();
            return people.ToImmutableList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving contact details for the provided national identity numbers.");

            throw;
        }
    }

    /// <summary>
    /// Asynchronously synchronizes the person contact preferences.
    /// </summary>
    /// <param name="personContactPreferencesSnapshots">The person contact preferences snapshots.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating success or failure.</returns>
    public async Task<Result<int, bool>> SyncPersonContactPreferencesAsync(IContactRegisterChangesLog personContactPreferencesSnapshots)
    {
        ArgumentNullException.ThrowIfNull(personContactPreferencesSnapshots);
        ArgumentNullException.ThrowIfNull(personContactPreferencesSnapshots.ContactPreferencesSnapshots);

        var distinctContactPreferences = GetDistinctContactPreferences(personContactPreferencesSnapshots.ContactPreferencesSnapshots);

        // Add or update the people in the database
        using var context = _contextFactory.CreateDbContext();
        foreach (var contactPreference in distinctContactPreferences)
        {
            var person = _mapper.Map<Person>(contactPreference);

            try
            {
                var existingPerson = await context.People.FirstOrDefaultAsync(e => e.FnumberAk == person.FnumberAk);

                if (existingPerson != null)
                {
                    existingPerson.EmailAddress = person.EmailAddress;
                    existingPerson.MobilePhoneNumber = person.MobilePhoneNumber;
                    context.People.Update(existingPerson);
                }
                else
                {
                    await context.People.AddAsync(person);
                }
            }
            catch (DbUpdateException updateException) when (updateException.InnerException is PostgresException postgresException && postgresException.SqlState == "23505")
            {
                _logger.LogError(updateException, "An error occurred while synchronizing person contact preferences.");

                throw;
            }
        }

        try
        {
            var existingMetadata = await context.Metadata.FirstOrDefaultAsync();
            if (existingMetadata != null)
            {
                context.Metadata.Remove(existingMetadata);
            }

            var metadata = new Metadata
            {
                Exported = DateTime.Now.ToUniversalTime(),
                LatestChangeNumber = personContactPreferencesSnapshots.EndingIdentifier ?? 0
            };
            await context.Metadata.AddAsync(metadata);

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while synchronizing person contact preferences.");

            throw;
        }

        return 0;
    }

    /// <summary>
    /// Finds distinct contact preferences by selecting the item with the largest values for the specified properties.
    /// </summary>
    /// <param name="contactPreferencesSnapshots">The collection of contact preferences snapshots.</param>
    /// <returns>A list of distinct contact preferences.</returns>
    private static List<PersonContactPreferencesSnapshot> GetDistinctContactPreferences(IEnumerable<PersonContactPreferencesSnapshot> contactPreferencesSnapshots)
    {
        return contactPreferencesSnapshots.GroupBy(p => p.PersonIdentifier)
                                          .Select(g => g.OrderByDescending(p => p.ContactDetailsSnapshot?.EmailLastUpdated)
                                                        .ThenByDescending(p => p.ContactDetailsSnapshot?.MobileNumberLastUpdated)
                                                        .ThenByDescending(p => p.ContactDetailsSnapshot?.EmailLastVerified)
                                                        .ThenByDescending(p => p.ContactDetailsSnapshot?.MobileNumberLastVerified)
                                                        .ThenByDescending(p => p.LanguageLastUpdated)
                                                        .First())
                                          .ToList();
    }
}
