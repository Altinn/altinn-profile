using Altinn.Profile.Core.Integrations;

namespace Altinn.Profile.Core.User
{
    /// <summary>
    /// Implementation of <see cref="IUserProfileService"/> that uses <see cref="IUserProfileClient"/> to fetch user profiles."/>
    /// </summary>
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileClient _userProfileClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserProfileService"/> class.
        /// </summary>
        /// <param name="userProfileClient">The user profile client available through DI</param>
        public UserProfileService(IUserProfileClient userProfileClient)
        {
            _userProfileClient = userProfileClient;
        }

        /// <inheritdoc/>
        public async Task<Platform.Profile.Models.UserProfile> GetUser(int userId)
        {
            return await _userProfileClient.GetUser(userId);
        }

        /// <inheritdoc/>
        public async Task<Platform.Profile.Models.UserProfile> GetUser(string ssn)
        {
            return await _userProfileClient.GetUser(ssn);
        }

        /// <inheritdoc/>
        public async Task<Platform.Profile.Models.UserProfile> GetUserByUsername(string username)
        {
            return await _userProfileClient.GetUserByUsername(username);
        }

        /// <inheritdoc/>
        public async Task<Platform.Profile.Models.UserProfile> GetUserByUuid(Guid userUuid)
        {
            return await _userProfileClient.GetUserByUuid(userUuid);
        }

        /// <inheritdoc/>
        public async Task<List<Platform.Profile.Models.UserProfile>> GetUserListByUuid(List<Guid> userUuidList)
        {
            return await _userProfileClient.GetUserListByUuid(userUuidList);
        }
    }
}
