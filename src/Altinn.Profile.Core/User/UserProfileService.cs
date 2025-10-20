using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Models;

namespace Altinn.Profile.Core.User;

/// <summary>
/// Implementation of <see cref="IUserProfileService"/> that uses <see cref="IUserProfileClient"/> to fetch user profiles."/>
/// </summary>
public class UserProfileService : IUserProfileService
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
        return await _userProfileClient.GetUser(userId);
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUser(string ssn)
    {
        return await _userProfileClient.GetUser(ssn);
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUserByUsername(string username)
    {
        return await _userProfileClient.GetUserByUsername(username);
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUserByUuid(Guid userUuid)
    {
        return await _userProfileClient.GetUserByUuid(userUuid);
    }

    /// <inheritdoc/>
    public async Task<Result<List<UserProfile>, bool>> GetUserListByUuid(List<Guid> userUuidList)
    {
        return await _userProfileClient.GetUserListByUuid(userUuidList);
    }

    /// <inheritdoc/>
    public async Task<ProfileSettings.ProfileSettings> UpdateProfileSettings(ProfileSettings.ProfileSettings profileSettings)
    {
        return await _profileSettingsRepository.UpdateProfileSettings(profileSettings);
    }
}
