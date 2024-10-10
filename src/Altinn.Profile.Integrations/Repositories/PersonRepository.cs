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
}
