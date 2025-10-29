using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Altinn.Profile.Core;
using Altinn.Profile.Core.User;
using Altinn.Profile.Models;
using Altinn.Profile.Tests.Testdata;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Core.User;

public class UserProfileCachingDecoratorTest
{
    private readonly Mock<IUserProfileService> _decoratedServiceMock = new();
    private readonly Mock<IOptions<CoreSettings>> coreSettingsOptions;

    public UserProfileCachingDecoratorTest()
    {
        coreSettingsOptions = new Mock<IOptions<CoreSettings>>();
        coreSettingsOptions.Setup(s => s.Value).Returns(new CoreSettings { ProfileCacheLifetimeSeconds = 600 });
    }

    /// <summary>
    /// Tests that the userprofile available in the cache is returned to the caller without forwarding request to decorated service.
    /// </summary>
    [Fact]
    public async Task GetUserUserId_UserInCache_decoratedServiceNotCalled()
    {
        // Arrange
        const int UserId = 2001607;
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        var userProfile = await TestDataLoader.Load<UserProfile>(UserId.ToString());
        memoryCache.Set("User_UserId_2001607", userProfile);
        var target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

        // Act
        Result<UserProfile, bool> result = await target.GetUser(UserId);

        // Assert
        Assert.True(result.IsSuccess, "Expected a success result");

        result.Match(
            actual =>
            {
                Assert.NotNull(actual);
                Assert.Equal(UserId, actual.UserId);
            },
            _ => { });

        _decoratedServiceMock.Verify(service => service.GetUser(It.IsAny<int>()), Times.Never());
    }

    /// <summary>
    /// Tests that the userprofile available in the cache is returned to the caller without forwarding request to decorated service.
    /// </summary>
    [Fact]
    public async Task GetUserUserUuid_UserInCache_decoratedServiceNotCalled()
    {
        // Arrange
        Guid userUuid = new("cc86d2c7-1695-44b0-8e82-e633243fdf31");
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        var userProfile = await TestDataLoader.Load<UserProfile>(userUuid.ToString());
        memoryCache.Set($"UserId_UserUuid_{userUuid}", userProfile.UserId);
        memoryCache.Set($"User_UserId_{userProfile.UserId}", userProfile);
        var target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

        // Act
        Result<UserProfile, bool> result = await target.GetUserByUuid(userUuid);

        // Assert
        Assert.True(result.IsSuccess, "Expected a success result");
        result.Match(
            actual =>
            {
                Assert.NotNull(actual);
                Assert.Equal(userUuid, actual.UserUuid);
            },
            _ => { });

        _decoratedServiceMock.Verify(service => service.GetUserByUuid(It.IsAny<Guid>()), Times.Never());
    }

    /// <summary>
    /// Tests that one of the user profiles are available in the cache the other is fetched from the decorated service both is returned to caller.
    /// </summary>
    [Fact]
    public async Task GetUserListUserUuid_UsersPartialInCache_decoratedServiceBothCalledAndNotCalled()
    {
        // Arrange
        List<Guid> userUuids = new List<Guid> { new("cc86d2c7-1695-44b0-8e82-e633243fdf31"), new("4c3b4909-eb17-45d5-bde1-256e065e196a") };
        Guid userUuidNotInCache = new("4c3b4909-eb17-45d5-bde1-256e065e196a");
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        UserProfile userProfile = await TestDataLoader.Load<UserProfile>(userUuids[0].ToString());
        memoryCache.Set($"UserId_UserUuid_{userUuids[0]}", userProfile.UserId);
        memoryCache.Set($"User_UserId_{userProfile.UserId}", userProfile);
        List<UserProfile> userProfiles = new List<UserProfile>();
        userProfiles.Add(await TestDataLoader.Load<UserProfile>(userUuidNotInCache.ToString()));
        _decoratedServiceMock.Setup(service => service.GetUserListByUuid(It.Is<List<Guid>>(g => g.TrueForAll(g2 => g2 == userUuidNotInCache)))).ReturnsAsync(userProfiles);
        UserProfileCachingDecorator target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

        // Act
        Result<List<UserProfile>, bool> result = await target.GetUserListByUuid(userUuids);

        // Assert
        Assert.True(result.IsSuccess, "Expected a success result");
        _decoratedServiceMock.Verify(service => service.GetUserListByUuid(It.Is<List<Guid>>(g => g.TrueForAll(g2 => g2 == userUuidNotInCache))), Times.Once);
        result.Match(
            actual =>
            {
                Assert.NotNull(actual);
                foreach (var userUuid in userUuids)
                {
                    UserProfile currentProfileFromResult = actual.Find(p => p.UserUuid == userUuid);
                    int currentProfileFromCache = memoryCache.Get<int>($"UserId_UserUuid_{userUuid}");
                    Assert.NotNull(currentProfileFromResult);
                    Assert.NotEqual(default(int), currentProfileFromCache);
                }
            },
            _ => { });
    }

    /// <summary>
    /// Tests that the user profiles are available in the cache and is returned to the caller without forwarding request to decorated service.
    /// </summary>
    [Fact]
    public async Task GetUserListUserUuid_UsersInCache_decoratedServiceNotCalled()
    {
        // Arrange
        List<Guid> userUuids = new List<Guid> { new("cc86d2c7-1695-44b0-8e82-e633243fdf31"), new("4c3b4909-eb17-45d5-bde1-256e065e196a") };
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        foreach (Guid userUuid in userUuids)
        {
            UserProfile userProfile = await TestDataLoader.Load<UserProfile>(userUuid.ToString());
            memoryCache.Set($"UserId_UserUuid_{userUuid}", userProfile.UserId);
            memoryCache.Set($"User_UserId_{userProfile.UserId}", userProfile);
        }

        UserProfileCachingDecorator target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

        // Act
        Result<List<UserProfile>, bool> result = await target.GetUserListByUuid(userUuids);

        // Assert
        _decoratedServiceMock.Verify(service => service.GetUserListByUuid(It.IsAny<List<Guid>>()), Times.Never());
        Assert.True(result.IsSuccess, "Expected a success result");
        result.Match(
            actual =>
            {
                Assert.NotNull(actual);
                foreach (var userUuid in userUuids)
                {
                    UserProfile currentProfileFromResult = actual.Find(p => p.UserUuid == userUuid);
                    Assert.NotNull(currentProfileFromResult);
                }
            },
            _ => { });
    }

    /// <summary>
    /// Tests that the userprofile is not available in the cache call is forwarded to decorated service and cache is populated result returned to caller.
    /// </summary>
    [Fact]
    public async Task GetUserUserId_UserNotInCache_decoratedServiceCalledMockPopulated()
    {
        // Arrange
        const int UserId = 2001607;
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        var userProfile = await TestDataLoader.Load<UserProfile>(UserId.ToString());
        _decoratedServiceMock.Setup(service => service.GetUser(It.IsAny<int>())).ReturnsAsync(userProfile);
        var target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

        // Act
        Result<UserProfile, bool> result = await target.GetUser(UserId);

        // Assert
        _decoratedServiceMock.Verify(service => service.GetUser(It.IsAny<int>()), Times.Once());
        Assert.True(result.IsSuccess, "Expected a success result");

        result.Match(
           actual =>
           {
               Assert.NotNull(actual);
               Assert.Equal(UserId, actual.UserId);
           },
           _ => { });

        Assert.True(memoryCache.TryGetValue("User_UserId_2001607", out UserProfile _), "No data found in memory cache");
    }

    /// <summary>
    /// Tests that the userprofile is not available in the cache call is forwarded to decorated service and cache is populated result returned to caller.
    /// </summary>
    [Fact]
    public async Task GetUserUserUuid_UserNotInCache_decoratedServiceCalledMockPopulated()
    {
        // Arrange
        Guid userUuid = new("cc86d2c7-1695-44b0-8e82-e633243fdf31");
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        var userProfile = await TestDataLoader.Load<UserProfile>(userUuid.ToString());
        _decoratedServiceMock.Setup(service => service.GetUserByUuid(It.IsAny<Guid>())).ReturnsAsync(userProfile);
        var target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

        // Act
        Result<UserProfile, bool> result = await target.GetUserByUuid(userUuid);

        // Assert
        _decoratedServiceMock.Verify(service => service.GetUserByUuid(It.IsAny<Guid>()), Times.Once());
        Assert.True(result.IsSuccess, "Expected a success result");

        result.Match(
            actual =>
            {
                Assert.NotNull(actual);
                Assert.Equal(userUuid, actual.UserUuid);
            },
            _ => { });

        Assert.True(memoryCache.TryGetValue($"UserId_UserUuid_{userUuid}", out int _), "No data found in memory cache");
    }

    /// <summary>
    /// Tests that the user profiles is not available in the cache call is forwarded to decorated service and cache is populated result returned to caller.
    /// </summary>
    [Fact]
    public async Task GetUserListUserUuid_UserNotInCache_decoratedServiceCalledMockPopulated()
    {
        // Arrange
        List<Guid> userUuids = new List<Guid> { new("cc86d2c7-1695-44b0-8e82-e633243fdf31"), new("4c3b4909-eb17-45d5-bde1-256e065e196a") };
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        List<UserProfile> userProfiles = new List<UserProfile>();
        userProfiles.Add(await TestDataLoader.Load<UserProfile>(userUuids[0].ToString()));
        userProfiles.Add(await TestDataLoader.Load<UserProfile>(userUuids[1].ToString()));

        _decoratedServiceMock.Setup(service => service.GetUserListByUuid(It.IsAny<List<Guid>>())).ReturnsAsync(userProfiles);
        var target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

        // Act
        Result<List<UserProfile>, bool> result = await target.GetUserListByUuid(userUuids);

        // Assert
        _decoratedServiceMock.Verify(service => service.GetUserListByUuid(It.IsAny<List<Guid>>()), Times.Once());
        result.Match(
            actual =>
            {
                Assert.Equal(2, actual.Count);
                Assert.Equal(userUuids[0], actual[0].UserUuid);
                Assert.Equal(userUuids[1], actual[1].UserUuid);
            },
            _ => { });

        Assert.True(memoryCache.TryGetValue($"UserId_UserUuid_{userUuids[0]}", out int _), "No data found in memory cache");
        Assert.True(memoryCache.TryGetValue($"UserId_UserUuid_{userUuids[1]}", out int _), "No data found in memory cache");
    }

    /// <summary>
    /// Tests that if the result from decorated service is null, nothing is stored in cache and the null object returned to caller.
    /// </summary>
    [Fact]
    public async Task GetUserUserUserId_ErrorResultFromDecoratedService_CacheNotPopulated()
    {
        // Arrange
        const int UserId = 2001607;
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        _decoratedServiceMock.Setup(service => service.GetUser(It.IsAny<int>())).ReturnsAsync(false);
        var target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

        // Act
        Result<UserProfile, bool> result = await target.GetUser(UserId);

        // Assert
        _decoratedServiceMock.Verify(service => service.GetUser(It.IsAny<int>()), Times.Once());
        Assert.True(result.IsError, "Expected an error result");

        Assert.False(memoryCache.TryGetValue("User_UserId_2001607", out UserProfile _), "Data unexpectedly found in memory cache");
    }

    /// <summary>
    /// Tests that if the result from decorated service is null, nothing is stored in cache and the null object returned to caller.
    /// </summary>
    [Fact]
    public async Task GetUserUserUserUuid_ErrorResultFromDecoratedService_CacheNotPopulated()
    {
        // Arrange
        Guid userUuid = new("cc86d2c7-1695-44b0-8e82-e633243fdf31");
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        _decoratedServiceMock.Setup(service => service.GetUserByUuid(It.IsAny<Guid>())).ReturnsAsync(false);
        var target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

        // Act
        Result<UserProfile, bool> result = await target.GetUserByUuid(userUuid);

        // Assert
        _decoratedServiceMock.Verify(service => service.GetUserByUuid(It.IsAny<Guid>()), Times.Once());
        Assert.True(result.IsError, "Expected an error result");
        Assert.False(memoryCache.TryGetValue($"User:UserUuid:{userUuid}", out UserProfile _), "Data unexpectedly found in memory cache");
    }

    /// <summary>
    /// Tests that if the result from decorated service is an empty list, nothing is stored in cache and the empty list returned to caller.
    /// </summary>
    [Fact]
    public async Task GetUserListUserUserUuid_EmptyListFromDecoratedService_CacheNotPopulated()
    {
        // Arrange
        List<Guid> userUuids = new List<Guid> { new("cc86d2c7-1695-44b0-8e82-e633243fdf31"), new("4c3b4909-eb17-45d5-bde1-256e065e196a") };
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        _decoratedServiceMock.Setup(service => service.GetUserListByUuid(It.IsAny<List<Guid>>())).ReturnsAsync(new List<UserProfile>());
        var target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

        // Act
        Result<List<UserProfile>, bool> result = await target.GetUserListByUuid(userUuids);

        // Assert
        _decoratedServiceMock.Verify(service => service.GetUserListByUuid(It.IsAny<List<Guid>>()), Times.Once);
        result.Match(
            Assert.Empty,
            _ => { });
        Assert.False(memoryCache.TryGetValue($"UserId_UserUuid_{userUuids[0]}", out int _));
        Assert.False(memoryCache.TryGetValue($"UserId_UserUuid_{userUuids[1]}", out int _));
    }

    /// <summary>
    /// Tests that the userprofile available in the cache is returned to the caller without forwarding request to decorated service.
    /// </summary>
    [Fact]
    public async Task GetUserUserSSN_UserInCache_decoratedServiceNotCalled()
    {
        // Arrange
        const string Ssn = "01025101038";
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        var userProfile = await TestDataLoader.Load<UserProfile>("2001607");
        memoryCache.Set("UserId_SSN_01025101038", userProfile.UserId);
        memoryCache.Set("User_UserId_" + userProfile.UserId, userProfile);
        var target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

        // Act
        Result<UserProfile, bool> result = await target.GetUser(Ssn);

        // Assert
        _decoratedServiceMock.Verify(service => service.GetUser(It.IsAny<string>()), Times.Never());
        Assert.True(result.IsSuccess, "Expected a success result");

        result.Match(
           actual =>
           {
               Assert.NotNull(actual);
               Assert.Equal(Ssn, actual.Party.SSN);
           },
           _ => { });
    }

    /// <summary>
    /// Tests that the userprofile available in the cache is returned to the caller without forwarding request to decorated service.
    /// </summary>
    [Fact]
    public async Task GetUserUserSSN_UserNotInCache_decoratedServiceCalledMockPopulated()
    {
        // Arrange
        const string Ssn = "01025101038";
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        var userProfile = await TestDataLoader.Load<UserProfile>("2001607");
        _decoratedServiceMock.Setup(service => service.GetUser(It.IsAny<string>())).ReturnsAsync(userProfile);

        var target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

        // Act
        Result<UserProfile, bool> result = await target.GetUser(Ssn);

        // Assert
        _decoratedServiceMock.Verify(service => service.GetUser(It.IsAny<string>()), Times.Once());
        Assert.True(result.IsSuccess, "Expected a success result");

        result.Match(
           actual =>
           {
               Assert.NotNull(actual);
               Assert.Equal(Ssn, actual.Party.SSN);
           },
           _ => { });

        Assert.True(memoryCache.TryGetValue("UserId_SSN_01025101038", out int _), "No data found in memory cache");
    }

    /// <summary>
    /// Tests that if the result from decorated service is null, nothing is stored in cache and the null object returned to caller.
    /// </summary>
    [Fact]
    public async Task GetUserUserSSN_ErrorResultFromDecoratedService_CacheNotPopulated()
    {
        // Arrange
        const string Ssn = "01025101037";
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        _decoratedServiceMock.Setup(service => service.GetUser(It.IsAny<string>())).ReturnsAsync(false);

        var target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

        // Act
        Result<UserProfile, bool> result = await target.GetUser(Ssn);

        // Assert
        _decoratedServiceMock.Verify(service => service.GetUser(It.IsAny<string>()), Times.Once());
        Assert.True(result.IsError, "Expected an error result");
        Assert.False(memoryCache.TryGetValue("User_UserId_2001607", out UserProfile _), "Data unexpectedly found in memory cache");
    }

    /// <summary>
    /// Tests that the userprofile available in the cache is returned to the caller without forwarding request to decorated service.
    /// </summary>
    [Fact]
    public async Task GetUserByUsername_UserInCache_decoratedServiceNotCalled()
    {
        // Arrange
        const string Username = "OrstaECUser";
        const int UserId = 2001072;
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        var userProfile = await TestDataLoader.Load<UserProfile>(Username);
        memoryCache.Set("UserId_Username_OrstaECUser", userProfile.UserId);
        memoryCache.Set("User_UserId_" + userProfile.UserId, userProfile);

        var target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

        // Act
        Result<UserProfile, bool> result = await target.GetUserByUsername(Username);

        // Assert
        _decoratedServiceMock.Verify(service => service.GetUserByUsername(It.IsAny<string>()), Times.Never());

        Assert.True(result.IsSuccess, "Expected a success result");
        result.Match(
           actual =>
           {
               Assert.NotNull(actual);
               Assert.Equal(UserId, actual.UserId);
               Assert.Equal(Username, actual.UserName);
           },
           _ => { });
    }

    /// <summary>
    /// Tests that when the userprofile is not available in the cache, the request is forwarded to the decorated service.
    /// </summary>
    [Fact]
    public async Task GetUserByUsername_UserNotInCache_decoratedServiceCalledMockPopulated()
    {
        // Arrange
        const string Username = "OrstaECUser";
        const int UserId = 2001072;

        MemoryCache memoryCache = new(new MemoryCacheOptions());

        var userProfile = await TestDataLoader.Load<UserProfile>(Username);
        _decoratedServiceMock.Setup(service => service.GetUserByUsername(Username)).ReturnsAsync(userProfile);
        var target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

        // Act
        Result<UserProfile, bool> result = await target.GetUserByUsername(Username);

        // Assert
        _decoratedServiceMock.Verify(service => service.GetUserByUsername(Username), Times.Once());
        Assert.True(result.IsSuccess, "Expected a success result");
        result.Match(
           actual =>
           {
               Assert.NotNull(actual);
               Assert.Equal(UserId, actual.UserId);
               Assert.Equal(Username, actual.UserName);
           },
           _ => { });

        Assert.True(memoryCache.TryGetValue("UserId_Username_OrstaECUser", out int _), "No data found in memory cache");
        Assert.True(memoryCache.TryGetValue("User_UserId_" + UserId, out UserProfile _), "No data found in memory cache");
    }

    /// <summary>
    /// Tests that if the result from decorated service is null, nothing is stored in cache and the null object returned to caller.
    /// </summary>
    [Fact]
    public async Task GetUserByUsername_ErrorResultFromDecoratedService_CacheNotPopulated()
    {
        // Arrange
        const string Username = "NonExistingUsername";
        MemoryCache memoryCache = new(new MemoryCacheOptions());

        _decoratedServiceMock.Setup(service => service.GetUserByUsername(Username)).ReturnsAsync(false);
        var target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

        // Act
        Result<UserProfile, bool> result = await target.GetUserByUsername(Username);

        // Assert
        _decoratedServiceMock.Verify(service => service.GetUserByUsername(Username), Times.Once());
        Assert.True(result.IsError, "Expected an error result");
        Assert.False(memoryCache.TryGetValue("User_Username_NonExistingUsername", out UserProfile _), "Data unexpectedly found in memory cache");
    }
}
