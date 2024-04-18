using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Altinn.Platform.Profile.Models;
using Altinn.Profile.Core;
using Altinn.Profile.Core.User;
using Altinn.Profile.Tests.Testdata;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.Core.User
{
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
            UserProfile actual = await target.GetUser(UserId);

            // Assert
            _decoratedServiceMock.Verify(service => service.GetUser(It.IsAny<int>()), Times.Never());
            Assert.NotNull(actual);
            Assert.Equal(UserId, actual.UserId);
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
            memoryCache.Set($"User:UserUuid:{userUuid}", userProfile);
            var target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

            // Act
            UserProfile actual = await target.GetUserByUuid(userUuid);

            // Assert
            _decoratedServiceMock.Verify(service => service.GetUserByUuid(It.IsAny<Guid>()), Times.Never());
            Assert.NotNull(actual);
            Assert.Equal(userUuid, actual.UserUuid);
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
            memoryCache.Set($"User:UserUuid:{userUuids[0]}", userProfile);
            List<UserProfile> userProfiles = new List<UserProfile>();
            userProfiles.Add(await TestDataLoader.Load<UserProfile>(userUuidNotInCache.ToString()));
            _decoratedServiceMock.Setup(service => service.GetUserListByUuid(It.Is<List<Guid>>(g => g.TrueForAll(g2 => g2 == userUuidNotInCache)))).ReturnsAsync(userProfiles);
            UserProfileCachingDecorator target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

            // Act
            List<UserProfile> actual = await target.GetUserListByUuid(userUuids);

            // Assert
            _decoratedServiceMock.Verify(service => service.GetUserListByUuid(It.Is<List<Guid>>(g => g.TrueForAll(g2 => g2 == userUuidNotInCache))), Times.Once);
            Assert.NotNull(actual);
            foreach (var userUuid in userUuids)
            {
                UserProfile currentProfileFromResult = actual.Find(p => p.UserUuid == userUuid);
                UserProfile currentProfileFromCache = memoryCache.Get<UserProfile>($"User:UserUuid:{userUuid}");
                Assert.NotNull(currentProfileFromResult);
                Assert.NotNull(currentProfileFromCache);
            }
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
                memoryCache.Set($"User:UserUuid:{userUuid}", userProfile);
            }

            UserProfileCachingDecorator target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

            // Act
            List<UserProfile> actual = await target.GetUserListByUuid(userUuids);

            // Assert
            _decoratedServiceMock.Verify(service => service.GetUserListByUuid(It.IsAny<List<Guid>>()), Times.Never());
            Assert.NotNull(actual);
            foreach (var userUuid in userUuids)
            {
                UserProfile currentProfileFromResult = actual.Find(p => p.UserUuid == userUuid);
                Assert.NotNull(currentProfileFromResult);
            }
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
            UserProfile actual = await target.GetUser(UserId);

            // Assert
            _decoratedServiceMock.Verify(service => service.GetUser(It.IsAny<int>()), Times.Once());

            Assert.NotNull(actual);
            Assert.Equal(UserId, actual.UserId);
            Assert.True(memoryCache.TryGetValue("User_UserId_2001607", out UserProfile _));
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
            UserProfile actual = await target.GetUserByUuid(userUuid);

            // Assert
            _decoratedServiceMock.Verify(service => service.GetUserByUuid(It.IsAny<Guid>()), Times.Once());

            Assert.NotNull(actual);
            Assert.Equal(userUuid, actual.UserUuid);
            Assert.True(memoryCache.TryGetValue($"User:UserUuid:{userUuid}", out UserProfile _));
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
            List<UserProfile> actual = await target.GetUserListByUuid(userUuids);

            // Assert
            _decoratedServiceMock.Verify(service => service.GetUserListByUuid(It.IsAny<List<Guid>>()), Times.Once());

            Assert.Equal(2, actual.Count);
            Assert.Equal(userUuids[0], actual[0].UserUuid);
            Assert.Equal(userUuids[1], actual[1].UserUuid);
            Assert.True(memoryCache.TryGetValue($"User:UserUuid:{userUuids[0]}", out UserProfile _));
            Assert.True(memoryCache.TryGetValue($"User:UserUuid:{userUuids[1]}", out UserProfile _));
        }

        /// <summary>
        /// Tests that if the result from decorated service is null, nothing is stored in cache and the null object returned to caller.
        /// </summary>
        [Fact]
        public async Task GetUserUserUserId_NullFromDecoratedService_CacheNotPopulated()
        {
            // Arrange
            const int UserId = 2001607;
            MemoryCache memoryCache = new(new MemoryCacheOptions());

            _decoratedServiceMock.Setup(service => service.GetUser(It.IsAny<int>())).ReturnsAsync((UserProfile)null);
            var target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

            // Act
            UserProfile actual = await target.GetUser(UserId);

            // Assert
            _decoratedServiceMock.Verify(service => service.GetUser(It.IsAny<int>()), Times.Once());
            Assert.Null(actual);
            Assert.False(memoryCache.TryGetValue("User_UserId_2001607", out UserProfile _));
        }

        /// <summary>
        /// Tests that if the result from decorated service is null, nothing is stored in cache and the null object returned to caller.
        /// </summary>
        [Fact]
        public async Task GetUserUserUserUuid_NullFromDecoratedService_CacheNotPopulated()
        {
            // Arrange
            Guid userUuid = new("cc86d2c7-1695-44b0-8e82-e633243fdf31");
            MemoryCache memoryCache = new(new MemoryCacheOptions());

            _decoratedServiceMock.Setup(service => service.GetUserByUuid(It.IsAny<Guid>())).ReturnsAsync((UserProfile)null);
            var target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

            // Act
            UserProfile actual = await target.GetUserByUuid(userUuid);

            // Assert
            _decoratedServiceMock.Verify(service => service.GetUserByUuid(It.IsAny<Guid>()), Times.Once());
            Assert.Null(actual);
            Assert.False(memoryCache.TryGetValue($"User:UserUuid:{userUuid}", out UserProfile _));
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
            List<UserProfile> actual = await target.GetUserListByUuid(userUuids);

            // Assert
            _decoratedServiceMock.Verify(service => service.GetUserListByUuid(It.IsAny<List<Guid>>()), Times.Once);
            Assert.Empty(actual);
            Assert.False(memoryCache.TryGetValue($"User:UserUuid:{userUuids[0]}", out UserProfile _));
            Assert.False(memoryCache.TryGetValue($"User:UserUuid:{userUuids[1]}", out UserProfile _));
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
            memoryCache.Set("User_SSN_01025101038", userProfile);
            var target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

            // Act
            UserProfile actual = await target.GetUser(Ssn);

            // Assert
            _decoratedServiceMock.Verify(service => service.GetUser(It.IsAny<string>()), Times.Never());
            Assert.NotNull(actual);
            Assert.Equal(Ssn, actual.Party.SSN);
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
            UserProfile actual = await target.GetUser(Ssn);

            // Assert
            _decoratedServiceMock.Verify(service => service.GetUser(It.IsAny<string>()), Times.Once());
            Assert.NotNull(actual);
            Assert.Equal(Ssn, actual.Party.SSN);
            Assert.True(memoryCache.TryGetValue("User_SSN_01025101038", out UserProfile _));
        }

        /// <summary>
        /// Tests that if the result from decorated service is null, nothing is stored in cache and the null object returned to caller.
        /// </summary>
        [Fact]
        public async Task GetUserUserSSN_NullFromDecoratedService_CacheNotPopulated()
        {
            // Arrange
            const string Ssn = "01025101037";
            MemoryCache memoryCache = new(new MemoryCacheOptions());

            _decoratedServiceMock.Setup(service => service.GetUser(It.IsAny<string>())).ReturnsAsync((UserProfile)null);

            var target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

            // Act
            UserProfile actual = await target.GetUser(Ssn);

            // Assert
            _decoratedServiceMock.Verify(service => service.GetUser(It.IsAny<string>()), Times.Once());
            Assert.Null(actual);
            Assert.False(memoryCache.TryGetValue("User_UserId_2001607", out UserProfile _));
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
            memoryCache.Set("User_Username_OrstaECUser", userProfile);
            var target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

            // Act
            UserProfile actual = await target.GetUserByUsername(Username);

            // Assert
            _decoratedServiceMock.Verify(service => service.GetUser(It.IsAny<int>()), Times.Never());
            Assert.NotNull(actual);
            Assert.Equal(UserId, actual.UserId);
            Assert.Equal(Username, actual.UserName);
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
            UserProfile actual = await target.GetUserByUsername(Username);

            // Assert
            _decoratedServiceMock.Verify(service => service.GetUserByUsername(Username), Times.Once());

            Assert.NotNull(actual);
            Assert.Equal(UserId, actual.UserId);
            Assert.Equal(Username, actual.UserName);
            Assert.True(memoryCache.TryGetValue("User_Username_OrstaECUser", out UserProfile _));
        }

        /// <summary>
        /// Tests that if the result from decorated service is null, nothing is stored in cache and the null object returned to caller.
        /// </summary>
        [Fact]
        public async Task GetUserByUsername_NullFromDecoratedService_CacheNotPopulated()
        {
            // Arrange
            const string Username = "NonExistingUsername";
            MemoryCache memoryCache = new(new MemoryCacheOptions());

            _decoratedServiceMock.Setup(service => service.GetUserByUsername(Username)).ReturnsAsync((UserProfile)null);
            var target = new UserProfileCachingDecorator(_decoratedServiceMock.Object, memoryCache, coreSettingsOptions.Object);

            // Act
            UserProfile actual = await target.GetUserByUsername(Username);

            // Assert
            _decoratedServiceMock.Verify(service => service.GetUserByUsername(Username), Times.Once());
            Assert.Null(actual);
            Assert.False(memoryCache.TryGetValue("User_Username_NonExistingUsername", out UserProfile _));
        }
    }
}
