using Altinn.Platform.Profile.Models;
using Altinn.Profile.Core.Integrations;

namespace Altinn.Profile.Core.User;

/// <summary>
/// Implementation of <see cref="IUserProfileService"/> that uses <see cref="IUserProfileClient"/> to fetch user profiles."/>
/// </summary>
public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileClient _userProfileRepo;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserProfileService"/> class.
    /// </summary>
    /// <param name="userProfileRepo">The user profile client available through DI</param>
    public UserProfileService(IUserProfileClient userProfileRepo)
    {
        _userProfileRepo = userProfileRepo;
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUser(int userId)
    {
        return await _userProfileRepo.GetUser(userId);
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUser(string ssn)
    {
        return await _userProfileRepo.GetUser(ssn);
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUserByUsername(string username)
    {
        return await _userProfileRepo.GetUserByUsername(username);
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUserByUuid(Guid userUuid)
    {
        return await _userProfileRepo.GetUserByUuid(userUuid);
    }

    /// <inheritdoc/>
    public async Task<Result<List<UserProfile>, bool>> GetUserListByUuid(List<Guid> userUuidList)
    {
        return await _userProfileRepo.GetUserListByUuid(userUuidList);
    }
}
