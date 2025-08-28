using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Profile.Changelog;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.ProfessionalNotificationAddresses;
using Altinn.Profile.Core.Telemetry;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Integrations.SblBridge.Changelog;

using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace Altinn.Profile.Tests.Changelog;

public class NotificationSettingsImportJobTests
{
    [Fact]
    public async Task RunAsync_ProcessesChangeLogFromClient()
    {
        // Arrange
        var logger = Mock.Of<ILogger<NotificationSettingImportJob>>();
        var timeProvider = TimeProvider.System;

        var changeLogClient = new Mock<IChangeLogClient>();
        var changelogSyncMetadataRepository = new Mock<IChangelogSyncMetadataRepository>();
        var notificationSettingSyncRepository = new Mock<IProfessionalNotificationsRepository>();

        var testChangeDate = DateTime.UtcNow.AddDays(-1);

        // Setup metadata repo to return a last sync date
        changelogSyncMetadataRepository
            .Setup(r => r.GetLatestSyncTimestampAsync(DataType.ReporteeNotificationSettings, It.IsAny<CancellationToken>()))
            .ReturnsAsync(testChangeDate);

        // Setup a fake notification setting and changelog item
        var expectedUserId = 42;
        var expectedPartyUuid = Guid.Parse("00000000-0000-0000-0000-000000000042");
        var expectedEmail = "test@example.com";
        var expectedPhone = "12345678";
        var notificationSettingJson = $"{{\"userId\": {expectedUserId}, \"partyUuid\": \"{expectedPartyUuid}\", \"phoneNumber\": \"{expectedPhone}\", \"email\": \"{expectedEmail}\", \"serviceOptions\": [\"\"]}}";
        var changeLogItem = new ChangeLogItem
        {
            ProfileChangeLogId = 1,
            ChangeDatetime = DateTime.UtcNow,
            OperationType = OperationType.Insert,
            DataType = DataType.ReporteeNotificationSettings,
            DataObject = notificationSettingJson,
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
            .Setup(c => c.GetChangeLog(It.IsAny<DateTime>(), DataType.ReporteeNotificationSettings, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount == 1 ? changeLog : new ChangeLog { ProfileChangeLogList = [] };
            });

        // Setup notification repo to expect an add or update
        notificationSettingSyncRepository
            .Setup(r => r.AddOrUpdateNotificationAddressAsync(
                It.Is<UserPartyContactInfo>(u =>
                u.UserId == expectedUserId &&
                u.PartyUuid == expectedPartyUuid &&
                u.EmailAddress == expectedEmail &&
                u.PhoneNumber == expectedPhone &&
                u.UserPartyContactInfoResources != null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var job = new TestableNotificationSettingImportJob(
            logger,
            changeLogClient.Object,
            timeProvider,
            changelogSyncMetadataRepository.Object,
            notificationSettingSyncRepository.Object,
            null);

        // Act
        await job.InvokeRunAsync(CancellationToken.None);

        // Assert
        notificationSettingSyncRepository.Verify(
            r => r.AddOrUpdateNotificationAddressAsync(
                It.Is<UserPartyContactInfo>(u =>
                u.UserId == expectedUserId &&
                u.PartyUuid == expectedPartyUuid &&
                u.EmailAddress == expectedEmail &&
                u.PhoneNumber == expectedPhone &&
                u.UserPartyContactInfoResources != null), 
                It.IsAny<CancellationToken>()),
            Times.Once);

        changelogSyncMetadataRepository.Verify(
            r => r.UpdateLatestChangeTimestampAsync(
            It.IsAny<DateTime>(),
            DataType.ReporteeNotificationSettings),
            Times.AtLeastOnce);

        changeLogClient.Verify(
            c => c.GetChangeLog(It.IsAny<DateTime>(), DataType.ReporteeNotificationSettings, It.IsAny<CancellationToken>()),
            Times.AtLeastOnce);
    }

    private class TestableNotificationSettingImportJob : NotificationSettingImportJob
    {
        public TestableNotificationSettingImportJob(
            ILogger<NotificationSettingImportJob> logger,
            IChangeLogClient changeLogClient,
            TimeProvider timeProvider,
            IChangelogSyncMetadataRepository changelogSyncMetadataRepository,
            IProfessionalNotificationsRepository notificationSettingSyncRepository,
            Telemetry telemetry = null)
            : base(logger, changeLogClient, timeProvider, changelogSyncMetadataRepository, notificationSettingSyncRepository, telemetry)
        {
        }

        // Expose the protected RunAsync as public for testing
        public async Task InvokeRunAsync(CancellationToken cancellationToken)
        {
            await RunAsync(cancellationToken);
        }
    }
}
