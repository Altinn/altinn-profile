#nullable enable

using Altinn.Profile.Integrations.Entities;
using Altinn.Profile.Integrations.Persistence;

using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Integrations.Repositories;

/// <summary>
/// Repository for handling register data.
/// </summary>
/// <seealso cref="IRegisterRepository" />
internal class RegisterRepository : ProfileRepository<Register>, IRegisterRepository
{
    private readonly ProfileDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterRepository"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="context"/> object is null.</exception>
    public RegisterRepository(ProfileDbContext context) : base(context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Asynchronously retrieves the contact info for multiple users based on the provided national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">A collection of national identity numbers to filter the user contact points.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of user contact points.</returns>
    public async Task<IEnumerable<Register>> GetUserContactInfoAsync(IEnumerable<string> nationalIdentityNumbers)
    {
        return await _context.Registers.Where(k => nationalIdentityNumbers.Contains(k.FnumberAk)).ToListAsync();
    }
}
