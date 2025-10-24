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
        /// <returns>A task representing the asynchronous operation.</returns>
        Task<ProfileSettings> UpdateProfileSettings(ProfileSettings profileSettings);

        /// <summary>
        /// Patches the profile settings for a user.
        /// </summary>
        /// <param name="profileSettings">The profile settings to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task<ProfileSettings?> PatchProfileSettings(ProfileSettingsPatchRequest profileSettings);

        /// <summary>
        /// Gets the local profile settings for a given user ID.
        /// </summary>
        Task<ProfileSettings?> GetProfileSettings(int userId);
    }
}
