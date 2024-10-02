using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Altinn.Profile.Context;
using Altinn.Profile.Models;

using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Repositories;

/// <summary>
/// Register Repository for handling register data
/// </summary>
/// <seealso cref="IRegisterRepository" />
public class RegisterRepository : Repository<Register>, IRegisterRepository
{
    private ProfileDbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterRepository"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    public RegisterRepository(ProfileDbContext context) : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// Asynchronously retrieves a collection of user contact points based on the provided social security numbers.
    /// </summary>
    /// <param name="nationalIdentityNumber">A collection of social security numbers to filter the user contact points.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of user contact points.</returns>
    public async Task<Register> GetUserContactPointAsync(string nationalIdentityNumber)
    {
        return await _context.Registers.SingleOrDefaultAsync(k => k.FnumberAk == nationalIdentityNumber);
    }

    /// <summary>
    /// Asynchronously retrieves a collection of user contact points based on the provided social security numbers.
    /// </summary>
    /// <param name="socialSecurityNumbers">A collection of social security numbers to filter the user contact points.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of user contact points.</returns>
    public async Task<IEnumerable<Register>> GetUserContactPointAsync(IEnumerable<string> socialSecurityNumbers)
    {
        return await _context.Registers.Where(k => socialSecurityNumbers.Contains(k.FnumberAk)).ToListAsync();
    }
}
