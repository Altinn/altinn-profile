using System.Threading.Tasks;

namespace Altinn.Profile.Repositories;

/// <summary>
/// Defines methods for handling entities in a repository.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public interface IRepository<T>
    where T : class
{
    /// <summary>
    /// Asynchronously retrieves entities.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the entity that matches the social security number.</returns>
    Task<T> GetAsync();
}
