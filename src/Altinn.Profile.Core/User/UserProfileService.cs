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
    private readonly IUserProfileComparer _userProfileComparer;
    private readonly IProfileSettingsRepository _profileSettingsRepository;
    private readonly IPersonService _personRepository;
    private readonly IRegisterClient _registerClient;
    private readonly CoreSettings _settings;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserProfileService"/> class.
    /// </summary>
    /// <param name="userProfileClient">The user profile client available through DI</param>
    /// <param name="userProfileComparer">The user profile comparer available through DI</param>
    /// <param name="profileSettingsRepository">The profile settings repository available through DI</param>
    /// <param name="personRepository">The person repository available through DI</param>
    /// <param name="registerClient">The register client available through DI</param>
    /// <param name="settings">The core settings available through DI</param>
    public UserProfileService(IUserProfileClient userProfileClient, IUserProfileComparer userProfileComparer, IProfileSettingsRepository profileSettingsRepository, IPersonService personRepository, IRegisterClient registerClient, IOptionsMonitor<CoreSettings> settings)
    {
        _userProfileClient = userProfileClient;
        _userProfileComparer = userProfileComparer;
        _profileSettingsRepository = profileSettingsRepository;
        _personRepository = personRepository;
        _registerClient = registerClient;
        _settings = settings.CurrentValue;
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUser(int userId, CancellationToken cancellationToken)
    {
        return await GetUserWithSourceSelection(
            () => _userProfileClient.GetUser(userId),
            () => _registerClient.GetUserParty(userId, cancellationToken),
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUser(string ssn, CancellationToken cancellationToken)
    {
        return await GetUserWithSourceSelection(
            () => _userProfileClient.GetUser(ssn),
            () => _registerClient.GetUserPartyBySsn(ssn, cancellationToken),
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUserByUsername(string username, CancellationToken cancellationToken)
    {
        return await GetUserWithSourceSelection(
            () => _userProfileClient.GetUserByUsername(username),
            () => _registerClient.GetUserPartyByUsername(username, cancellationToken),
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUserByUuid(Guid userUuid, CancellationToken cancellationToken)
    {
        return await GetUserWithSourceSelection(
            () => _userProfileClient.GetUserByUuid(userUuid),
            () => _registerClient.GetUserParty(userUuid, cancellationToken),
            cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Result<List<UserProfile>, bool>> GetUserListByUuid(List<Guid> userUuidList, CancellationToken cancellationToken)
    {
        Task<Result<List<UserProfile>, bool>> legacyTask = _userProfileClient.GetUserListByUuid(userUuidList);

        if (!_settings.RegisterLookupInShadowMode)
        {
            Result<List<UserProfile>, bool> legacyResult = await legacyTask;
            if (!legacyResult.IsSuccess)
            {
                return false;
            }

            List<UserProfile> legacyProfiles = legacyResult.Match(userProfiles => userProfiles, _ => []);
            return await EnrichWithKrrDataAndProfileSettings(legacyProfiles);
        }

        Task<List<UserProfile>> registerTask = GetUserListFromRegister(userUuidList, cancellationToken);

        await Task.WhenAll(legacyTask, registerTask);

        Result<List<UserProfile>, bool> result = await legacyTask;
        List<UserProfile> registerProfiles = await registerTask;

        if (!result.IsSuccess)
        {
            foreach (UserProfile registerProfile in registerProfiles)
            {
                _userProfileComparer.CompareAndLog(null, registerProfile);
            }

            return false;
        }

        List<UserProfile> legacyProfilesWithEnrichment = await EnrichWithKrrDataAndProfileSettings(result.Match(userProfiles => userProfiles, _ => []));

        CompareUserProfilesByUuid(legacyProfilesWithEnrichment, registerProfiles, userUuidList);

        return legacyProfilesWithEnrichment;
    }

    private async Task<Result<UserProfile, bool>> GetUserWithSourceSelection(
        Func<Task<Result<UserProfile, bool>>> getLegacy,
        Func<Task<Party?>> getRegisterParty,
        CancellationToken cancellationToken)
    {
        if (_settings.RegisterAsPrimaryUserProfileSource)
        {
            UserProfile? registerProfile = await TryGetEligibleRegisterProfile(getRegisterParty(), cancellationToken);
            if (registerProfile is not null)
            {
                return registerProfile;
            }

            UserProfile? fallbackLegacy = await GetEnrichedLegacyUserProfile(getLegacy());
            return CreateUserProfileResult(fallbackLegacy);
        }

        if (_settings.RegisterLookupInShadowMode)
        {
            Task<Result<UserProfile, bool>> legacyTask = getLegacy();
            Task<UserProfile?> registerTask = GetUserFromRegister(getRegisterParty(), cancellationToken);

            await Task.WhenAll(legacyTask, registerTask);

            UserProfile? registerProfile = await registerTask;
            UserProfile? legacyProfile = await GetEnrichedLegacyUserProfile(legacyTask);

            _userProfileComparer.CompareAndLog(legacyProfile, registerProfile);

            return CreateUserProfileResult(legacyProfile);
        }

        UserProfile? legacyOnly = await GetEnrichedLegacyUserProfile(getLegacy());
        return CreateUserProfileResult(legacyOnly);
    }

    private static Result<UserProfile, bool> CreateUserProfileResult(UserProfile? userProfile)
    {
        if (userProfile is null)
        {
            return false;
        }

        return userProfile;
    }

    private static bool IsEligibleRegisterProfile(Party? party)
    {
        // To ensure that we only use register profiles that can be enriched with KRR data, we require that the profile has a non-empty SSN.
        // This is because KRR data is linked to the user's SSN, and without it, we cannot enrich the profile with contact information from KRR.
        // All ssn users are returned as Person type from the register.
        return party is Register.Contracts.Person;
    }

    private async Task<UserProfile?> TryGetEligibleRegisterProfile(Task<Party?> registerPartyTask, CancellationToken cancellationToken)
    {
        Party? registerParty;

        try
        {
            registerParty = await registerPartyTask;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // fallback to legacy
            return null;
        }

        if (IsEligibleRegisterProfile(registerParty))
        {
            return await CreateAndEnrichProfileFromParty(registerParty, cancellationToken);
        }

        return null;
    }

    private async Task<UserProfile?> GetEnrichedLegacyUserProfile(Task<Result<UserProfile, bool>> legacyTask)
    {
        Result<UserProfile, bool> legacyResult = await legacyTask;
        if (!legacyResult.IsSuccess)
        {
            return null;
        }

        UserProfile legacyProfile = legacyResult.Match(userProfile => userProfile, _ => default!);
        legacyProfile = await EnrichWithProfileSettings(legacyProfile, default);
        legacyProfile = await EnrichWithKrrData(legacyProfile);

        return legacyProfile;
    }

    private async Task<List<UserProfile>> EnrichWithKrrDataAndProfileSettings(List<UserProfile> legacyProfiles)
    {
        List<UserProfile> enriched = new(legacyProfiles.Count);
        foreach (UserProfile userProfile in legacyProfiles)
        {
            UserProfile enrichedUser = await EnrichWithKrrData(userProfile, default);
            enriched.Add(await EnrichWithProfileSettings(enrichedUser, default));
        }

        return enriched;
    }

    private void CompareUserProfilesByUuid(List<UserProfile> source, List<UserProfile> target, IEnumerable<Guid> userUuids)
    {
        Dictionary<Guid, UserProfile> sourceByUuid = source
            .Where(userProfile => userProfile.UserUuid.HasValue && userProfile.UserUuid.Value != Guid.Empty)
            .GroupBy(userProfile => userProfile.UserUuid!.Value)
            .ToDictionary(g => g.Key, g => g.First());

        Dictionary<Guid, UserProfile> targetByUuid = target
            .Where(userProfile => userProfile.UserUuid.HasValue && userProfile.UserUuid.Value != Guid.Empty)
            .GroupBy(userProfile => userProfile.UserUuid!.Value)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (Guid userUuid in userUuids.Distinct())
        {
            sourceByUuid.TryGetValue(userUuid, out UserProfile? sourceProfile);
            targetByUuid.TryGetValue(userUuid, out UserProfile? targetProfile);
            _userProfileComparer.CompareAndLog(sourceProfile, targetProfile);
        }
    }

    private async Task<List<UserProfile>> GetUserListFromRegister(List<Guid> userUuidList, CancellationToken cancellationToken)
    {
        IReadOnlyList<Party> registerParties = await _registerClient.GetUserParties(userUuidList, cancellationToken);
        List<UserProfile> registerProfiles = new(registerParties?.Count ?? 0);
        if (registerParties == null)
        {
            return registerProfiles;
        }

        foreach (Party registerParty in registerParties)
        {
            UserProfile? userProfile = await CreateAndEnrichProfileFromParty(registerParty, cancellationToken);
            if (userProfile != null)
            {
                registerProfiles.Add(userProfile);
            }
        }

        return registerProfiles;
    }

    private async Task<UserProfile?> GetUserFromRegister(Task<Party?> registerPartyTask, CancellationToken cancellationToken)
    {
        Party? registerParty;

        try
        {
            registerParty = await registerPartyTask;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // in shadow mode, exceptions from the register client should not affect the user experience.
            return null;
        }

        return await CreateAndEnrichProfileFromParty(registerParty, cancellationToken);
    }

    private async Task<UserProfile?> CreateAndEnrichProfileFromParty(Party? party, CancellationToken cancellationToken)
    {
        UserProfile? userProfile = UserProfileMapper.MapFromParty(party);
        if (userProfile is null)
        {
            return null;
        }

        userProfile = await EnrichWithProfileSettings(userProfile, cancellationToken);
        userProfile = await EnrichWithKrrData(userProfile, cancellationToken);

        return userProfile;
    }

    /// <inheritdoc/>
    public async Task<string> GetPreferredLanguage(int userId, CancellationToken cancellationToken = default)
    {
        var profileSettings = await _profileSettingsRepository.GetProfileSettings(userId, cancellationToken);
        return profileSettings?.LanguageType ?? LanguageType.NB;
    }

    /// <inheritdoc/>
    public async Task<DateTime?> GetIgnoreUnitProfileDateTime(int userId, CancellationToken cancellationToken = default)
    {
        var profileSettings = await _profileSettingsRepository.GetProfileSettings(userId, cancellationToken);
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

    private async Task<UserProfile> EnrichWithProfileSettings(UserProfile userProfile, CancellationToken cancellationToken)
    {
        ProfileSettings.ProfileSettings? profileSettings = await _profileSettingsRepository.GetProfileSettings(userProfile.UserId, cancellationToken);
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
