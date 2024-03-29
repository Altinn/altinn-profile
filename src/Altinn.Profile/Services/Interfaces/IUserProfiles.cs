using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Altinn.Platform.Profile.Models;

namespace Altinn.Profile.Services.Interfaces
{
    /// <summary>
    /// Interface handling methods for operations related to users
    /// </summary>
    public interface IUserProfiles
    {
        /// <summary>
        /// Method that fetches a user based on a user id
        /// </summary>
        /// <param name="userId">The user id</param>
        /// <returns>User profile with given user id.</returns>
        Task<UserProfile> GetUser(int userId);

        /// <summary>
        /// Method that fetches a user based on ssn.
        /// </summary>
        /// <param name="ssn">The user's ssn.</param>
        /// <returns>User profile connected to given ssn.</returns>
        Task<UserProfile> GetUser(string ssn);

        /// <summary>
        /// Method that fetches a user based on a user uuid
        /// </summary>
        /// <param name="userUuid">The user uuid</param>
        /// <returns>User profile with given user uuid.</returns>
        Task<UserProfile> GetUserByUuid(Guid userUuid);

        /// <summary>
        /// Method that fetches a list of users based on a list of user uuid
        /// </summary>
        /// <param name="userUuidList">The list of user uuids</param>
        /// <returns>List of User profiles with given user uuids</returns>
        Task<List<UserProfile>> GetUserListByUuid(List<Guid> userUuidList);

        /// <summary>
        /// Method that fetches a user based on username.
        /// </summary>
        /// <param name="username">The user's username.</param>
        /// <returns>User profile connected to given username.</returns>
        Task<UserProfile> GetUserByUsername(string username);
    }
}
