using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Models;

namespace Altinn.Profile.Core.User
{
    /// <summary>
    /// Provides services for enriching <see cref="UserProfile"/> instances with profile settings from the repository. Used to separate concerns of which part of the user profile that is cached. 
    /// </summary>
    public interface IUserProfileSettingsService
    {
        /// <summary>
        /// Enriches the given <see cref="UserProfile"/> with profile settings from the repository.
        /// </summary>
        /// <param name="userProfile">The userProfile to enrich.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<UserProfile> EnrichWithProfileSettings(UserProfile userProfile);

        /// <summary>
        /// Updates the profile settings for a user.
        /// </summary>
        /// <param name="profileSettings">The updated profile settings from request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<ProfileSettings.ProfileSettings> UpdateProfileSettings(ProfileSettings.ProfileSettings profileSettings, CancellationToken cancellationToken);

        /// <summary>
        /// Patches the profile settings for a user.
        /// </summary>
        /// <param name="profileSettings">The updated profile settings from request</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task<ProfileSettings.ProfileSettings?> PatchProfileSettings(ProfileSettingsPatchModel profileSettings, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the profile settings for a given user ID.
        /// </summary>
        Task<ProfileSettings.ProfileSettings?> GetProfileSettings(int userId);
    }
}
