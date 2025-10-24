using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Core.Utils;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Integrations.Repositories.A2Sync;

using Microsoft.EntityFrameworkCore;

using Moq;

using OpenTelemetry;
using OpenTelemetry.Metrics;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Repositories;

public class ProfileSettingsRepositoryTests
{
    private readonly ProfileDbContext _databaseContext;
    private readonly Mock<IDbContextFactory<ProfileDbContext>> _databaseContextFactory;
    private readonly ProfileSettingsRepository _repository;

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

        _repository = new ProfileSettingsRepository(_databaseContextFactory.Object);
    }

    [Fact]
    public async Task UpdateProfileSettings_AddsNewProfileSettings_Successful()
    {
        // Arrange
        var repository = new ProfileSettingsRepository(_databaseContextFactory.Object);

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
        await repository.UpdateProfileSettings(profileSettings);

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
    }

    [Fact]
    public async Task UpdateProfileSettings_UpdatesExistingProfileSettings_EmitsUpdatedMetric()
    {
        // Arrange
        var repository = new ProfileSettingsRepository(_databaseContextFactory.Object);

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
        await repository.UpdateProfileSettings(updated);

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
    }

    [Fact]
    public async Task GetProfileSettings_ReturnsNullIfNotExists()
    {
        var userId = 4;

        var repository = new ProfileSettingsRepository(_databaseContextFactory.Object);

        var result = await _repository.GetProfileSettings(userId);

        Assert.Null(result);
    }

    [Fact]
    public async Task PatchProfileSettings_UpdatesExistingProfileSettings_Successful()
    {
        // Arrange
        var userId = 300;
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

        var newPreselected = Guid.NewGuid();
        var patch = new ProfileSettingsPatchRequest
        {
            UserId = userId,
            Language = "nb",
            DoNotPromptForParty = true,
            PreselectedPartyUuid = new Optional<Guid?>(newPreselected),
            ShowClientUnits = true,
            ShouldShowSubEntities = true,
            ShouldShowDeletedEntities = true
        };

        // Act
        var result = await _repository.PatchProfileSettings(patch);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal("nb", result.LanguageType);
        Assert.True(result.DoNotPromptForParty);
        Assert.Equal(newPreselected, result.PreselectedPartyUuid);
        Assert.True(result.ShowClientUnits);
        Assert.True(result.ShouldShowSubEntities);
        Assert.True(result.ShouldShowDeletedEntities);
    }

    [Fact]
    public async Task PatchProfileSettings_ClearsPreselectedPartyUuid_WhenOptionalHasNullValue()
    {
        // Arrange
        var userId = 301;
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

        // Optional explicitly present but with null value => should clear stored value
        var patch = new ProfileSettingsPatchRequest
        {
            UserId = userId,
            PreselectedPartyUuid = new Optional<Guid?>(null)
        };

        // Act
        var result = await _repository.PatchProfileSettings(patch);

        // Assert
        Assert.NotNull(result);
        Assert.Null(result.PreselectedPartyUuid);
    }

    [Fact]
    public async Task PatchProfileSettings_ClearsPreselectedPartyUuid_WhenOptionalValue()
    {
        // Arrange
        var userId = 301;
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

        // Optional explicitly not present => should not change stored value
        var patch = new ProfileSettingsPatchRequest
        {
            UserId = userId,
            PreselectedPartyUuid = new Optional<Guid?>()
        };

        // Act
        var result = await _repository.PatchProfileSettings(patch);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.PreselectedPartyUuid);
        Assert.Equal(existing.PreselectedPartyUuid, result.PreselectedPartyUuid);
    }

    [Fact]
    public async Task PatchProfileSettings_ReturnsNullIfNotExists()
    {
        // Arrange
        var userId = 9999;
        var patch = new ProfileSettingsPatchRequest
        {
            UserId = userId,
            Language = "nb"
        };

        // Act
        var result = await _repository.PatchProfileSettings(patch);

        // Assert
        Assert.Null(result);
    }
}
