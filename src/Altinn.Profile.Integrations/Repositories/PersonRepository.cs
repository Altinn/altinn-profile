#nullable enable

using System.Collections.Immutable;

using Altinn.Profile.Core;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.Person.ContactPreferences;
using Altinn.Profile.Core.Telemetry;
using Altinn.Profile.Integrations.ContactRegister;
using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Mappings;
using Altinn.Profile.Integrations.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories;

/// <summary>
/// Defines a repository for handling person data operations.
/// </summary>
/// <seealso cref="IPersonUpdater" />
/// <seealso cref="IPersonService" />
/// <remarks>
/// Initializes a new instance of the <see cref="PersonRepository"/> class.
/// </remarks>
/// <param name="contextFactory">The factory for creating database context instances.</param>
/// <param name="telemetry">The application <see cref="Telemetry"/> instance.</param>
public class PersonRepository(IDbContextFactory<ProfileDbContext> contextFactory, Telemetry? telemetry)
    : IPersonUpdater, IPersonService
{
    private readonly IDbContextFactory<ProfileDbContext> _contextFactory = contextFactory;
    private readonly Telemetry? _telemetry = telemetry;

    /// <summary>
    /// Asynchronously retrieves the contact details for multiple persons by their national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers to look up.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a an <see cref="ImmutableList{T}"/> of <see cref="PersonContactPreferences"/> objects representing the contact details of the persons.
    /// </returns>
    /// <summary>
    /// Retrieves contact preferences for the specified national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers (matched against Person.FnumberAk).</param>
    /// <param name="cancellationToken">Cancellation token used for database operations.</param>
    /// <returns>An immutable list of PersonContactPreferences for persons whose FnumberAk is in <paramref name="nationalIdentityNumbers"/>; empty if none match.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="nationalIdentityNumbers"/> is null.</exception>
    public async Task<ImmutableList<PersonContactPreferences>> GetContactPreferencesAsync(IEnumerable<string> nationalIdentityNumbers, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(nationalIdentityNumbers);

        if (!nationalIdentityNumbers.Any())
        {
            return [];
        }

        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync(cancellationToken);

        List<Person> people = await databaseContext.People.Where(e => nationalIdentityNumbers.Contains(e.FnumberAk)).ToListAsync(cancellationToken);

        var asContactPreferences = people.Select(PersonContactPreferencesMapper.Map);
        return asContactPreferences.ToImmutableList();
    }

    /// <summary>
    /// Asynchronously synchronizes the changes in person contact preferences.
    /// </summary>
    /// <param name="personContactPreferencesSnapshots">The snapshots of person contact preferences to be synchronized.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object with a <see cref="bool"/> indicating success or failure.
    /// </returns>
    public async Task<int> SyncPersonContactPreferencesAsync(ContactRegisterChangesLog personContactPreferencesSnapshots)
    {
        ArgumentNullException.ThrowIfNull(personContactPreferencesSnapshots);
        ArgumentNullException.ThrowIfNull(personContactPreferencesSnapshots.ContactPreferencesSnapshots);

        ImmutableList<PersonContactPreferencesSnapshot> distinctContactPreferences =
            GetDistinctContactPreferences(personContactPreferencesSnapshots.ContactPreferencesSnapshots);

        using ProfileDbContext databaseContext = await _contextFactory.CreateDbContextAsync();

        foreach (PersonContactPreferencesSnapshot contactPreferenceSnapshot in distinctContactPreferences)
        {
            Person person = PersonMapper.Map(contactPreferenceSnapshot);

            Person? existingPerson = await databaseContext.People.FirstOrDefaultAsync(e => e.FnumberAk == person.FnumberAk);

            if (existingPerson is null)
            {
                await databaseContext.People.AddAsync(person);
                _telemetry?.PersonAdded();
            }
            else
            {
                existingPerson.Reservation = person.Reservation;
                existingPerson.EmailAddress = person.EmailAddress;
                existingPerson.EmailAddressLastUpdated = person.EmailAddressLastUpdated;
                existingPerson.EmailAddressLastVerified = person.EmailAddressLastVerified;
                existingPerson.MobilePhoneNumber = person.MobilePhoneNumber;
                existingPerson.MobilePhoneNumberLastUpdated = person.MobilePhoneNumberLastUpdated;
                existingPerson.MobilePhoneNumberLastVerified = person.MobilePhoneNumberLastVerified;
                existingPerson.LanguageCode = person.LanguageCode;

                databaseContext.People.Update(existingPerson);
                _telemetry?.PersonUpdated();
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
