using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core.Telemetry;
using Altinn.Profile.Core.User.ContactInfo;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories.A2Sync;
using Altinn.Profile.Integrations.SblBridge.User.PrivateConsent;

using Microsoft.EntityFrameworkCore;

using OpenTelemetry;
using OpenTelemetry.Metrics;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Repositories;

public class SIUserContactInfoSyncRepositoryTests
{
    private class TestDbContextFactory(DbContextOptions<ProfileDbContext> options) : IDbContextFactory<ProfileDbContext>
    {
        private readonly DbContextOptions<ProfileDbContext> _options = options;

        public ProfileDbContext CreateDbContext() => new(_options);

        public Task<ProfileDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => Task.FromResult(new ProfileDbContext(_options));
    }

    private static DbContextOptions<ProfileDbContext> CreateOptions(string name)
        => new DbContextOptionsBuilder<ProfileDbContext>()
            .UseInMemoryDatabase(databaseName: name)
            .Options;

    private static SiUserContactSettings CreateContactSettings(int userId, Guid userUuid, string userName = "testuser", string email = "test@example.com", string phone = "+4712345678")
        => new()
        {
            UserId = userId,
            UserUuid = userUuid,
            UserName = userName,
            EmailAddress = email,
            PhoneNumber = phone
        };

    [Fact]
    public async Task InsertOrUpdate_WhenUserDoesNotExist_InsertsUserWithCorrectProperties()
    {
        // Arrange
        var options = CreateOptions(nameof(InsertOrUpdate_WhenUserDoesNotExist_InsertsUserWithCorrectProperties));
        var factory = new TestDbContextFactory(options);
        var repository = new SIUserContactInfoSyncRepository(factory, null);

        var userId = 1;
        var userUuid = Guid.NewGuid();
        var updatedDatetime = DateTime.UtcNow.AddMinutes(-5);
        var contactSettings = CreateContactSettings(userId, userUuid);

        // Act
        await repository.InsertOrUpdate(contactSettings, updatedDatetime, TestContext.Current.CancellationToken);

        // Assert
        await using var assertContext = new ProfileDbContext(options);
        var stored = await assertContext.SelfIdentifiedUsers.FirstOrDefaultAsync(
            u => u.UserId == userId,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(stored);
        Assert.Equal(userId, stored.UserId);
        Assert.Equal(userUuid, stored.UserUuid);
        Assert.Equal(contactSettings.UserName, stored.Username);
        Assert.Equal(contactSettings.EmailAddress, stored.EmailAddress);
        Assert.Equal(contactSettings.PhoneNumber, stored.PhoneNumber);
        Assert.Equal(updatedDatetime, stored.CreatedAt);
    }

    [Fact]
    public async Task InsertOrUpdate_WhenUserDoesNotExist_ReturnsInsertedUserWithCorrectProperties()
    {
        // Arrange
        var options = CreateOptions(nameof(InsertOrUpdate_WhenUserDoesNotExist_ReturnsInsertedUserWithCorrectProperties));
        var factory = new TestDbContextFactory(options);
        var repository = new SIUserContactInfoSyncRepository(factory, null);

        var userId = 2;
        var userUuid = Guid.NewGuid();
        var updatedDatetime = DateTime.UtcNow.AddMinutes(-5);
        var contactSettings = CreateContactSettings(userId, userUuid);

        // Act
        var result = await repository.InsertOrUpdate(contactSettings, updatedDatetime, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(userUuid, result.UserUuid);
        Assert.Equal(contactSettings.UserName, result.Username);
        Assert.Equal(contactSettings.EmailAddress, result.EmailAddress);
        Assert.Equal(contactSettings.PhoneNumber, result.PhoneNumber);
        Assert.Equal(updatedDatetime, result.CreatedAt);
    }

    [Fact]
    public async Task InsertOrUpdate_WhenUserDoesNotExist_WithPhoneNumber_SetsPhoneNumberLastChangedToUpdatedDatetime()
    {
        // Arrange
        var options = CreateOptions(nameof(InsertOrUpdate_WhenUserDoesNotExist_WithPhoneNumber_SetsPhoneNumberLastChangedToUpdatedDatetime));
        var factory = new TestDbContextFactory(options);
        var repository = new SIUserContactInfoSyncRepository(factory, null);

        var updatedDatetime = DateTime.UtcNow.AddMinutes(-5);
        var contactSettings = CreateContactSettings(3, Guid.NewGuid(), phone: "+4712345678");

        // Act
        var result = await repository.InsertOrUpdate(contactSettings, updatedDatetime, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result.PhoneNumberLastChanged);
        Assert.Equal(updatedDatetime, result.PhoneNumberLastChanged.Value);
    }

    [Fact]
    public async Task InsertOrUpdate_WhenUserDoesNotExist_WithNullPhoneNumber_SetsPhoneNumberLastChangedToNull()
    {
        // Arrange
        var options = CreateOptions(nameof(InsertOrUpdate_WhenUserDoesNotExist_WithNullPhoneNumber_SetsPhoneNumberLastChangedToNull));
        var factory = new TestDbContextFactory(options);
        var repository = new SIUserContactInfoSyncRepository(factory, null);

        var contactSettings = CreateContactSettings(4, Guid.NewGuid(), phone: null);

        // Act
        var result = await repository.InsertOrUpdate(contactSettings, DateTime.UtcNow, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result.PhoneNumber);
        Assert.Null(result.PhoneNumberLastChanged);
    }

    [Fact]
    public async Task InsertOrUpdate_WhenUserDoesNotExist_WithNullEmailAddress_FallsBackToEmptyString()
    {
        // Arrange
        var options = CreateOptions(nameof(InsertOrUpdate_WhenUserDoesNotExist_WithNullEmailAddress_FallsBackToEmptyString));
        var factory = new TestDbContextFactory(options);
        var repository = new SIUserContactInfoSyncRepository(factory, null);

        var contactSettings = CreateContactSettings(5, Guid.NewGuid(), email: null);

        // Act
        var result = await repository.InsertOrUpdate(contactSettings, DateTime.UtcNow, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(string.Empty, result.EmailAddress);
    }

    [Fact]
    public async Task InsertOrUpdate_WhenUserExists_UpdatesEmailAndPhoneNumber()
    {
        // Arrange
        var options = CreateOptions(nameof(InsertOrUpdate_WhenUserExists_UpdatesEmailAndPhoneNumber));
        var factory = new TestDbContextFactory(options);
        var repository = new SIUserContactInfoSyncRepository(factory, null);

        var userId = 6;
        var userUuid = Guid.NewGuid();

        await using (var seedContext = new ProfileDbContext(options))
        {
            seedContext.SelfIdentifiedUsers.Add(new UserContactInfo
            {
                UserId = userId,
                UserUuid = userUuid,
                Username = "original",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                EmailAddress = "old@example.com",
                PhoneNumber = "+4711111111",
                PhoneNumberLastChanged = DateTime.UtcNow.AddDays(-1)
            });
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var updatedDatetime = DateTime.UtcNow;
        var contactSettings = CreateContactSettings(userId, userUuid, email: "new@example.com", phone: "+4799999999");

        // Act
        await repository.InsertOrUpdate(contactSettings, updatedDatetime, TestContext.Current.CancellationToken);

        // Assert
        await using var assertContext = new ProfileDbContext(options);
        var stored = await assertContext.SelfIdentifiedUsers.FirstOrDefaultAsync(
            u => u.UserId == userId,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(stored);
        Assert.Equal("new@example.com", stored.EmailAddress);
        Assert.Equal("+4799999999", stored.PhoneNumber);
    }

    [Fact]
    public async Task InsertOrUpdate_WhenUserExists_ReturnsUpdatedUser()
    {
        // Arrange
        var options = CreateOptions(nameof(InsertOrUpdate_WhenUserExists_ReturnsUpdatedUser));
        var factory = new TestDbContextFactory(options);
        var repository = new SIUserContactInfoSyncRepository(factory, null);

        var userId = 7;
        var userUuid = Guid.NewGuid();

        await using (var seedContext = new ProfileDbContext(options))
        {
            seedContext.SelfIdentifiedUsers.Add(new UserContactInfo
            {
                UserId = userId,
                UserUuid = userUuid,
                Username = "original",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                EmailAddress = "old@example.com",
                PhoneNumber = null
            });
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var contactSettings = CreateContactSettings(userId, userUuid, email: "new@example.com", phone: "+4799999999");

        // Act
        var result = await repository.InsertOrUpdate(contactSettings, DateTime.UtcNow, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("new@example.com", result.EmailAddress);
        Assert.Equal("+4799999999", result.PhoneNumber);
    }

    [Fact]
    public async Task InsertOrUpdate_WhenUserExists_WithPhoneNumber_SetsPhoneNumberLastChangedToUpdatedDatetime()
    {
        // Arrange
        var options = CreateOptions(nameof(InsertOrUpdate_WhenUserExists_WithPhoneNumber_SetsPhoneNumberLastChangedToUpdatedDatetime));
        var factory = new TestDbContextFactory(options);
        var repository = new SIUserContactInfoSyncRepository(factory, null);

        var userId = 8;

        await using (var seedContext = new ProfileDbContext(options))
        {
            seedContext.SelfIdentifiedUsers.Add(new UserContactInfo
            {
                UserId = userId,
                UserUuid = Guid.NewGuid(),
                Username = "user",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                EmailAddress = "old@example.com",
                PhoneNumber = null
            });
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var updatedDatetime = DateTime.UtcNow;
        var contactSettings = CreateContactSettings(userId, Guid.NewGuid(), phone: "+4712345678");

        // Act
        var result = await repository.InsertOrUpdate(contactSettings, updatedDatetime, TestContext.Current.CancellationToken);

        // Assert
        Assert.NotNull(result.PhoneNumberLastChanged);
        Assert.Equal(updatedDatetime, result.PhoneNumberLastChanged.Value);
    }

    [Fact]
    public async Task InsertOrUpdate_WhenUserExists_WithNullPhoneNumber_SetsPhoneNumberLastChangedToNull()
    {
        // Arrange
        var options = CreateOptions(nameof(InsertOrUpdate_WhenUserExists_WithNullPhoneNumber_SetsPhoneNumberLastChangedToNull));
        var factory = new TestDbContextFactory(options);
        var repository = new SIUserContactInfoSyncRepository(factory, null);

        var userId = 9;

        await using (var seedContext = new ProfileDbContext(options))
        {
            seedContext.SelfIdentifiedUsers.Add(new UserContactInfo
            {
                UserId = userId,
                UserUuid = Guid.NewGuid(),
                Username = "user",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                EmailAddress = "old@example.com",
                PhoneNumber = "+4711111111",
                PhoneNumberLastChanged = DateTime.UtcNow.AddDays(-1)
            });
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var contactSettings = CreateContactSettings(userId, Guid.NewGuid(), phone: null);

        // Act
        var result = await repository.InsertOrUpdate(contactSettings, DateTime.UtcNow, TestContext.Current.CancellationToken);

        // Assert
        Assert.Null(result.PhoneNumber);
        Assert.Null(result.PhoneNumberLastChanged);
    }

    [Fact]
    public async Task InsertOrUpdate_WhenUserExists_EmitsUpdatedMetric()
    {
        // Arrange
        var options = CreateOptions(nameof(InsertOrUpdate_WhenUserExists_EmitsUpdatedMetric));
        var factory = new TestDbContextFactory(options);

        var metricItems = new List<Metric>();
        using var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter(Telemetry.AppName)
            .AddInMemoryExporter(metricItems)
            .Build();

        using var telemetry = new Telemetry();
        var repository = new SIUserContactInfoSyncRepository(factory, telemetry);

        var userId = 10;

        await using (var seedContext = new ProfileDbContext(options))
        {
            seedContext.SelfIdentifiedUsers.Add(new UserContactInfo
            {
                UserId = userId,
                UserUuid = Guid.NewGuid(),
                Username = "user",
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                EmailAddress = "old@example.com"
            });
            await seedContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        var contactSettings = CreateContactSettings(userId, Guid.NewGuid());

        // Act
        await repository.InsertOrUpdate(contactSettings, DateTime.UtcNow, TestContext.Current.CancellationToken);

        meterProvider.ForceFlush();

        // Assert
        var updatedMetric = metricItems.Find(item => item.Name == Telemetry.Metrics.CreateName("siusercontactsettings.updated"));
        Assert.NotNull(updatedMetric);

        long updatedSum = 0;
        foreach (ref readonly var point in updatedMetric.GetMetricPoints())
        {
            updatedSum += point.GetSumLong();
        }

        Assert.Equal(1, updatedSum);
    }
}
