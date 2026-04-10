using System;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core.User.ContactInfo;
using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories;

using Microsoft.EntityFrameworkCore;

using Moq;

using Wolverine;
using Wolverine.EntityFrameworkCore;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Repositories;

public class UserContactInfoRepositoryTests
{
    private readonly Mock<IDbContextOutbox> _dbContextOutboxMock = new();

    private class TestDbContextFactory(DbContextOptions<ProfileDbContext> options) : IDbContextFactory<ProfileDbContext>
    {
        private readonly DbContextOptions<ProfileDbContext> _options = options;

        public ProfileDbContext CreateDbContext() => new(_options);

        public Task<ProfileDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => Task.FromResult(new ProfileDbContext(_options));
    }

    private static DbContextOptions<ProfileDbContext> CreateOptions(string DBName)
    {
        return new DbContextOptionsBuilder<ProfileDbContext>()
            .UseInMemoryDatabase(databaseName: DBName)
            .Options;
    }

    [Fact]
    public async Task CreateUserContactInfo_WhenUserWithSameIdAlreadyExists_Throws()
    {
        // Arrange
        SiUserContactInfoAddedEvent actualEventRaised = null;
        void EventRaisingCallback(SiUserContactInfoAddedEvent ev, DeliveryOptions opts) => actualEventRaised = ev;
        MockDbContextOutbox((Action<SiUserContactInfoAddedEvent, DeliveryOptions>)EventRaisingCallback);

        var options = CreateOptions(nameof(CreateUserContactInfo_WhenUserWithSameIdAlreadyExists_Throws));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory, _dbContextOutboxMock.Object);

        await using var seedContext = new ProfileDbContext(options);
        var existingUserContactInfo = new UserContactInfo()
        {
            UserId = 1,
            UserUuid = Guid.NewGuid(),
            Username = "foobar",
            CreatedAt = DateTime.Now.AddMinutes(-2),
            EmailAddress = "some@email.com",
            PhoneNumber = null,
            PhoneNumberLastChanged = null
        };
        seedContext.SelfIdentifiedUsers.Add(existingUserContactInfo);
        await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var userContactInfoToCreate = new UserContactInfoCreateModel()
        {
            UserId = 1,
            UserUuid = Guid.NewGuid(),
            Username = "barfoo",
            EmailAddress = "some@email.com"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UserContactInfoAlreadyExistsException>(() =>
            repository.CreateUserContactInfo(userContactInfoToCreate, TestContext.Current.CancellationToken));

        _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<SiUserContactInfoAddedEvent>(), It.IsAny<DeliveryOptions>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserContactInfo_WhenPhoneNumberIsIncluded_SetsCorrectPropertiesInDbRecord()
    {
        // Arrange
        SiUserContactInfoAddedEvent actualEventRaised = null;
        void EventRaisingCallback(SiUserContactInfoAddedEvent ev, DeliveryOptions opts) => actualEventRaised = ev;
        MockDbContextOutbox((Action<SiUserContactInfoAddedEvent, DeliveryOptions>)EventRaisingCallback);

        var options = CreateOptions(nameof(CreateUserContactInfo_WhenPhoneNumberIsIncluded_SetsCorrectPropertiesInDbRecord));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory, _dbContextOutboxMock.Object);

        var userContactInfoToCreate = new UserContactInfoCreateModel()
        {
            UserId = 2,
            UserUuid = Guid.NewGuid(),
            Username = "barfoo",
            EmailAddress = "some@email.com",
            PhoneNumber = "+4798765432"
        };

        // Act
        var before = DateTime.UtcNow;
        var result = await repository.CreateUserContactInfo(userContactInfoToCreate, TestContext.Current.CancellationToken);
        var after = DateTime.UtcNow;

        // Assert
        await using var assertContext = new ProfileDbContext(options);
        var updatedUserContactInfo = await assertContext.SelfIdentifiedUsers.FirstOrDefaultAsync(
            u => u.UserId == 2,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(updatedUserContactInfo);
        Assert.InRange(updatedUserContactInfo.CreatedAt, before, after);
        Assert.Equal(userContactInfoToCreate.UserUuid, updatedUserContactInfo.UserUuid);
        Assert.Equal(userContactInfoToCreate.Username, updatedUserContactInfo.Username);
        Assert.Equal(userContactInfoToCreate.EmailAddress, updatedUserContactInfo.EmailAddress);
        Assert.Equal(userContactInfoToCreate.PhoneNumber, updatedUserContactInfo.PhoneNumber);
        Assert.NotNull(updatedUserContactInfo.PhoneNumberLastChanged);
        Assert.InRange(updatedUserContactInfo.PhoneNumberLastChanged.Value, before, after);

        _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<SiUserContactInfoAddedEvent>(), It.IsAny<DeliveryOptions>()), Times.Once);
        Assert.NotNull(actualEventRaised);
    }

    [Fact]
    public async Task CreateUserContactInfo_WhenPhoneNumberIsIncluded_ReturnsModelWithCorrectProperties()
    {
        // Arrange
        SiUserContactInfoAddedEvent actualEventRaised = null;
        void EventRaisingCallback(SiUserContactInfoAddedEvent ev, DeliveryOptions opts) => actualEventRaised = ev;
        MockDbContextOutbox((Action<SiUserContactInfoAddedEvent, DeliveryOptions>)EventRaisingCallback);

        var options = CreateOptions(nameof(CreateUserContactInfo_WhenPhoneNumberIsIncluded_ReturnsModelWithCorrectProperties));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory, _dbContextOutboxMock.Object);
        var userContactInfoToCreate = new UserContactInfoCreateModel()
        {
            UserId = 3,
            UserUuid = Guid.NewGuid(),
            Username = "barfoo",
            EmailAddress = "some@email.com",
            PhoneNumber = "+4798765432"
        };

        // Act
        var before = DateTime.UtcNow;
        var result = await repository.CreateUserContactInfo(userContactInfoToCreate, TestContext.Current.CancellationToken);
        var after = DateTime.UtcNow;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userContactInfoToCreate.UserId, result.UserId);
        Assert.Equal(userContactInfoToCreate.UserUuid, result.UserUuid);
        Assert.Equal(userContactInfoToCreate.Username, result.Username);
        Assert.Equal(userContactInfoToCreate.EmailAddress, result.EmailAddress);
        Assert.Equal(userContactInfoToCreate.PhoneNumber, result.PhoneNumber);
        Assert.NotNull(result.PhoneNumberLastChanged);
        Assert.InRange(result.PhoneNumberLastChanged.Value, before, after);

        _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<SiUserContactInfoAddedEvent>(), It.IsAny<DeliveryOptions>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserContactInfo_WhenPhoneNumberIsExcluded_SetsCorrectPropertiesInDbRecord()
    {
        // Arrange
        SiUserContactInfoAddedEvent actualEventRaised = null;
        void EventRaisingCallback(SiUserContactInfoAddedEvent ev, DeliveryOptions opts) => actualEventRaised = ev;
        MockDbContextOutbox((Action<SiUserContactInfoAddedEvent, DeliveryOptions>)EventRaisingCallback);

        var options = CreateOptions(nameof(CreateUserContactInfo_WhenPhoneNumberIsExcluded_SetsCorrectPropertiesInDbRecord));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory, _dbContextOutboxMock.Object);
        var userContactInfoToCreate = new UserContactInfoCreateModel()
        {
            UserId = 4,
            UserUuid = Guid.NewGuid(),
            Username = "barfoo",
            EmailAddress = "some@email.com",
        };

        // Act
        var before = DateTime.UtcNow;
        var result = await repository.CreateUserContactInfo(userContactInfoToCreate, TestContext.Current.CancellationToken);
        var after = DateTime.UtcNow;

        // Assert
        await using var assertContext = new ProfileDbContext(options);
        var updatedUserContactInfo = await assertContext.SelfIdentifiedUsers.FirstOrDefaultAsync(
            u => u.UserId == 4,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(updatedUserContactInfo);
        Assert.InRange(updatedUserContactInfo.CreatedAt, before, after);
        Assert.Equal(userContactInfoToCreate.UserUuid, updatedUserContactInfo.UserUuid);
        Assert.Equal(userContactInfoToCreate.Username, updatedUserContactInfo.Username);
        Assert.Equal(userContactInfoToCreate.EmailAddress, updatedUserContactInfo.EmailAddress);
        Assert.Null(updatedUserContactInfo.PhoneNumber);
        Assert.Null(updatedUserContactInfo.PhoneNumberLastChanged);

        _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<SiUserContactInfoAddedEvent>(), It.IsAny<DeliveryOptions>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserContactInfo_WhenPhoneNumberIsExcluded_ReturnsModelWithCorrectProperties()
    {
        // Arrange
        SiUserContactInfoAddedEvent actualEventRaised = null;
        void EventRaisingCallback(SiUserContactInfoAddedEvent ev, DeliveryOptions opts) => actualEventRaised = ev;
        MockDbContextOutbox((Action<SiUserContactInfoAddedEvent, DeliveryOptions>)EventRaisingCallback);

        var options = CreateOptions(nameof(CreateUserContactInfo_WhenPhoneNumberIsExcluded_ReturnsModelWithCorrectProperties));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory, _dbContextOutboxMock.Object);
        var userContactInfoToCreate = new UserContactInfoCreateModel()
        {
            UserId = 5,
            UserUuid = Guid.NewGuid(),
            Username = "barfoo",
            EmailAddress = "some@email.com"
        };

        // Act
        var before = DateTime.UtcNow;
        var result = await repository.CreateUserContactInfo(userContactInfoToCreate, TestContext.Current.CancellationToken);
        var after = DateTime.UtcNow;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userContactInfoToCreate.UserId, result.UserId);
        Assert.Equal(userContactInfoToCreate.UserUuid, result.UserUuid);
        Assert.Equal(userContactInfoToCreate.Username, result.Username);
        Assert.Equal(userContactInfoToCreate.EmailAddress, result.EmailAddress);
        Assert.Null(result.PhoneNumber);
        Assert.Null(result.PhoneNumberLastChanged);

        _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<SiUserContactInfoAddedEvent>(), It.IsAny<DeliveryOptions>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePhoneNumber_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        SiUserContactInfoUpdatedEvent actualEventRaised = null;
        void EventRaisingCallback(SiUserContactInfoUpdatedEvent ev, DeliveryOptions opts) => actualEventRaised = ev;
        MockDbContextOutbox((Action<SiUserContactInfoUpdatedEvent, DeliveryOptions>)EventRaisingCallback);

        var options = CreateOptions(nameof(UpdatePhoneNumber_WhenUserDoesNotExist_ReturnsNull));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory, _dbContextOutboxMock.Object);

        // Act
        var result = await repository.UpdatePhoneNumber(6, "+4798765431", CancellationToken.None);

        // Assert
        Assert.Null(result);

        _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<SiUserContactInfoUpdatedEvent>(), It.IsAny<DeliveryOptions>()), Times.Never);
    }

    [Fact]
    public async Task UpdatePhoneNumber_WhenUserExists_UpdatesPhoneNumber()
    {
        // Arrange
        SiUserContactInfoUpdatedEvent actualEventRaised = null;
        void EventRaisingCallback(SiUserContactInfoUpdatedEvent ev, DeliveryOptions opts) => actualEventRaised = ev;
        MockDbContextOutbox((Action<SiUserContactInfoUpdatedEvent, DeliveryOptions>)EventRaisingCallback);

        var options = CreateOptions(nameof(UpdatePhoneNumber_WhenUserExists_UpdatesPhoneNumber));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory, _dbContextOutboxMock.Object);

        int testUserId = 7;
        string existingNumber = "+4798765431";
        string newNumber = "+4798765432";

        await using (var seedContext = new ProfileDbContext(options))
        {
            var userContactInfo = new UserContactInfo()
            {
                UserId = testUserId,
                UserUuid = Guid.NewGuid(),
                Username = "foobar",
                CreatedAt = DateTime.Now.AddMinutes(-2),
                EmailAddress = "some@email.com",
                PhoneNumber = existingNumber,
                PhoneNumberLastChanged = DateTime.Now.AddMinutes(-1)
            };
            seedContext.SelfIdentifiedUsers.Add(userContactInfo);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Act
        var result = await repository.UpdatePhoneNumber(testUserId, newNumber, CancellationToken.None);

        // Assert
        await using var assertContext = new ProfileDbContext(options);
        var updatedUserContactInfo = await assertContext.SelfIdentifiedUsers.FirstOrDefaultAsync(
            u => u.UserId == testUserId,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(updatedUserContactInfo);
        Assert.Equal(newNumber, updatedUserContactInfo.PhoneNumber);

        _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<SiUserContactInfoUpdatedEvent>(), It.IsAny<DeliveryOptions>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePhoneNumber_WhenUserExists_UpdatesPhoneNumberRegisteredToNow()
    {
        // Arrange
        SiUserContactInfoUpdatedEvent actualEventRaised = null;
        void EventRaisingCallback(SiUserContactInfoUpdatedEvent ev, DeliveryOptions opts) => actualEventRaised = ev;
        MockDbContextOutbox((Action<SiUserContactInfoUpdatedEvent, DeliveryOptions>)EventRaisingCallback);

        var options = CreateOptions(nameof(UpdatePhoneNumber_WhenUserExists_UpdatesPhoneNumberRegisteredToNow));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory, _dbContextOutboxMock.Object);

        int testUserId = 8;

        await using (var seedContext = new ProfileDbContext(options))
        {
            var userContactInfo = new UserContactInfo()
            {
                UserId = testUserId,
                UserUuid = Guid.NewGuid(),
                Username = "foobar",
                CreatedAt = DateTime.Now.AddMinutes(-2),
                EmailAddress = "some@mail.no",
                PhoneNumber = "+4798765430",
                PhoneNumberLastChanged = DateTime.Now.AddMinutes(-1)
            };
            seedContext.SelfIdentifiedUsers.Add(userContactInfo);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Act
        var before = DateTime.UtcNow;
        var result = await repository.UpdatePhoneNumber(testUserId, "+4798765433", CancellationToken.None);
        var after = DateTime.UtcNow;

        // Assert
        await using var assertContext = new ProfileDbContext(options);
        var updatedUserContactInfo = await assertContext.SelfIdentifiedUsers.FirstOrDefaultAsync(
            u => u.UserId == testUserId,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(updatedUserContactInfo);
        Assert.NotNull(updatedUserContactInfo.PhoneNumberLastChanged);
        Assert.InRange(updatedUserContactInfo.PhoneNumberLastChanged.Value, before, after);

        _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<SiUserContactInfoUpdatedEvent>(), It.IsAny<DeliveryOptions>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePhoneNumber_WhenUserExists_ReturnsUpdatedContactInfo()
    {
        // Arrange
        SiUserContactInfoUpdatedEvent actualEventRaised = null;
        void EventRaisingCallback(SiUserContactInfoUpdatedEvent ev, DeliveryOptions opts) => actualEventRaised = ev;
        MockDbContextOutbox((Action<SiUserContactInfoUpdatedEvent, DeliveryOptions>)EventRaisingCallback);

        var options = CreateOptions(nameof(UpdatePhoneNumber_WhenUserExists_ReturnsUpdatedContactInfo));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory, _dbContextOutboxMock.Object);

        int testUserId = 9;

        await using var seedContext = new ProfileDbContext(options);
        var userContactInfo = new UserContactInfo()
        {
            UserId = testUserId,
            UserUuid = Guid.NewGuid(),
            Username = "foobar",
            CreatedAt = DateTime.Now.AddMinutes(-2),
            EmailAddress = "some@mail.no",
            PhoneNumber = "+4798765430",
            PhoneNumberLastChanged = DateTime.Now.AddMinutes(-1)
        };
        seedContext.SelfIdentifiedUsers.Add(userContactInfo);
        await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var before = DateTime.UtcNow;
        var result = await repository.UpdatePhoneNumber(testUserId, "+4798765433", CancellationToken.None);
        var after = DateTime.UtcNow;

        // Assert
        Assert.NotNull(result);
        Assert.Equal("+4798765433", result.PhoneNumber);
        Assert.NotNull(result.PhoneNumberLastChanged);
        Assert.InRange(result.PhoneNumberLastChanged.Value, before, after);

        _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<SiUserContactInfoUpdatedEvent>(), It.IsAny<DeliveryOptions>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePhoneNumber_WhenNumberIsRemoved_ReturnsUpdatedContactInfo()
    {
        // Arrange
        SiUserContactInfoUpdatedEvent actualEventRaised = null;
        void EventRaisingCallback(SiUserContactInfoUpdatedEvent ev, DeliveryOptions opts) => actualEventRaised = ev;
        MockDbContextOutbox((Action<SiUserContactInfoUpdatedEvent, DeliveryOptions>)EventRaisingCallback);

        var options = CreateOptions(nameof(UpdatePhoneNumber_WhenNumberIsRemoved_ReturnsUpdatedContactInfo));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory, _dbContextOutboxMock.Object);

        int testUserId = 11;

        await using var seedContext = new ProfileDbContext(options);
        var userContactInfo = new UserContactInfo()
        {
            UserId = testUserId,
            UserUuid = Guid.NewGuid(),
            Username = "foobar",
            CreatedAt = DateTime.Now.AddMinutes(-2),
            EmailAddress = "some@mail.no",
            PhoneNumber = "+4798765430",
            PhoneNumberLastChanged = DateTime.Now.AddMinutes(-1)
        };
        seedContext.SelfIdentifiedUsers.Add(userContactInfo);
        await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var before = DateTime.UtcNow;
        var result = await repository.UpdatePhoneNumber(testUserId, null, CancellationToken.None);
        var after = DateTime.UtcNow;

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.PhoneNumber);
        Assert.NotNull(result.PhoneNumberLastChanged);
        Assert.InRange(result.PhoneNumberLastChanged.Value, before, after);

        _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<SiUserContactInfoUpdatedEvent>(), It.IsAny<DeliveryOptions>()), Times.Once);
        Assert.NotNull(actualEventRaised.PhoneNumber);
        Assert.Equal(string.Empty, actualEventRaised.PhoneNumber);
    }

    [Fact]
    public async Task UpdatePhoneNumber_WhenUserAndNumberAlreadyExists_DoesNotUpdateLastChanged()
    {
        // Arrange
        SiUserContactInfoUpdatedEvent actualEventRaised = null;
        void EventRaisingCallback(SiUserContactInfoUpdatedEvent ev, DeliveryOptions opts) => actualEventRaised = ev;
        MockDbContextOutbox((Action<SiUserContactInfoUpdatedEvent, DeliveryOptions>)EventRaisingCallback);

        var options = CreateOptions(nameof(UpdatePhoneNumber_WhenUserAndNumberAlreadyExists_DoesNotUpdateLastChanged));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory, _dbContextOutboxMock.Object);

        int testUserId = 10;
        var existingNumberLastChanged = DateTime.Now.AddMinutes(-1);

        await using var seedContext = new ProfileDbContext(options);
        var userContactInfo = new UserContactInfo()
        {
            UserId = testUserId,
            UserUuid = Guid.NewGuid(),
            Username = "foobar",
            CreatedAt = DateTime.Now.AddMinutes(-2),
            EmailAddress = "some@mail.no",
            PhoneNumber = "+4798765430",
            PhoneNumberLastChanged = existingNumberLastChanged
        };
        seedContext.SelfIdentifiedUsers.Add(userContactInfo);
        await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await repository.UpdatePhoneNumber(testUserId, "+4798765430", CancellationToken.None);

        // Assert
        Assert.Equal(existingNumberLastChanged.Ticks, result.PhoneNumberLastChanged.Value.Ticks);

        _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<SiUserContactInfoUpdatedEvent>(), It.IsAny<DeliveryOptions>()), Times.Never);
    }

    private void MockDbContextOutbox<TEvent>(Action<TEvent, DeliveryOptions> callback)
    {
        DbContext context = null;

        _dbContextOutboxMock
            .Setup(mock => mock.Enroll(It.IsAny<DbContext>()))
            .Callback<DbContext>(ctx =>
            {
                context = ctx;
            });

        _dbContextOutboxMock
            .Setup(mock => mock.SaveChangesAndFlushMessagesAsync(It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken ct) =>
            {
                await context.SaveChangesAsync(ct);
            });

        _dbContextOutboxMock
            .Setup(mock => mock.PublishAsync(It.IsAny<TEvent>(), It.IsAny<DeliveryOptions>()))
            .Callback(callback);
    }
}
