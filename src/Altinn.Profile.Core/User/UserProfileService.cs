﻿using Altinn.Platform.Profile.Models;
using Altinn.Profile.Core.Integrations;

namespace Altinn.Profile.Core.User;

/// <summary>
/// Implementation of <see cref="IUserProfileService"/> that uses <see cref="IUserProfileRepository"/> to fetch user profiles."/>
/// </summary>
public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _userProfileClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserProfileService"/> class.
    /// </summary>
    /// <param name="userProfileClient">The user profile client available through DI</param>
    public UserProfileService(IUserProfileRepository userProfileClient)
    {
        _userProfileClient = userProfileClient;
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
}
