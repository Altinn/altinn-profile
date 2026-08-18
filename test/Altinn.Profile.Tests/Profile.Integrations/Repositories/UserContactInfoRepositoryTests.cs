using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core.User.ContactInfo;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Repositories;

public class UserContactInfoRepositoryTests
{
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
        var options = CreateOptions(nameof(CreateUserContactInfo_WhenUserWithSameIdAlreadyExists_Throws));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory);

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
    }

    [Fact]
    public async Task CreateUserContactInfo_WhenPhoneNumberIsIncluded_SetsCorrectPropertiesInDbRecord()
    {
        // Arrange
        var options = CreateOptions(nameof(CreateUserContactInfo_WhenPhoneNumberIsIncluded_SetsCorrectPropertiesInDbRecord));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory);

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
    }

    [Fact]
    public async Task CreateUserContactInfo_WhenPhoneNumberIsIncluded_ReturnsModelWithCorrectProperties()
    {
        // Arrange
        var options = CreateOptions(nameof(CreateUserContactInfo_WhenPhoneNumberIsIncluded_ReturnsModelWithCorrectProperties));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory);
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
    }

    [Fact]
    public async Task CreateUserContactInfo_WhenPhoneNumberIsExcluded_SetsCorrectPropertiesInDbRecord()
    {
        // Arrange
        var options = CreateOptions(nameof(CreateUserContactInfo_WhenPhoneNumberIsExcluded_SetsCorrectPropertiesInDbRecord));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory);
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
    }

    [Fact]
    public async Task CreateUserContactInfo_WhenPhoneNumberIsExcluded_ReturnsModelWithCorrectProperties()
    {
        // Arrange
        var options = CreateOptions(nameof(CreateUserContactInfo_WhenPhoneNumberIsExcluded_ReturnsModelWithCorrectProperties));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory);
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
    }

    [Fact]
    public async Task UpdatePhoneNumber_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var options = CreateOptions(nameof(UpdatePhoneNumber_WhenUserDoesNotExist_ReturnsNull));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory);

        // Act
        var result = await repository.UpdatePhoneNumber(6, "+4798765431", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdatePhoneNumber_WhenUserExists_UpdatesPhoneNumber()
    {
        // Arrange
        var options = CreateOptions(nameof(UpdatePhoneNumber_WhenUserExists_UpdatesPhoneNumber));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory);

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
    }

    [Fact]
    public async Task UpdatePhoneNumber_WhenUserExists_UpdatesPhoneNumberRegisteredToNow()
    {
        // Arrange
        var options = CreateOptions(nameof(UpdatePhoneNumber_WhenUserExists_UpdatesPhoneNumberRegisteredToNow));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory);

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
    }

    [Fact]
    public async Task UpdatePhoneNumber_WhenUserExists_ReturnsUpdatedContactInfo()
    {
        // Arrange
        var options = CreateOptions(nameof(UpdatePhoneNumber_WhenUserExists_ReturnsUpdatedContactInfo));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory);

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
    }

    [Fact]
    public async Task UpdatePhoneNumber_WhenNumberIsRemoved_ReturnsUpdatedContactInfo()
    {
        // Arrange
        var options = CreateOptions(nameof(UpdatePhoneNumber_WhenNumberIsRemoved_ReturnsUpdatedContactInfo));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory);

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
    }

    [Fact]
    public async Task UpdatePhoneNumber_WhenUserAndNumberAlreadyExists_DoesNotUpdateLastChanged()
    {
        // Arrange
        var options = CreateOptions(nameof(UpdatePhoneNumber_WhenUserAndNumberAlreadyExists_DoesNotUpdateLastChanged));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory);

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
    }

    [Fact]
    public async Task Get_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var options = CreateOptions(nameof(Get_WhenUserDoesNotExist_ReturnsNull));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory);

        // Act
        var result = await repository.Get(12, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Get_WhenUserExists_ReturnsContactInfo()
    {
        // Arrange
        var options = CreateOptions(nameof(Get_WhenUserExists_ReturnsContactInfo));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory);

        int testUserId = 13;
        var expectedUserUuid = Guid.NewGuid();
        var expectedCreatedAt = DateTime.UtcNow.AddMinutes(-2);
        var expectedPhoneNumberLastChanged = DateTime.UtcNow.AddMinutes(-1);

        await using var seedContext = new ProfileDbContext(options);
        seedContext.SelfIdentifiedUsers.Add(new UserContactInfo()
        {
            UserId = testUserId,
            UserUuid = expectedUserUuid,
            Username = "foobar",
            CreatedAt = expectedCreatedAt,
            EmailAddress = "some@mail.no",
            PhoneNumber = "+4798765430",
            PhoneNumberLastChanged = expectedPhoneNumberLastChanged
        });
        await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await repository.Get(testUserId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(testUserId, result.UserId);
        Assert.Equal(expectedUserUuid, result.UserUuid);
        Assert.Equal("foobar", result.Username);
        Assert.Equal(expectedCreatedAt, result.CreatedAt);
        Assert.Equal("some@mail.no", result.EmailAddress);
        Assert.Equal("+4798765430", result.PhoneNumber);
        Assert.Equal(expectedPhoneNumberLastChanged, result.PhoneNumberLastChanged);
    }

    [Fact]
    public async Task GetByUsername_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var options = CreateOptions(nameof(GetByUsername_WhenUserDoesNotExist_ReturnsNull));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory);

        // Act
        var result = await repository.GetByUsername("missing-user", CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByUsername_WhenUserExists_ReturnsContactInfo()
    {
        // Arrange
        var options = CreateOptions(nameof(GetByUsername_WhenUserExists_ReturnsContactInfo));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory);

        const string Username = "foobar";
        var expectedUserUuid = Guid.NewGuid();
        var expectedCreatedAt = DateTime.UtcNow.AddMinutes(-2);
        var expectedPhoneNumberLastChanged = DateTime.UtcNow.AddMinutes(-1);

        await using var seedContext = new ProfileDbContext(options);
        seedContext.SelfIdentifiedUsers.Add(new UserContactInfo()
        {
            UserId = 14,
            UserUuid = expectedUserUuid,
            Username = Username,
            CreatedAt = expectedCreatedAt,
            EmailAddress = "some@mail.no",
            PhoneNumber = "+4798765430",
            PhoneNumberLastChanged = expectedPhoneNumberLastChanged
        });
        await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await repository.GetByUsername(Username, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(14, result.UserId);
        Assert.Equal(expectedUserUuid, result.UserUuid);
        Assert.Equal(Username, result.Username);
        Assert.Equal(expectedCreatedAt, result.CreatedAt);
        Assert.Equal("some@mail.no", result.EmailAddress);
        Assert.Equal("+4798765430", result.PhoneNumber);
        Assert.Equal(expectedPhoneNumberLastChanged, result.PhoneNumberLastChanged);
    }

    [Fact]
    public async Task GetByEmail_WhenNoUsersWithEmailExist_ReturnsEmptyList()
    {
        // Arrange
        var options = CreateOptions(nameof(GetByEmail_WhenNoUsersWithEmailExist_ReturnsEmptyList));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory);

        // Act
        var result = await repository.GetByEmail("nonexistent@example.com", CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByEmail_WhenSingleUserWithEmailExists_ReturnsSingleUser()
    {
        // Arrange
        var options = CreateOptions(nameof(GetByEmail_WhenSingleUserWithEmailExists_ReturnsSingleUser));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory);

        const string Email = "user@example.com";
        var expectedUserUuid = Guid.NewGuid();
        var expectedCreatedAt = DateTime.UtcNow.AddMinutes(-2);
        var expectedPhoneNumberLastChanged = DateTime.UtcNow.AddMinutes(-1);

        await using var seedContext = new ProfileDbContext(options);
        seedContext.SelfIdentifiedUsers.Add(new UserContactInfo()
        {
            UserId = 15,
            UserUuid = expectedUserUuid,
            Username = "singleuser",
            CreatedAt = expectedCreatedAt,
            EmailAddress = Email,
            PhoneNumber = "+4798765430",
            PhoneNumberLastChanged = expectedPhoneNumberLastChanged
        });
        await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await repository.GetByEmail(Email, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(15, result[0].UserId);
        Assert.Equal(expectedUserUuid, result[0].UserUuid);
        Assert.Equal("singleuser", result[0].Username);
        Assert.Equal(expectedCreatedAt, result[0].CreatedAt);
        Assert.Equal(Email, result[0].EmailAddress);
        Assert.Equal("+4798765430", result[0].PhoneNumber);
        Assert.Equal(expectedPhoneNumberLastChanged, result[0].PhoneNumberLastChanged);
    }

    [Fact]
    public async Task GetByEmail_WhenMultipleUsersWithSameEmailExist_ReturnsAllMatches()
    {
        // Arrange
        var options = CreateOptions(nameof(GetByEmail_WhenMultipleUsersWithSameEmailExist_ReturnsAllMatches));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory);

        const string Email = "shared@example.com";
        var userUuid1 = Guid.NewGuid();
        var userUuid2 = Guid.NewGuid();
        var createdAt1 = DateTime.UtcNow.AddMinutes(-5);
        var createdAt2 = DateTime.UtcNow.AddMinutes(-3);

        await using var seedContext = new ProfileDbContext(options);
        seedContext.SelfIdentifiedUsers.Add(new UserContactInfo()
        {
            UserId = 16,
            UserUuid = userUuid1,
            Username = "user1",
            CreatedAt = createdAt1,
            EmailAddress = Email,
            PhoneNumber = "+4798765431",
            PhoneNumberLastChanged = createdAt1
        });
        seedContext.SelfIdentifiedUsers.Add(new UserContactInfo()
        {
            UserId = 17,
            UserUuid = userUuid2,
            Username = "user2",
            CreatedAt = createdAt2,
            EmailAddress = Email,
            PhoneNumber = "+4798765432",
            PhoneNumberLastChanged = createdAt2
        });
        await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await repository.GetByEmail(Email, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var user1 = result.FirstOrDefault(u => u.UserId == 16);
        Assert.Equal(16, user1.UserId);
        Assert.Equal("user1", user1.Username);
        Assert.Equal("+4798765431", user1.PhoneNumber);

        var user2 = result.FirstOrDefault(u => u.UserId == 17);
        Assert.Equal(17, user2.UserId);
        Assert.Equal("user2", user2.Username);
        Assert.Equal("+4798765432", user2.PhoneNumber);
    }

    [Fact]
    public async Task GetByEmail_IsCaseInsensitive()
    {
        // Arrange
        var options = CreateOptions(nameof(GetByEmail_IsCaseInsensitive));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory);

        const string EmailLowercase = "user@example.com";
        var expectedUserUuid = Guid.NewGuid();

        await using var seedContext = new ProfileDbContext(options);
        seedContext.SelfIdentifiedUsers.Add(new UserContactInfo()
        {
            UserId = 18,
            UserUuid = expectedUserUuid,
            Username = "caseinsensitiveuser",
            CreatedAt = DateTime.UtcNow,
            EmailAddress = EmailLowercase,
            PhoneNumber = null,
            PhoneNumberLastChanged = null
        });
        await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act - Query with different cases
        var resultLowercase = await repository.GetByEmail("user@example.com", CancellationToken.None);
        var resultUppercase = await repository.GetByEmail("USER@EXAMPLE.COM", CancellationToken.None);
        var resultMixedCase = await repository.GetByEmail("User@Example.Com", CancellationToken.None);

        // Assert
        Assert.Single(resultLowercase);
        Assert.Equal(18, resultLowercase[0].UserId);

        Assert.Single(resultUppercase);
        Assert.Equal(18, resultUppercase[0].UserId);

        Assert.Single(resultMixedCase);
        Assert.Equal(18, resultMixedCase[0].UserId);
    }

    [Fact]
    public async Task GetByEmail_WhenUserHasNullPhoneNumber_ReturnsUserWithNullPhoneNumber()
    {
        // Arrange
        var options = CreateOptions(nameof(GetByEmail_WhenUserHasNullPhoneNumber_ReturnsUserWithNullPhoneNumber));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory);

        const string Email = "nophone@example.com";
        var expectedUserUuid = Guid.NewGuid();
        var expectedCreatedAt = DateTime.UtcNow.AddMinutes(-2);

        await using var seedContext = new ProfileDbContext(options);
        seedContext.SelfIdentifiedUsers.Add(new UserContactInfo()
        {
            UserId = 19,
            UserUuid = expectedUserUuid,
            Username = "nophoneuser",
            CreatedAt = expectedCreatedAt,
            EmailAddress = Email,
            PhoneNumber = null,
            PhoneNumberLastChanged = null
        });
        await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await repository.GetByEmail(Email, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal(19, result[0].UserId);
        Assert.Equal("nophoneuser", result[0].Username);
        Assert.Null(result[0].PhoneNumber);
        Assert.Null(result[0].PhoneNumberLastChanged);
    }
}
