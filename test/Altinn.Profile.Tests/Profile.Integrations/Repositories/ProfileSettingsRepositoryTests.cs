using System;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Integrations.Events;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories;

using Microsoft.EntityFrameworkCore;

using Moq;

using Wolverine;
using Wolverine.EntityFrameworkCore;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Repositories;

public class ProfileSettingsRepositoryTests
{
    private readonly ProfileDbContext _databaseContext;
    private readonly Mock<IDbContextFactory<ProfileDbContext>> _databaseContextFactory;
    private readonly ProfileSettingsRepository _repository;
    private readonly Mock<IDbContextOutbox> _dbContextOutboxMock = new();

    public ProfileSettingsRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ProfileDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _databaseContextFactory = new Mock<IDbContextFactory<ProfileDbContext>>();
        _databaseContextFactory.Setup(f => f.CreateDbContext())
            .Returns(() => new ProfileDbContext(options));

        _databaseContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new ProfileDbContext(options));

        _databaseContext = _databaseContextFactory.Object.CreateDbContext();

        _repository = new ProfileSettingsRepository(_databaseContextFactory.Object, _dbContextOutboxMock.Object);
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
            .Returns(async () =>
            {
                await context.SaveChangesAsync(CancellationToken.None);
            });

        _dbContextOutboxMock
            .Setup(mock => mock.PublishAsync(It.IsAny<TEvent>(), null))
            .Callback(callback);
    }

    [Fact]
    public async Task UpdateProfileSettings_AddsNewProfileSettings_Successful()
    {
        // Arrange
        ProfileSettingsUpdatedEvent actualEventRaised = null;
        void EventRaisingCallback(ProfileSettingsUpdatedEvent ev, DeliveryOptions opts) => actualEventRaised = ev;
        MockDbContextOutbox((Action<ProfileSettingsUpdatedEvent, DeliveryOptions>)EventRaisingCallback);

        var repository = new ProfileSettingsRepository(_databaseContextFactory.Object, _dbContextOutboxMock.Object);

        var profileSettings = new ProfileSettings
        {
            UserId = 100,
            DoNotPromptForParty = true,
            PreselectedPartyUuid = Guid.NewGuid(),
            ShowClientUnits = true,
            ShouldShowSubEntities = false,
            ShouldShowDeletedEntities = false,
            IgnoreUnitProfileDateTime = DateTime.Now,
            LanguageType = "en"
        };

        // Act
        await repository.UpdateProfileSettings(profileSettings, CancellationToken.None);

        var updated = await repository.GetProfileSettings(profileSettings.UserId);
        Assert.NotNull(updated);
        Assert.Equal(profileSettings.UserId, updated.UserId);
        Assert.Equal(profileSettings.DoNotPromptForParty, updated.DoNotPromptForParty);
        Assert.Equal(profileSettings.PreselectedPartyUuid, updated.PreselectedPartyUuid);
        Assert.Equal(profileSettings.ShowClientUnits, updated.ShowClientUnits);
        Assert.Equal(profileSettings.ShouldShowSubEntities, updated.ShouldShowSubEntities);
        Assert.Equal(profileSettings.ShouldShowDeletedEntities, updated.ShouldShowDeletedEntities);
        Assert.Equal(profileSettings.IgnoreUnitProfileDateTime, updated.IgnoreUnitProfileDateTime);
        Assert.Equal(profileSettings.LanguageType, updated.LanguageType);

        _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<ProfileSettingsUpdatedEvent>(), It.IsAny<DeliveryOptions>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProfileSettings_UpdatesExistingProfileSettings_EmitsUpdatedMetric()
    {
        // Arrange
        ProfileSettingsUpdatedEvent actualEventRaised = null;
        void EventRaisingCallback(ProfileSettingsUpdatedEvent ev, DeliveryOptions opts) => actualEventRaised = ev;
        MockDbContextOutbox((Action<ProfileSettingsUpdatedEvent, DeliveryOptions>)EventRaisingCallback);

        var repository = new ProfileSettingsRepository(_databaseContextFactory.Object, _dbContextOutboxMock.Object);

        var userId = 200;
        var existing = new ProfileSettings
        {
            UserId = userId,
            DoNotPromptForParty = false,
            PreselectedPartyUuid = Guid.NewGuid(),
            ShowClientUnits = false,
            ShouldShowSubEntities = false,
            ShouldShowDeletedEntities = false,
            IgnoreUnitProfileDateTime = null,
            LanguageType = "en"
        };
        _databaseContext.ProfileSettings.Add(existing);
        await _databaseContext.SaveChangesAsync();

        var updated = new ProfileSettings
        {
            UserId = userId,
            DoNotPromptForParty = true,
            PreselectedPartyUuid = Guid.NewGuid(),
            ShowClientUnits = true,
            ShouldShowSubEntities = true,
            ShouldShowDeletedEntities = true,
            IgnoreUnitProfileDateTime = DateTime.Now,
            LanguageType = "nb"
        };

        // Act
        await repository.UpdateProfileSettings(updated, CancellationToken.None);

        var stored = await repository.GetProfileSettings(existing.UserId);
        Assert.NotNull(stored);
        Assert.Equal(updated.UserId, stored.UserId);
        Assert.Equal(updated.DoNotPromptForParty, stored.DoNotPromptForParty);
        Assert.Equal(updated.PreselectedPartyUuid, stored.PreselectedPartyUuid);
        Assert.Equal(updated.ShowClientUnits, stored.ShowClientUnits);
        Assert.Equal(updated.ShouldShowSubEntities, stored.ShouldShowSubEntities);
        Assert.Equal(updated.ShouldShowDeletedEntities, stored.ShouldShowDeletedEntities);
        Assert.Equal(updated.IgnoreUnitProfileDateTime, stored.IgnoreUnitProfileDateTime);
        Assert.Equal(updated.LanguageType, stored.LanguageType);

        _dbContextOutboxMock.Verify(mock => mock.PublishAsync(It.IsAny<ProfileSettingsUpdatedEvent>(), It.IsAny<DeliveryOptions>()), Times.Once);
    }

    [Fact]
    public async Task GetProfileSettings_ReturnsNullIfNotExists()
    {
        var userId = 4;

        var result = await _repository.GetProfileSettings(userId);

        Assert.Null(result);
    }
}
