using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Models;
using Altinn.Register.Contracts;

using Microsoft.Extensions.Options;

using static Altinn.Register.Contracts.PartyUrn;

namespace Altinn.Profile.Core.User;

/// <summary>
/// Implementation of <see cref="IUserProfileService"/> that uses <see cref="IUserProfileClient"/> to fetch user profiles.
/// </summary>
public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileClient _userProfileClient;
    private readonly IProfileSettingsRepository _profileSettingsRepository;
    private readonly IPersonService _personRepository;
    private readonly IRegisterClient _registerClient;
    private readonly CoreSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserProfileService"/> class.
    /// </summary>
    /// <param name="userProfileClient">The user profile client available through DI</param>
    /// <param name="profileSettingsRepository">The profile settings repository available through DI</param>
    /// <param name="personRepository">The person repository available through DI</param>
    /// <param name="registerClient">The register client available through DI</param>
    /// <param name="settings">The core settings available through DI</param>
    public UserProfileService(IUserProfileClient userProfileClient, IProfileSettingsRepository profileSettingsRepository, IPersonService personRepository, IRegisterClient registerClient, IOptionsMonitor<CoreSettings> settings)
    {
        _userProfileClient = userProfileClient;
        _profileSettingsRepository = profileSettingsRepository;
        _personRepository = personRepository;
        _registerClient = registerClient;
        _settings = settings.CurrentValue;
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUser(int userId)
    {
        if (_settings.LookupUsersFromRegister)
        {
            var party = await _registerClient.GetUserParty(userId, default);
            var userProfile = UserProfileMapper.MapFromParty(party);

            if (userProfile != null)
            {
                if (party?.Type == PartyType.Person)
                {
                    userProfile = await EnrichWithProfileSettings(userProfile);
                    userProfile = await EnrichWithKrrData(userProfile);
                    return userProfile;
                }
            }
        }

        // Using this flow as a fallback if the user is not found in the register, or if the setting to lookup users from the register is disabled.
        // This ensures that we can still fetch user profiles for self-identified where we do not have all data locally yet
        var result = await _userProfileClient.GetUser(userId);

        if (result.IsSuccess)
        {
            var enriched = await EnrichWithProfileSettings(result.Match(userProfile => userProfile, _ => default!));
            enriched = await EnrichWithKrrData(enriched);
            return enriched;
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUser(string ssn)
    {
        if (_settings.LookupUsersFromRegister)
        {
            var party = await _registerClient.GetUserPartyBySsn(ssn, default);
            var userProfile = UserProfileMapper.MapFromParty(party);

            if (userProfile != null)
            {
                if (party?.Type == PartyType.Person)
                {
                    userProfile = await EnrichWithProfileSettings(userProfile);
                    userProfile = await EnrichWithKrrData(userProfile);
                    return userProfile;
                }
            }
        }

        // Using this flow as a fallback if the user is not found in the register, or if the setting to lookup users from the register is disabled.
        // This ensures that we can still fetch user profiles for self-identified where we do not have all data locally yet
        var result = await _userProfileClient.GetUser(ssn);
        if (result.IsSuccess)
        {
            var enriched = await EnrichWithProfileSettings(result.Match(userProfile => userProfile, _ => default!));
            enriched = await EnrichWithKrrData(enriched);
            return enriched;
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUserByUsername(string username)
    {
        // Using SBL Bridge to fetch users as we do not have necessary data locally yet. 
        var result = await _userProfileClient.GetUserByUsername(username);
        if (result.IsSuccess)
        {
            var enriched = await EnrichWithProfileSettings(result.Match(userProfile => userProfile, _ => default!));
            enriched = await EnrichWithKrrData(enriched);
            return enriched;
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUserByUuid(Guid userUuid)
    {
        if (_settings.LookupUsersFromRegister)
        {
            var party = await _registerClient.GetUserParty(userUuid, default);
            var userProfile = UserProfileMapper.MapFromParty(party);

            if (userProfile != null)
            {
                if (party?.Type == PartyType.Person)
                {
                    userProfile = await EnrichWithProfileSettings(userProfile);
                    userProfile = await EnrichWithKrrData(userProfile);
                    return userProfile;
                }
            }
        }

        // Using this flow as a fallback if the user is not found in the register, or if the setting to lookup users from the register is disabled.
        // This ensures that we can still fetch user profiles for self-identified where we do not have all data locally yet
        var result = await _userProfileClient.GetUserByUuid(userUuid);
        if (result.IsSuccess)
        {
            var enriched = await EnrichWithProfileSettings(result.Match(userProfile => userProfile, _ => default!));
            enriched = await EnrichWithKrrData(enriched);
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
            var enriched = new List<UserProfile>();
            result.Match(
                async userProfiles =>
                {
                    foreach (UserProfile userProfile in userProfiles)
                    {
                        var enrichedUser = await EnrichWithKrrData(userProfile);
                        enriched.Add(await EnrichWithProfileSettings(enrichedUser));
                    }
                },
                _ => { });
            return enriched;
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task<string> GetPreferredLanguage(int userId)
    {
        var profileSettings = await _profileSettingsRepository.GetProfileSettings(userId);
        return profileSettings?.LanguageType ?? LanguageType.NB;
    }

    /// <inheritdoc/>
    public async Task<DateTime?> GetIgnoreUnitProfileDateTime(int userId)
    {
        var profileSettings = await _profileSettingsRepository.GetProfileSettings(userId);
        return profileSettings?.IgnoreUnitProfileDateTime;
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

    private async Task<UserProfile> EnrichWithProfileSettings(UserProfile userProfile)
    {
        ProfileSettings.ProfileSettings? profileSettings = await _profileSettingsRepository.GetProfileSettings(userProfile.UserId);
        if (profileSettings != null)
        {
            userProfile.ProfileSettingPreference ??= new ProfileSettingPreference();
            userProfile.ProfileSettingPreference.DoNotPromptForParty = profileSettings.DoNotPromptForParty;
            userProfile.ProfileSettingPreference.Language = profileSettings.LanguageType;
            userProfile.ProfileSettingPreference.PreselectedPartyUuid = profileSettings.PreselectedPartyUuid;
            userProfile.ProfileSettingPreference.ShowClientUnits = profileSettings.ShowClientUnits;
            userProfile.ProfileSettingPreference.ShouldShowSubEntities = profileSettings.ShouldShowSubEntities;
            userProfile.ProfileSettingPreference.ShouldShowDeletedEntities = profileSettings.ShouldShowDeletedEntities;
        }
        else
        {
            // If there are no profile settings for the user, we initialize it with default values to ensure that the user profile always has valid profile settings.
            userProfile.ProfileSettingPreference ??= ProfileSettingPreference.GetDefaultValues();
        }

        if (userProfile.ProfileSettingPreference.PreselectedPartyUuid != null && _settings.LookupPreselectedPartyIdAtRegister)
        {
            // If a preselected party UUID is provided, we need to fetch the corresponding party ID from the register to ensure data consistency.
            int? partyId = await _registerClient.GetPartyId(userProfile.ProfileSettingPreference.PreselectedPartyUuid.Value, default);
            userProfile.ProfileSettingPreference.PreSelectedPartyId = partyId ?? 0;
        }

        return userProfile;
    }

    private async Task<UserProfile> EnrichWithKrrData(UserProfile userProfile, CancellationToken cancellationToken = default)
    {
        if (userProfile.Party == null || string.IsNullOrEmpty(userProfile.Party.SSN))
        {
            // If the user profile does not have a party or SSN, we cannot enrich it with KRR data, so we return the original user profile. 
            // This is the case for self-identified users. 
            return userProfile;
        }

        var contactPreferences = await _personRepository.GetContactPreferencesAsync([userProfile.Party.SSN], cancellationToken);
        if (contactPreferences != null && contactPreferences.Count > 0)
        {
            userProfile.PhoneNumber = contactPreferences[0].MobileNumber;
            userProfile.Email = contactPreferences[0].Email;
            userProfile.IsReserved = contactPreferences[0].IsReserved;
        }

        return userProfile;
    }
}
