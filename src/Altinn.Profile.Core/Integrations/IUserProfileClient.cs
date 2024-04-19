using Altinn.Platform.Profile.Models;

namespace Altinn.Profile.Core.Integrations
{
    /// <summary>
    /// Interface describing a client for the user profile service
    /// </summary>
    public interface IUserProfileClient
    {
        /// <summary>
        /// Method that fetches a user based on a user id
        /// </summary>
        /// <param name="userId">The user id</param>
        /// <returns>User profile with given user id or a boolean if failure.</returns>
        Task<Result<UserProfile, bool>> GetUser(int userId);

        /// <summary>
        /// Method that fetches a user based on ssn.
        /// </summary>
        /// <param name="ssn">The user's ssn.</param>
        /// <returns>User profile connected to given ssn or a boolean if failure.</returns>
        Task<Result<UserProfile, bool>> GetUser(string ssn);

        /// <summary>
        /// Method that fetches a user based on a user uuid
        /// </summary>
        /// <param name="userUuid">The user uuid</param>
        /// <returns>User profile with given user uuid or a boolean if failure.</returns>
        Task<Result<UserProfile, bool>> GetUserByUuid(Guid userUuid);

        /// <summary>
        /// Method that fetches a list of users based on a list of user uuid
        /// </summary>
        /// <param name="userUuidList">The list of user uuids</param>
        /// <returns>List of User profiles with given user uuids or a boolean if failure.</returns>
        Task<Result<List<UserProfile>, bool>> GetUserListByUuid(List<Guid> userUuidList);

        /// <summary>
        /// Method that fetches a user based on username.
        /// </summary>
        /// <param name="username">The user's username.</param>
        /// <returns>User profile connected to given username or a boolean if failure.</returns>
        Task<Result<UserProfile, bool>> GetUserByUsername(string username);
    }
}
