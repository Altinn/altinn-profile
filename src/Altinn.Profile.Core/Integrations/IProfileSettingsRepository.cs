using Altinn.Profile.Core.User.ProfileSettings;

namespace Altinn.Profile.Core.Integrations
{
    /// <summary>
    /// Provides methods for updating user profile settings in the repository.
    /// </summary>
    public interface IProfileSettingsRepository
    {
        /// <summary>
        /// Updates the profile settings for a user.
        /// </summary>
        /// <param name="profileSettings">The profile settings to update.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task<ProfileSettings> UpdateProfileSettings(ProfileSettings profileSettings, CancellationToken cancellationToken);

        /// <summary>
        /// Patches the profile settings for a user.
        /// </summary>
        /// <param name="profileSettings">The profile settings to update.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task<ProfileSettings?> PatchProfileSettings(ProfileSettingsPatchModel profileSettings, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves the profile settings for a given user ID.
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<ProfileSettings?> GetProfileSettings(int userId, CancellationToken cancellationToken);
    }
}
