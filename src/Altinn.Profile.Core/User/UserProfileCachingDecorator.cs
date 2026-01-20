using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Models;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Altinn.Profile.Core.User;

/// <summary>.
/// Decorates an implementation of IUserProfiles by caching the userProfile object.
/// If available, object is retrieved from cache without calling the service
/// </summary>
public class UserProfileCachingDecorator : IUserProfileService
{
    private readonly IUserProfileService _decoratedService;
    private readonly IUserProfileSettingsService _userProfileSettingsService;
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _cacheOptions;
    private const string CacheKeyPrefix = "User_UserId_";

    /// <summary>
    /// Initializes a new instance of the <see cref="UserProfileCachingDecorator"/> class.
    /// </summary>
    /// <param name="decoratedService">The decorated userProfiles service</param>
    /// <param name="memoryCache">The memory cache</param>
    /// <param name="settings">The core settings</param>
    /// <param name="userProfileSettingsService">The user profile settings service</param>
    public UserProfileCachingDecorator(
        IUserProfileService decoratedService,
        IMemoryCache memoryCache,
        IOptions<CoreSettings> settings,
        IUserProfileSettingsService userProfileSettingsService)
    {
        _decoratedService = decoratedService;
        _memoryCache = memoryCache;
        _cacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(settings.Value.ProfileCacheLifetimeSeconds)
        };
        _userProfileSettingsService = userProfileSettingsService;
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUser(int userId)
    {
        if (TryGetUserFromCache(userId, out UserProfile? user))
        {
            await _userProfileSettingsService.EnrichWithProfileSettings(user!);
            return user!;
        }

        Result<UserProfile, bool> result = await _decoratedService.GetUser(userId);

        result.Match(
            userProfile => AddUserToCache(userProfile),
            _ => { });

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUser(string ssn)
    {
        string uniqueCacheKey = "UserId_SSN_" + ssn;

        if (TryGetUserFromCache(uniqueCacheKey, out UserProfile? user))
        {
            await _userProfileSettingsService.EnrichWithProfileSettings(user!);
            return user!;
        }

        Result<UserProfile, bool> result = await _decoratedService.GetUser(ssn);

        result.Match(
           userProfile => AddUserToCache(uniqueCacheKey, userProfile),
           _ => { });

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUserByUuid(Guid userUuid)
    {
        string uniqueCacheKey = $"UserId_UserUuid_{userUuid}";

        if (TryGetUserFromCache(uniqueCacheKey, out UserProfile? user))
        {
            await _userProfileSettingsService.EnrichWithProfileSettings(user!);
            return user!;
        }

        Result<UserProfile, bool> result = await _decoratedService.GetUserByUuid(userUuid);

        result.Match(
             userProfile => AddUserToCache(uniqueCacheKey, userProfile),
             _ => { });
        return result;
    }

    /// <inheritdoc /> 
    public async Task<Result<List<UserProfile>, bool>> GetUserListByUuid(List<Guid> userUuidList)
    {
        List<Guid> userUuidListNotInCache = [];
        List<UserProfile> result = [];

        foreach (Guid userUuid in userUuidList)
        {
            string uniqueCacheKey = $"UserId_UserUuid_{userUuid}";
            if (TryGetUserFromCache(uniqueCacheKey, out UserProfile? user))
            {
                await _userProfileSettingsService.EnrichWithProfileSettings(user!);
                result.Add(user!);
            }
            else
            {
                userUuidListNotInCache.Add(userUuid);
            }
        }

        if (userUuidListNotInCache.Count > 0)
        {
            Result<List<UserProfile>, bool> fetchedUserProfiles = await _decoratedService.GetUserListByUuid(userUuidListNotInCache);
            List<UserProfile> usersToCache = fetchedUserProfiles.Match(
             userProfileList => userProfileList,
             _ => []);

            foreach (UserProfile user in usersToCache)
            {
                string uniqueCacheKey = $"UserId_UserUuid_{user.UserUuid}";
                AddUserToCache(uniqueCacheKey, user);

                result.Add(user);
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUserByUsername(string username)
    {
        string uniqueCacheKey = "UserId_Username_" + username;

        if (TryGetUserFromCache(uniqueCacheKey, out UserProfile? user))
        {
            await _userProfileSettingsService.EnrichWithProfileSettings(user!);
            return user!;
        }

        Result<UserProfile, bool> result = await _decoratedService.GetUserByUsername(username);

        result.Match(
         userProfile => AddUserToCache(uniqueCacheKey, userProfile),
         _ => { });

        return result;
    }

    /// <inheritdoc/>
    public async Task<ProfileSettings.ProfileSettings> UpdateProfileSettings(ProfileSettings.ProfileSettings profileSettings, CancellationToken cancellationToken)
    {
        // this should not be cached
        var result = await _decoratedService.UpdateProfileSettings(profileSettings, cancellationToken);

        return result;
    }

    /// <inheritdoc/>
    public async Task<ProfileSettings.ProfileSettings?> PatchProfileSettings(ProfileSettingsPatchModel profileSettings, CancellationToken cancellationToken)
    {
        // this should not be cached
        var result = await _decoratedService.PatchProfileSettings(profileSettings, cancellationToken);

        return result;
    }

    /// <inheritdoc/>
    public async Task<ProfileSettings.ProfileSettings?> GetProfileSettings(int userId)
    {
        // this should not be cached
        var result = await _decoratedService.GetProfileSettings(userId);
            
        return result;
    }

    private void AddUserToCache(string uniqueCacheKey, UserProfile userProfile)
    {
        // Cache userId for the unique key (ssn, username, uuid)
        _memoryCache.Set(uniqueCacheKey, userProfile.UserId, _cacheOptions);

        // Cache the full user profile for userId key
        AddUserToCache(userProfile);
    }

    private void AddUserToCache(UserProfile userProfile)
    {
        string userCacheKey = CacheKeyPrefix + userProfile.UserId;

        // Cache the full user profile for userId key
        _memoryCache.Set(userCacheKey, userProfile, _cacheOptions);
    }

    /// <summary>
    /// Get the user from cache based on unique cache key (ssn, username, uuid)
    /// </summary>
    /// <param name="uniqueCacheKey">Cache key with ssn, username or uuid</param>
    /// <param name="user">The userProfile output</param>
    /// <returns>Returns true if the user was found in cache, false otherwise</returns>
    private bool TryGetUserFromCache(string uniqueCacheKey, out UserProfile? user)
    {
        user = null;
        if (_memoryCache.TryGetValue(uniqueCacheKey, out int? userId)
            && TryGetUserFromCache((int)userId!, out user))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Get the user from cache based on userId
    /// </summary>
    /// <returns>Returns true if the user was found in cache, false otherwise</returns>
    private bool TryGetUserFromCache(int userId, out UserProfile? user)
    {
        string cacheKey = CacheKeyPrefix + userId;

        var success = _memoryCache.TryGetValue(cacheKey, out user);
        return success;
    }
}
