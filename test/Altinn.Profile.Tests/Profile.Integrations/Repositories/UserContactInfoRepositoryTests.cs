using System;
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
    public async Task UpdatePhoneNumber_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var options = CreateOptions(nameof(UpdatePhoneNumber_WhenUserDoesNotExist_ReturnsNull));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory);

        // Act
        var result = await repository.UpdatePhoneNumber(4, "+4798765431", CancellationToken.None);

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

        int testUserId = 5;
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

        int testUserId = 6;

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
}
