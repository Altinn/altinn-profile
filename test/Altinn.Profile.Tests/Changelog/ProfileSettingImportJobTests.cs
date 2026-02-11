using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Changelog;
using Altinn.Profile.Core.Telemetry;
using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Integrations.Repositories.A2Sync;
using Altinn.Profile.Integrations.SblBridge.Changelog;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Altinn.Profile.Tests.Changelog
{
    public class ProfileSettingImportJobTests
    {
        [Fact]
        public async Task RunAsync_ProcessesChangeLogFromClient()
        {
            // Arrange
            var logger = Mock.Of<ILogger<ProfileSettingImportJob>>();
            var timeProvider = TimeProvider.System;

            var changeLogClient = new Mock<IChangeLogClient>();
            var changelogSyncMetadataRepository = new Mock<IChangelogSyncMetadataRepository>();

            // ProfileSettingsSyncRepository is a concrete dependency in the job.
            // Create a mock and setup UpdateProfileSettings to complete successfully.
            var profileSettingsSyncRepository = new Mock<IProfileSettingsSyncRepository>();
            profileSettingsSyncRepository
                .Setup(r => r.UpdateProfileSettings(It.IsAny<ProfileSettings>()))
                .Returns(Task.CompletedTask);

            var testChangeDate = DateTime.UtcNow.AddDays(-1);

            // Setup metadata repo to return a last sync date
            changelogSyncMetadataRepository
                .Setup(r => r.GetLatestSyncTimestampAsync(DataType.PortalSettingPreferences, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testChangeDate);

            // Create a portal settings JSON that PortalSettings.Deserialize can consume.
            // Field names follow the camelCase convention used elsewhere in changelog items.
            var expectedUserId = 123;
            var expectedLanguage = 1044; // Altinn2 language code example
            var expectedPreselectedParty = Guid.Parse("00000000-0000-0000-0000-000000000123");
            var portalSettingsJson = $@"{{
                ""userId"": {expectedUserId},
                ""languageType"": {expectedLanguage},
                ""doNotPromptForParty"": true,
                ""preselectedPartyUuid"": ""{expectedPreselectedParty}"",
                ""showClientUnits"": true,
                ""shouldShowSubEntities"": false,
                ""shouldShowDeletedEntities"": false,
                ""ignoreUnitProfileDateTime"": null
            }}";

            var changeLogItem = new ChangeLogItem
            {
                ProfileChangeLogId = 1,
                ChangeDatetime = DateTime.UtcNow,
                OperationType = OperationType.Insert,
                DataObject = portalSettingsJson,
                DataType = DataType.PortalSettingPreferences
            };

            var changeLog = new ChangeLog
            {
                ProfileChangeLogList = new List<ChangeLogItem> { changeLogItem }
            };

            // Setup the client to return the changelog once, then an empty page to end the loop
            changeLogClient
                .SetupSequence(c => c.GetChangeLog(It.IsAny<DateTime>(), DataType.PortalSettingPreferences, It.IsAny<CancellationToken>()))
                .ReturnsAsync(changeLog)
                .ReturnsAsync(new ChangeLog { ProfileChangeLogList = new List<ChangeLogItem>() });
       
            var job = new TestableProfileSettingImportJob(
                logger,
                changeLogClient.Object,
                timeProvider,
                changelogSyncMetadataRepository.Object,
                profileSettingsSyncRepository.Object,
                null);

            // Act
            await job.InvokeRunAsync(TestContext.Current.CancellationToken);

            // Assert
            profileSettingsSyncRepository.Verify(
                r => r.UpdateProfileSettings(It.Is<ProfileSettings>(p =>
                    p.UserId == expectedUserId &&
                    !string.IsNullOrEmpty(p.LanguageType))), // exact mapping handled by LanguageType.GetFromAltinn2Code
                Times.Once);

            changelogSyncMetadataRepository.Verify(
                r => r.UpdateLatestChangeTimestampAsync(
                    It.IsAny<DateTime>(),
                    DataType.PortalSettingPreferences),
                Times.AtLeastOnce);

            changeLogClient.Verify(
                c => c.GetChangeLog(It.IsAny<DateTime>(), DataType.PortalSettingPreferences, It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task RunAsync_LogsWarningOnDeserializationFailure()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ProfileSettingImportJob>>();
            var timeProvider = TimeProvider.System;

            var changeLogClient = new Mock<IChangeLogClient>();
            var changelogSyncMetadataRepository = new Mock<IChangelogSyncMetadataRepository>();

            var profileSettingsSyncRepository = new Mock<IProfileSettingsSyncRepository>();

            // Setup metadata repo to return null so we fetch from DateTime.MinValue
            changelogSyncMetadataRepository
                .Setup(r => r.GetLatestSyncTimestampAsync(DataType.PortalSettingPreferences, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DateTime?)null);

            // Provide an invalid JSON so deserialization returns null
            var changeLogItem = new ChangeLogItem
            {
                ProfileChangeLogId = 1,
                ChangeDatetime = DateTime.UtcNow,
                OperationType = OperationType.Insert,
                DataObject = "this-is-not-json",
                DataType = DataType.PortalSettingPreferences
            };

            var changeLog = new ChangeLog
            {
                ProfileChangeLogList = new List<ChangeLogItem> { changeLogItem }
            };

            var callCount = 0;
            changeLogClient
                .Setup(c => c.GetChangeLog(It.IsAny<DateTime>(), DataType.PortalSettingPreferences, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    return callCount == 1 ? changeLog : new ChangeLog { ProfileChangeLogList = new List<ChangeLogItem>() };
                });

            var job = new TestableProfileSettingImportJob(
                loggerMock.Object,
                changeLogClient.Object,
                timeProvider,
                changelogSyncMetadataRepository.Object,
                profileSettingsSyncRepository.Object,
                null);

            // Act
            await job.InvokeRunAsync(TestContext.Current.CancellationToken);

            // Assert - an error should be logged when deserialization fails
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to deserialize ProfileSetting change log item")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Ensure repository was not called when deserialization fails
            profileSettingsSyncRepository.Verify(
                r => r.UpdateProfileSettings(It.IsAny<ProfileSettings>()),
                Times.Never);
        }

        private class TestableProfileSettingImportJob : ProfileSettingImportJob
        {
            public TestableProfileSettingImportJob(
                ILogger<ProfileSettingImportJob> logger,
                IChangeLogClient changeLogClient,
                TimeProvider timeProvider,
                IChangelogSyncMetadataRepository changelogSyncMetadataRepository,
                IProfileSettingsSyncRepository profileSettingsSyncRepository,
                Telemetry telemetry = null)
                : base(logger, changeLogClient, timeProvider, changelogSyncMetadataRepository, profileSettingsSyncRepository, telemetry)
            {
            }

            // Expose the protected RunAsync as public for testing
            public async Task InvokeRunAsync(CancellationToken cancellationToken)
            {
                await RunAsync(cancellationToken);
            }
        }
    }
}
