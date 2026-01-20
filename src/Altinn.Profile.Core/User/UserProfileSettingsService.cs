using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Models;

namespace Altinn.Profile.Core.User;

/// <summary>
/// Implementation of <see cref="IUserProfileSettingsService"/> that uses <see cref="IProfileSettingsRepository"/> to fetch user profiles.
/// </summary>
public class UserProfileSettingsService : IUserProfileSettingsService
{
    private readonly IProfileSettingsRepository _profileSettingsRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserProfileService"/> class.
    /// </summary>
    /// <param name="profileSettingsRepository">The profile settings repository available through DI</param>
    public UserProfileSettingsService(IProfileSettingsRepository profileSettingsRepository)
    {
        _profileSettingsRepository = profileSettingsRepository;
    }

    /// <inheritdoc/>
    public async Task<ProfileSettings.ProfileSettings?> GetProfileSettings(int userId)
    {
        return await _profileSettingsRepository.GetProfileSettings(userId);
    }

    /// <inheritdoc/>
    public async Task<ProfileSettings.ProfileSettings> UpdateProfileSettings(ProfileSettings.ProfileSettings profileSettings, CancellationToken cancellationToken)
    {
        return await _profileSettingsRepository.UpdateProfileSettings(profileSettings, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ProfileSettings.ProfileSettings?> PatchProfileSettings(ProfileSettingsPatchModel profileSettings, CancellationToken cancellationToken)
    {
        return await _profileSettingsRepository.PatchProfileSettings(profileSettings, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<UserProfile> EnrichWithProfileSettings(UserProfile userProfile)
    {
        ProfileSettings.ProfileSettings? profileSettings = await _profileSettingsRepository.GetProfileSettings(userProfile.UserId);
        if (profileSettings != null)
        {
            userProfile.ProfileSettingPreference.DoNotPromptForParty = profileSettings.DoNotPromptForParty;
            userProfile.ProfileSettingPreference.Language = profileSettings.LanguageType;
            userProfile.ProfileSettingPreference.PreselectedPartyUuid = profileSettings.PreselectedPartyUuid;
            userProfile.ProfileSettingPreference.ShowClientUnits = profileSettings.ShowClientUnits;
            userProfile.ProfileSettingPreference.ShouldShowSubEntities = profileSettings.ShouldShowSubEntities;
            userProfile.ProfileSettingPreference.ShouldShowDeletedEntities = profileSettings.ShouldShowDeletedEntities;
        }

        return userProfile;
    }
}
