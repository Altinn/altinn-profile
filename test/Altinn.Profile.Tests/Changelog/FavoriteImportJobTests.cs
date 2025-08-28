using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Changelog;
using Altinn.Profile.Core.Telemetry;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Integrations.SblBridge.Changelog;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Altinn.Profile.Tests.Changelog;

public class FavoriteImportJobTests
{
    [Fact]
    public async Task RunAsync_ProcessesChangeLogFromClient()
    {
        // Arrange
        var logger = Mock.Of<ILogger<FavoriteImportJob>>();
        var timeProvider = TimeProvider.System;

        var changeLogClient = new Mock<IChangeLogClient>();
        var changelogSyncMetadataRepository = new Mock<IChangelogSyncMetadataRepository>();
        var favoriteSyncRepository = new Mock<IFavoriteSyncRepository>();

        var testChangeDate = DateTime.UtcNow.AddDays(-1);

        // Setup metadata repo to return a last sync date
        changelogSyncMetadataRepository
            .Setup(r => r.GetLatestSyncTimestampAsync(DataType.Favorites, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testChangeDate);

        // Setup a fake favorite and changelog item
        var expectedUserId = 1;
        var expectedPartyUuid = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var favoriteJson = $"{{\"userId\":{expectedUserId},\"partyUuid\":\"{expectedPartyUuid}\"}}";
        var changeLogItem = new ChangeLogItem
        {
            ProfileChangeLogId = 1,
            ChangeDatetime = DateTime.UtcNow,
            OperationType = OperationType.Insert,
            DataType = DataType.Favorites,
            DataObject = favoriteJson,
            ChangeSource = 2,
            LoggedDateTime = DateTime.UtcNow
        };

        var changeLog = new ChangeLog
        {
            ProfileChangeLogList = new List<ChangeLogItem> { changeLogItem }
        };

        // Setup the client to return the changelog once, then an empty list
        var callCount = 0;
        changeLogClient
            .Setup(c => c.GetChangeLog(It.IsAny<DateTime>(), DataType.Favorites, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount == 1 ? changeLog : new ChangeLog { ProfileChangeLogList = new List<ChangeLogItem>() };
            });

        // Setup favorite repo to expect an add
        favoriteSyncRepository
            .Setup(r => r.AddPartyToFavorites(expectedUserId, expectedPartyUuid, changeLogItem.ChangeDatetime, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var job = new TestableFavoriteImportJob(
            logger,
            changeLogClient.Object,
            timeProvider,
            changelogSyncMetadataRepository.Object,
            favoriteSyncRepository.Object,
            null);

        // Act
        await job.InvokeRunAsync(CancellationToken.None);

        // Assert
        favoriteSyncRepository.Verify(
            r =>
            r.AddPartyToFavorites(expectedUserId, expectedPartyUuid, changeLogItem.ChangeDatetime, It.IsAny<CancellationToken>()),
            Times.Once);

        changeLogClient.Verify(
            c =>
            c.GetChangeLog(It.IsAny<DateTime>(), DataType.Favorites, It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    private class TestableFavoriteImportJob : FavoriteImportJob
    {
        public TestableFavoriteImportJob(
            ILogger<FavoriteImportJob> logger,
            IChangeLogClient changeLogClient,
            TimeProvider timeProvider,
            IChangelogSyncMetadataRepository changelogSyncMetadataRepository,
            IFavoriteSyncRepository favoriteSyncRepository,
            Telemetry telemetry = null)
            : base(logger, changeLogClient, timeProvider, changelogSyncMetadataRepository, favoriteSyncRepository, telemetry)
        {
        }

        // Expose the protected RunAsync as public for testing
        public async Task InvokeRunAsync(CancellationToken cancellationToken)
        {
            await RunAsync(cancellationToken);
        }
    }
}
