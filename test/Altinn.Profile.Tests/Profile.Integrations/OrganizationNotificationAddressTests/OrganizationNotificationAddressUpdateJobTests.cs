using System;
using System.Threading.Tasks;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Tests.Testdata;
using Microsoft.Extensions.Logging;
using Moq;

using Xunit;

namespace Altinn.Profile.Tests.Profile.Integrations.OrganizationNotificationAddressTests.OrganizationNotificationAddressTests;

public class OrganizationNotificationAddressUpdateJobTests()
{
    private readonly OrganizationNotificationAddressSettings _settings = new() { ChangesLogEndpoint = "https://example.com/changes", ChangesLogPageSize = 10000 };
    private readonly Mock<IRegistrySyncMetadataRepository> _metadataRepository = new();
    private readonly Mock<IOrganizationNotificationAddressUpdater> _organizationNotificationAddressUpdater = new();
    private readonly Mock<IOrganizationNotificationAddressHttpClient> _httpClient = new();
    private readonly Mock<ILogger<OrganizationNotificationAddressUpdateJob>> _logger = new();

    [Fact]
    public async Task SyncNotificationAddressesAsync_IfNoEntries_DoNothing()
    {
        // Arrange
        _metadataRepository.SetupSequence(m => m.GetLatestSyncTimestampAsync())
    .ReturnsAsync(DateTime.Now.AddDays(-1));

        _httpClient.SetupSequence(h => h.GetAddressChangesAsync(It.IsAny<string>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_0_faulty"));

        OrganizationNotificationAddressUpdateJob target =
            new(_settings, _httpClient.Object, _metadataRepository.Object, _organizationNotificationAddressUpdater.Object, _logger.Object);

        // Act
        await target.SyncNotificationAddressesAsync();

        // Assert
        _metadataRepository.VerifyAll();
        _httpClient.VerifyAll();
        _organizationNotificationAddressUpdater.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SyncNotificationAddressesAsync_IfNoChanges_DoNothing()
    {
        // Arrange
        _metadataRepository.SetupSequence(m => m.GetLatestSyncTimestampAsync())
    .ReturnsAsync(DateTime.Now.AddDays(-1));

        _httpClient.SetupSequence(h => h.GetAddressChangesAsync(It.IsAny<string>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_0"));

        OrganizationNotificationAddressUpdateJob target =
            new(_settings, _httpClient.Object, _metadataRepository.Object, _organizationNotificationAddressUpdater.Object, _logger.Object);

        // Act
        await target.SyncNotificationAddressesAsync();

        // Assert
        _metadataRepository.VerifyAll();
        _httpClient.VerifyAll();
        _organizationNotificationAddressUpdater.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task SyncNotificationAddressesAsyncTest_Success()
    {
        // Arrange
        _metadataRepository.SetupSequence(m => m.GetLatestSyncTimestampAsync())
    .ReturnsAsync(DateTime.Now.AddDays(-1));

        _httpClient.SetupSequence(h => h.GetAddressChangesAsync(It.IsAny<string>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_1"))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_2"));

        _organizationNotificationAddressUpdater.SetupSequence(p => p.SyncNotificationAddressesAsync(It.IsAny<NotificationAddressChangesLog>()))
            .ReturnsAsync(2)
            .ReturnsAsync(4);

        _metadataRepository.Setup(m => m.UpdateLatestChangeTimestampAsync(It.IsAny<DateTime>()));

        OrganizationNotificationAddressUpdateJob target =
            new(_settings, _httpClient.Object, _metadataRepository.Object, _organizationNotificationAddressUpdater.Object, _logger.Object);

        // Act
        await target.SyncNotificationAddressesAsync();

        // Assert
        _metadataRepository.VerifyAll();
        _httpClient.VerifyAll();
        _organizationNotificationAddressUpdater.VerifyAll();
    }

    [Fact]
    public async Task SyncNotificationAddressesAsyncTest_WhenNoLatestSyncDate_GetsWithoutSinceParam()
    {
        // Arrange
        _metadataRepository.SetupSequence(m => m.GetLatestSyncTimestampAsync())
    .ReturnsAsync((DateTime?)null);

        _httpClient.Setup(h => h.GetAddressChangesAsync(It.Is<string>(s => !s.Contains("since"))))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_2"));

        _organizationNotificationAddressUpdater.SetupSequence(p => p.SyncNotificationAddressesAsync(It.IsAny<NotificationAddressChangesLog>()))
            .ReturnsAsync(4);

        _metadataRepository.Setup(m => m.UpdateLatestChangeTimestampAsync(It.IsAny<DateTime>()));

        OrganizationNotificationAddressUpdateJob target =
            new(_settings, _httpClient.Object, _metadataRepository.Object, _organizationNotificationAddressUpdater.Object, _logger.Object);

        // Act
        await target.SyncNotificationAddressesAsync();

        // Assert
        _metadataRepository.VerifyAll();
        _httpClient.VerifyAll();
        _organizationNotificationAddressUpdater.VerifyAll();
    }

    [Fact]
    public async Task SyncNotificationAddressesAsyncTest_WhenNullableValues_Success()
    {
        // Arrange
        _metadataRepository.Setup(m => m.GetLatestSyncTimestampAsync())
    .ReturnsAsync(DateTime.Now.AddDays(-1));

        _httpClient.Setup(h => h.GetAddressChangesAsync(It.IsAny<string>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_3"));

        _organizationNotificationAddressUpdater.Setup(p => p.SyncNotificationAddressesAsync(It.IsAny<NotificationAddressChangesLog>()))
            .ReturnsAsync(2);

        OrganizationNotificationAddressUpdateJob target =
            new(_settings, _httpClient.Object, _metadataRepository.Object, _organizationNotificationAddressUpdater.Object, _logger.Object);

        // Act
        await target.SyncNotificationAddressesAsync();

        // Assert
        _metadataRepository.VerifyAll();
        _httpClient.VerifyAll();
        _organizationNotificationAddressUpdater.VerifyAll();
    }

    [Fact]
    public async Task SyncNotificationAddressesAsync_WhenFailingToUpdateAddresses_DoNotUpdateSyncTime()
    {
        // Arrange
        _metadataRepository.SetupSequence(m => m.GetLatestSyncTimestampAsync())
    .ReturnsAsync(DateTime.Now.AddDays(-1));

        _httpClient.SetupSequence(h => h.GetAddressChangesAsync(It.IsAny<string>()))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_1"))
            .ReturnsAsync(await TestDataLoader.Load<NotificationAddressChangesLog>("changes_2"));

        _organizationNotificationAddressUpdater.SetupSequence(p => p.SyncNotificationAddressesAsync(It.IsAny<NotificationAddressChangesLog>()))
            .ReturnsAsync(0)
            .ReturnsAsync(0);

        OrganizationNotificationAddressUpdateJob target =
            new(_settings, _httpClient.Object, _metadataRepository.Object, _organizationNotificationAddressUpdater.Object, _logger.Object);

        // Act
        await target.SyncNotificationAddressesAsync();

        // Assert
        _httpClient.VerifyAll();
        _organizationNotificationAddressUpdater.VerifyAll();

        // Verify that metadataRepository.UpdateLatestChangeTimestampAsync() is not called
        _metadataRepository.VerifyAll();
        _metadataRepository.VerifyNoOtherCalls();
    }
}
