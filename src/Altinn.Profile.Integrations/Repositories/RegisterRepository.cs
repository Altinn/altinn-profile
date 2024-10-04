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
    /// Asynchronously retrieves the register data for multiple users by their national identity numbers.
    /// </summary>
    /// <param name="nationalIdentityNumbers">The collection of national identity numbers.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a collection of register data for the users.
    /// </returns>
    public async Task<IEnumerable<Register>> GetUserContactInfoAsync(IEnumerable<string> nationalIdentityNumbers)
    {
        return await _context.Registers.Where(k => nationalIdentityNumbers.Contains(k.FnumberAk)).ToListAsync();
    }
}
