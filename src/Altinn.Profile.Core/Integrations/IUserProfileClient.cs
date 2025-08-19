using Altinn.Profile.Models;

namespace Altinn.Profile.Core.Integrations;

/// <summary>
/// Interface for accessing user profile services.
/// </summary>
public interface IUserProfileClient
{
    /// <summary>
    /// Retrieves a user profile based on the user's social security number (SSN).
    /// </summary>
    /// <param name="ssn">The user's social security number.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object with a <see cref="UserProfile"/> on success, or a boolean indicating failure.</returns>
    Task<Result<UserProfile, bool>> GetUser(string ssn);

    /// <summary>
    /// Retrieves a user profile based on the user's ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object with a <see cref="UserProfile"/> on success, or a boolean indicating failure.</returns>
    Task<Result<UserProfile, bool>> GetUser(int userId);

    /// <summary>
    /// Retrieves a user profile based on the user's username.
    /// </summary>
    /// <param name="username">The user's username.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object with a <see cref="UserProfile"/> on success, or a boolean indicating failure.</returns>
    Task<Result<UserProfile, bool>> GetUserByUsername(string username);

    /// <summary>
    /// Retrieves a user profile based on the user's UUID.
    /// </summary>
    /// <param name="userUuid">The user UUID.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object with a <see cref="UserProfile"/> on success, or a boolean indicating failure.</returns>
    Task<Result<UserProfile, bool>> GetUserByUuid(Guid userUuid);

    /// <summary>
    /// Retrieves a list of user profiles based on a list of user UUIDs.
    /// </summary>
    /// <param name="userUuidList">The list of user UUIDs.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object with a list of <see cref="UserProfile"/> on success, or a boolean indicating failure.</returns>
    Task<Result<List<UserProfile>, bool>> GetUserListByUuid(List<Guid> userUuidList);
}
