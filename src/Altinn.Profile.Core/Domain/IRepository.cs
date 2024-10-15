#nullable enable

namespace Altinn.Profile.Core.Domain;

/// <summary>
/// Defines generic methods for handling entities in a repository.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
public interface IRepository<T>
    where T : class
{
    /// <summary>
    /// Asynchronously retrieves all entities.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of entities.</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Asynchronously retrieves entities with optional filtering, sorting, and pagination.
    /// </summary>
    /// <param name="filter">Optional filter criteria.</param>
    /// <param name="orderBy">Optional ordering criteria.</param>
    /// <param name="skip">The number of entities to skip.</param>
    /// <param name="take">The number of entities to take.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a collection of filtered, sorted, and paginated entities.</returns>
    Task<IEnumerable<T>> GetAsync(
        Func<T, bool>? filter = null,
        Func<IEnumerable<T>, IOrderedEnumerable<T>>? orderBy = null,
        int? skip = null,
        int? take = null);

    /// <summary>
    /// Asynchronously retrieves an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the entity that matches the identifier.</returns>
    Task<T?> GetByIdAsync(string id);

    /// <summary>
    /// Asynchronously checks if an entity exists by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating the existence of the entity.</returns>
    Task<bool> ExistsAsync(string id);

    /// <summary>
    /// Asynchronously adds a new entity.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the added entity.</returns>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Asynchronously adds multiple entities.
    /// </summary>
    /// <param name="entities">The entities to add.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task AddRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Asynchronously updates an existing entity.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Asynchronously updates multiple entities.
    /// </summary>
    /// <param name="entities">The entities to update.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task UpdateRangeAsync(IEnumerable<T> entities);

    /// <summary>
    /// Asynchronously deletes an entity by its identifier.
    /// </summary>
    /// <param name="id">The identifier of the entity to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteAsync(string id);

    /// <summary>
    /// Asynchronously deletes multiple entities by their identifiers.
    /// </summary>
    /// <param name="ids">The identifiers of the entities to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task DeleteRangeAsync(IEnumerable<string> ids);

    /// <summary>
    /// Saves changes to the data source asynchronously.
    /// </summary>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the data source.</returns>
    Task<int> SaveChangesAsync();
}
