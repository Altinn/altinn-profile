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
    public async Task UpdateMobileNumber_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var options = CreateOptions(nameof(UpdateMobileNumber_WhenUserDoesNotExist_ReturnsNull));
        var factory = new TestDbContextFactory(options);
        var repository = new UserContactInfoRepository(factory);

        // Act
        var result = await repository.UpdateMobileNumber(4, "+4798765431");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateMobileNumber_WhenUserExists_UpdatesPhoneNumber()
    {
        // Arrange
        var options = CreateOptions(nameof(UpdateMobileNumber_WhenUserExists_UpdatesPhoneNumber));
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
                MobileNumber = existingNumber,
                MobileNumberRegistered = DateTime.Now.AddMinutes(-1)
            };
            seedContext.SelfIdentifiedUsers.Add(userContactInfo);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Act
        var result = await repository.UpdateMobileNumber(testUserId, newNumber);

        // Assert
        await using var assertContext = new ProfileDbContext(options);
        var updatedUserContactInfo = await assertContext.SelfIdentifiedUsers.FirstOrDefaultAsync(
            u => u.UserId == testUserId,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(existingNumber, newNumber);
    }

    [Fact]
    public async Task UpdateMobileNumber_WhenUserExists_UpdatesPhoneNumberRegisteredToNow()
    {
        // Arrange
        var options = CreateOptions(nameof(UpdateMobileNumber_WhenUserExists_UpdatesPhoneNumberRegisteredToNow));
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
                MobileNumber = "+4798765430",
                MobileNumberRegistered = DateTime.Now.AddMinutes(-1)
            };
            seedContext.SelfIdentifiedUsers.Add(userContactInfo);
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        // Act
        var result = await repository.UpdateMobileNumber(testUserId, "+4798765433");

        // Assert
        await using var assertContext = new ProfileDbContext(options);
        var updatedUserContactInfo = await assertContext.SelfIdentifiedUsers.FirstOrDefaultAsync(
            u => u.UserId == testUserId,
            cancellationToken: TestContext.Current.CancellationToken);
        TimeSpan tolerance = TimeSpan.FromMilliseconds(5);
        Assert.Equal(DateTime.Now, updatedUserContactInfo.MobileNumberRegistered, tolerance); // The new timestamp should be approx. equal to now
    }
}
