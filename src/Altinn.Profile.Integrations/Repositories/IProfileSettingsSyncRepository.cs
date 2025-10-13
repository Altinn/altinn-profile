using Altinn.Profile.Core.User.ProfileSettings;

namespace Altinn.Profile.Integrations.Repositories
{
    /// <summary>
    /// Defines methods for synchronizing profile settings with Altinn2
    /// </summary>
    /// <remarks>Can be removed when Altinn2 is decommissioned</remarks>
    public interface IProfileSettingsSyncRepository
    {
        /// <summary>
        /// Updates the profile settings for a user.
        /// </summary>
        /// <param name="profileSettings">The profile settings to update.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task UpdateProfileSettings(ProfileSettings profileSettings);
    }
}
