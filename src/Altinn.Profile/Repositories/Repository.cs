using System.Threading.Tasks;

using Altinn.Profile.Context;

using Microsoft.EntityFrameworkCore;

namespace Altinn.Profile.Repositories;

/// <summary>
/// Generic repository for handling data access operations.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
/// <seealso cref="IRepository{T}" />
public class Repository<T> : IRepository<T>
    where T : class
{
    private readonly ProfileDbContext _context;
    private readonly DbSet<T> _dbSet;

    /// <summary>
    /// Initializes a new instance of the <see cref="Repository{T}"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    public Repository(ProfileDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    /// <inheritdoc/>
    public async Task<T> GetAsync()
    {
        return await _dbSet.FirstAsync();
    }
}
