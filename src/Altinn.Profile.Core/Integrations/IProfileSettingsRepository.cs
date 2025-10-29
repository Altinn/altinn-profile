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
        /// Gets the local profile settings for a given user ID.
        /// </summary>
        Task<ProfileSettings?> GetProfileSettings(int userId);
    }
}
