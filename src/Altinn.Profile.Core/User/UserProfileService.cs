using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Models;

namespace Altinn.Profile.Core.User;

/// <summary>
/// Implementation of <see cref="IUserProfileService"/> that uses <see cref="IUserProfileClient"/> to fetch user profiles.
/// </summary>
public class UserProfileService : IUserProfileService, IUserProfileSettingsService
{
    private readonly IUserProfileClient _userProfileClient;
    private readonly IProfileSettingsRepository _profileSettingsRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserProfileService"/> class.
    /// </summary>
    /// <param name="userProfileClient">The user profile client available through DI</param>
    /// <param name="profileSettingsRepository">The profile settings repository available through DI</param>
    public UserProfileService(IUserProfileClient userProfileClient, IProfileSettingsRepository profileSettingsRepository)
    {
        _userProfileClient = userProfileClient;
        _profileSettingsRepository = profileSettingsRepository;
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUser(int userId)
    {
        var result = await _userProfileClient.GetUser(userId);

        if (result.IsSuccess)
        {
            var enriched = await EnrichWithProfileSettings(result.Match(userProfile => userProfile, _ => default!));
            return enriched;
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUser(string ssn)
    {
        var result = await _userProfileClient.GetUser(ssn);
        if (result.IsSuccess)
        {
            var enriched = await EnrichWithProfileSettings(result.Match(userProfile => userProfile, _ => default!));
            return enriched;
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUserByUsername(string username)
    {
        var result = await _userProfileClient.GetUserByUsername(username);
        if (result.IsSuccess)
        {
            var enriched = await EnrichWithProfileSettings(result.Match(userProfile => userProfile, _ => default!));
            return enriched;
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUserByUuid(Guid userUuid)
    {
        var result = await _userProfileClient.GetUserByUuid(userUuid);
        if (result.IsSuccess)
        {
            var enriched = await EnrichWithProfileSettings(result.Match(userProfile => userProfile, _ => default!));
            return enriched;
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task<Result<List<UserProfile>, bool>> GetUserListByUuid(List<Guid> userUuidList)
    {
        var result = await _userProfileClient.GetUserListByUuid(userUuidList);
        if (result.IsSuccess)
        {
            var userProfiles = result.Match(profiles => profiles, _ => []);
            var enriched = new List<UserProfile>();
            foreach (UserProfile userProfile in userProfiles)
            {
                enriched.Add(await EnrichWithProfileSettings(userProfile));
            }

            return enriched;
        }

        return false;
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
