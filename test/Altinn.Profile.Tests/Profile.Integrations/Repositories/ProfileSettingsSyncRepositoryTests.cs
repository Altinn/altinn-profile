using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Core.Telemetry;
using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Integrations.Persistence;
using Altinn.Profile.Integrations.Repositories.A2Sync;
using Microsoft.EntityFrameworkCore;
using Moq;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.Repositories;

public class ProfileSettingsSyncRepositoryTests : IDisposable
{
    private bool _isDisposed;
    private readonly ProfileDbContext _databaseContext;
    private readonly Mock<IDbContextFactory<ProfileDbContext>> _databaseContextFactory;

    public ProfileSettingsSyncRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ProfileDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _databaseContext = new ProfileDbContext(options);

        _databaseContextFactory = new Mock<IDbContextFactory<ProfileDbContext>>();
        _databaseContextFactory.Setup(f => f.CreateDbContext())
            .Returns(_databaseContext);

        _databaseContextFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_databaseContext);

        _databaseContext = _databaseContextFactory.Object.CreateDbContext();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                _databaseContext.Database.EnsureDeleted();
                _databaseContext.Dispose();
            }

            _isDisposed = true;
        }
    }

    [Fact]
    public async Task UpdateProfileSettings_AddsNewProfileSettings_EmitsAddedMetric()
    {
        // Arrange
        var metricItems = new List<Metric>();
        using var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter(Telemetry.AppName)
            .AddInMemoryExporter(metricItems)
            .Build();

        using var telemetry = new Telemetry();
        var repository = new ProfileSettingsSyncRepository(_databaseContextFactory.Object, telemetry);

        var profileSettings = new ProfileSettings
        {
            UserId = 100,
            DoNotPromptForParty = true,
            PreselectedPartyUuid = Guid.NewGuid(),
            ShowClientUnits = true,
            ShouldShowSubEntities = false,
            ShouldShowDeletedEntities = false,
            IgnoreUnitProfileDateTime = DateTime.Now
        };

        // Act
        await repository.UpdateProfileSettings(profileSettings);

        meterProvider.ForceFlush();

        // Assert
        var addedMetric = metricItems.Single(item => item.Name == Telemetry.Metrics.CreateName("profilesettings.added"));
        Assert.NotNull(addedMetric);

        long addedSum = 0;
        foreach (ref readonly var p in addedMetric.GetMetricPoints())
        {
            addedSum += p.GetSumLong();
        }

        Assert.Equal(1, addedSum);
    }

    [Fact]
    public async Task UpdateProfileSettings_UpdatesExistingProfileSettings_EmitsUpdatedMetric()
    {
        // Arrange
        var metricItems = new List<Metric>();
        using var meterProvider = Sdk.CreateMeterProviderBuilder()
            .AddMeter(Telemetry.AppName)
            .AddInMemoryExporter(metricItems)
            .Build();

        using var telemetry = new Telemetry();
        var repository = new ProfileSettingsSyncRepository(_databaseContextFactory.Object, telemetry);

        var userId = 200;
        var existing = new ProfileSettings
        {
            UserId = userId,
            DoNotPromptForParty = false,
            PreselectedPartyUuid = Guid.NewGuid(),
            ShowClientUnits = false,
            ShouldShowSubEntities = false,
            ShouldShowDeletedEntities = false,
            IgnoreUnitProfileDateTime = null
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
            IgnoreUnitProfileDateTime = DateTime.Now
        };

        // Act
        await repository.UpdateProfileSettings(updated);

        meterProvider.ForceFlush();

        // Assert
        var profileSettingsUpdated = metricItems.Single(item => item.Name == Telemetry.Metrics.CreateName("profilesettings.updated"));
        long updatedSum = 0;
        foreach (ref readonly var p in profileSettingsUpdated.GetMetricPoints())
        {
            updatedSum += p.GetSumLong();
        }

        Assert.Equal(1, updatedSum);
    }
}
