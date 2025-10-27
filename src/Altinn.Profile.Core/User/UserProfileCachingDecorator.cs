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
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _cacheOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserProfileCachingDecorator"/> class.
    /// </summary>
    /// <param name="decoratedService">The decorated userProfiles service</param>
    /// <param name="memoryCache">The memory cache</param>
    /// <param name="settings">The core settings</param>
    public UserProfileCachingDecorator(
        IUserProfileService decoratedService,
        IMemoryCache memoryCache,
        IOptions<CoreSettings> settings)
    {
        _decoratedService = decoratedService;
        _memoryCache = memoryCache;
        _cacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(settings.Value.ProfileCacheLifetimeSeconds)
        };
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUser(int userId)
    {
        string uniqueCacheKey = "User_UserId_" + userId;

        if (_memoryCache.TryGetValue(uniqueCacheKey, out UserProfile? user))
        {
            return user!;
        }

        Result<UserProfile, bool> result = await _decoratedService.GetUser(userId);

        result.Match(
            userProfile => _memoryCache.Set(uniqueCacheKey, userProfile, _cacheOptions),
            _ => { });

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUser(string ssn)
    {
        string uniqueCacheKey = "User_SSN_" + ssn;

        if (_memoryCache.TryGetValue(uniqueCacheKey, out UserProfile? user))
        {
            return user!;
        }

        Result<UserProfile, bool> result = await _decoratedService.GetUser(ssn);

        result.Match(
           userProfile => _memoryCache.Set(uniqueCacheKey, userProfile, _cacheOptions),
           _ => { });

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUserByUuid(Guid userUuid)
    {
        string uniqueCacheKey = $"User:UserUuid:{userUuid}";

        if (_memoryCache.TryGetValue(uniqueCacheKey, out UserProfile? user))
        {
            return user!;
        }

        Result<UserProfile, bool> result = await _decoratedService.GetUserByUuid(userUuid);

        result.Match(
         userProfile => _memoryCache.Set(uniqueCacheKey, userProfile, _cacheOptions),
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
            string uniqueCacheKey = $"User:UserUuid:{userUuid}";
            if (_memoryCache.TryGetValue(uniqueCacheKey, out UserProfile? user))
            {
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
                string uniqueCacheKey = $"User:UserUuid:{user.UserUuid}";
                _memoryCache.Set(uniqueCacheKey, user, _cacheOptions);
                result.Add(user);
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<Result<UserProfile, bool>> GetUserByUsername(string username)
    {
        string uniqueCacheKey = "User_Username_" + username;

        if (_memoryCache.TryGetValue(uniqueCacheKey, out UserProfile? user))
        {
            return user!;
        }

        Result<UserProfile, bool> result = await _decoratedService.GetUserByUsername(username);

        result.Match(
         userProfile => _memoryCache.Set(uniqueCacheKey, userProfile, _cacheOptions),
         _ => { });

        return result;
    }

    /// <inheritdoc/>
    public async Task<ProfileSettings.ProfileSettings> UpdateProfileSettings(ProfileSettings.ProfileSettings profileSettings)
    {
        // this should not be cached
        var result = await _decoratedService.UpdateProfileSettings(profileSettings);
        InvalidateCache(profileSettings.UserId);

        return result;
    }

    /// <inheritdoc/>
    public async Task<ProfileSettings.ProfileSettings?> PatchProfileSettings(ProfileSettingsPatchRequest profileSettings)
    {
        // this should not be cached
        var result = await _decoratedService.PatchProfileSettings(profileSettings);
        InvalidateCache(profileSettings.UserId);

        return result;
    }

    private void InvalidateCache(int userId)
    {
        string userIdKey = "User_UserId_" + userId;
        _memoryCache.Remove(userIdKey);
    }
}
