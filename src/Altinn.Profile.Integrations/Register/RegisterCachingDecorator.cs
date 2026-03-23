using Altinn.Profile.Core;
using Altinn.Profile.Core.Integrations;
using Altinn.Register.Contracts;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using ContactPointParty = Altinn.Profile.Core.Unit.ContactPoints.Party;
using Party = Altinn.Register.Contracts.Party;

namespace Altinn.Profile.Integrations.Register;

/// <summary>.
/// Decorates an implementation of IRegisterClient by caching the response object.
/// If available, object is retrieved from cache without calling the client
/// </summary>
public class RegisterCachingDecorator : IRegisterClient
{
    private readonly IRegisterClient _decoratedService;
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions _cacheOptions;
    private const string CacheKeyPrefix = "Party_User_UserId_";

    /// <summary>
    /// Initializes a new instance of the <see cref="RegisterCachingDecorator"/> class.
    /// </summary>
    /// <param name="decoratedService">The decorated register client</param>
    /// <param name="memoryCache">The memory cache</param>
    /// <param name="settings">The core settings</param>
    public RegisterCachingDecorator(
        IRegisterClient decoratedService,
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
    public async Task<string?> GetMainUnit(string orgNumber, CancellationToken cancellationToken)
    {
        var cacheKey = $"MainUnit_{orgNumber}";
        if (_memoryCache.TryGetValue(cacheKey, out string? mainUnit))
        {
            return mainUnit;
        }

        mainUnit = await _decoratedService.GetMainUnit(orgNumber, cancellationToken);
        if (mainUnit != null)
        {
            _memoryCache.Set(cacheKey, mainUnit, _cacheOptions);
        }

        return mainUnit;
    }

    /// <inheritdoc/>
    public async Task<int?> GetPartyId(Guid partyUuid, CancellationToken cancellationToken)
    {
        var cacheKey = $"PartyId_{partyUuid}";
        if (_memoryCache.TryGetValue(cacheKey, out int? partyId))
        {
            return partyId;
        }

        partyId = await _decoratedService.GetPartyId(partyUuid, cancellationToken);
        if (partyId.HasValue)
        {
            _memoryCache.Set(cacheKey, partyId.Value, _cacheOptions);
        }

        return partyId;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ContactPointParty>?> GetPartyUuids(string[] orgNumbers, CancellationToken cancellationToken)
    {
        List<string> orgNumbersNotInCache = [];
        List<ContactPointParty> result = [];

        foreach (string orgNumber in orgNumbers)
        {
            string uniqueCacheKey = $"PartyUuid_{orgNumber}";
            if (_memoryCache.TryGetValue(uniqueCacheKey, out ContactPointParty? partyUuid))
            {
                result.Add(partyUuid!);
            }
            else
            {
                orgNumbersNotInCache.Add(orgNumber);
            }
        }

        if (orgNumbersNotInCache.Count > 0)
        {
            IReadOnlyList<ContactPointParty>? fetchedParties = await _decoratedService.GetPartyUuids([..orgNumbersNotInCache], cancellationToken);
            if (fetchedParties == null)
            {
                return result;
            }

            foreach (ContactPointParty party in fetchedParties)
            {
                string uniqueCacheKey = $"PartyUuid_{party.OrganizationIdentifier}";
                _memoryCache.Set(uniqueCacheKey, party);

                result.Add(party);
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<string?> GetOrganizationNumberByPartyUuid(Guid partyUuid, CancellationToken cancellationToken)
    {
        var cacheKey = $"OrgNumber_{partyUuid}";
        if (_memoryCache.TryGetValue(cacheKey, out string? orgNumber))
        {
            return orgNumber;
        }

        orgNumber = await _decoratedService.GetOrganizationNumberByPartyUuid(partyUuid, cancellationToken);

        if (orgNumber != null)
        {
            _memoryCache.Set(cacheKey, orgNumber, _cacheOptions);
        }

        return orgNumber;
    }

    /// <inheritdoc/>
    public async Task<Party?> GetUserParty(Guid userUuid, CancellationToken cancellationToken)
    {
        string uniqueCacheKey = $"Party_UserId_UserUuid_{userUuid}";
        if (TryGetUserFromCache(uniqueCacheKey, out Party? user))
        {
            return user!;
        }

        var party = await _decoratedService.GetUserParty(userUuid, cancellationToken);

        if (party != null)
        {
            AddUserToCache(uniqueCacheKey, party);
        }

        return party;
    }

    /// <inheritdoc/>
    public async Task<Party?> GetUserParty(int userId, CancellationToken cancellationToken)
    {
        if (TryGetUserFromCache(userId, out Party? user))
        {
            return user!;
        }

        var party = await _decoratedService.GetUserParty(userId, cancellationToken);

        if (party != null)
        {
            AddUserToCache(party);
        }

        return party;
    }

    /// <inheritdoc/>
    public async Task<Party?> GetUserPartyByUsername(string username, CancellationToken cancellationToken)
    {
        string uniqueCacheKey = "Party_UserId_Username_" + username;
        if (TryGetUserFromCache(uniqueCacheKey, out Party? user))
        {
            return user!;
        }

        var party = await _decoratedService.GetUserPartyByUsername(username, cancellationToken);

        if (party != null)
        {
            AddUserToCache(uniqueCacheKey, party);
        }

        return party;
    }

    /// <inheritdoc/>
    public async Task<Party?> GetUserPartyBySsn(string ssn, CancellationToken cancellationToken)
    {
        string uniqueCacheKey = "Party_UserId_SSN_" + ssn;
        if (TryGetUserFromCache(uniqueCacheKey, out Party? user))
        {
            return user!;
        }

        var party = await _decoratedService.GetUserPartyBySsn(ssn, cancellationToken);

        if (party != null)
        {
            AddUserToCache(uniqueCacheKey, party);
        }

        return party;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Party>> GetUserParties(List<Guid> userUuids, CancellationToken cancellationToken)
    {
        List<Guid> userUuidListNotInCache = [];
        List<Party> result = [];

        foreach (Guid userUuid in userUuids)
        {
            string uniqueCacheKey = $"Party_UserId_UserUuid_{userUuid}";
            if (TryGetUserFromCache(uniqueCacheKey, out Party? user))
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
            IReadOnlyList<Party> fetchedUserProfiles = await _decoratedService.GetUserParties(userUuidListNotInCache, cancellationToken);

            foreach (Party user in fetchedUserProfiles)
            {
                string uniqueCacheKey = $"Party_UserId_UserUuid_{user.Uuid}";
                AddUserToCache(uniqueCacheKey, user);

                result.Add(user);
            }
        }

        return result;
    }

    // Private methods
    private void AddUserToCache(string uniqueCacheKey, Party userProfile)
    {
        if (!userProfile.User.HasValue || !userProfile.User.Value.UserId.HasValue)
        {
            return;
        }

        int userId = (int)userProfile.User.Value.UserId.Value;

        // Cache userId for the unique key (ssn, username, uuid)
        _memoryCache.Set(uniqueCacheKey, userId, _cacheOptions);

        // Cache the full user profile for userId key
        AddUserToCache(userProfile);
    }

    private void AddUserToCache(Party userProfile)
    {
        if (!userProfile.User.HasValue || !userProfile.User.Value.UserId.HasValue)
        {
            return;
        }

        int userId = (int)userProfile.User.Value.UserId.Value;
        string userCacheKey = CacheKeyPrefix + userId;

        // Cache the full user profile for userId key
        _memoryCache.Set(userCacheKey, userProfile, _cacheOptions);
    }

    /// <summary>
    /// Get the user from cache based on unique cache key (ssn, username, uuid)
    /// </summary>
    /// <param name="uniqueCacheKey">Cache key with ssn, username or uuid</param>
    /// <param name="user">The userProfile output</param>
    /// <returns>Returns true if the user was found in cache, false otherwise</returns>
    private bool TryGetUserFromCache(string uniqueCacheKey, out Party? user)
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
    private bool TryGetUserFromCache(int userId, out Party? user)
    {
        string cacheKey = CacheKeyPrefix + userId;

        var success = _memoryCache.TryGetValue(cacheKey, out user);
        return success;
    }
}
