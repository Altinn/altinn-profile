using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Profile.Changelog;
using Altinn.Profile.Core.AddressVerifications.Models;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.Telemetry;
using Altinn.Profile.Core.User.ContactInfo;
using Altinn.Profile.Integrations.Repositories.A2Sync;
using Altinn.Profile.Integrations.SblBridge.Changelog;
using Altinn.Profile.Integrations.SblBridge.User.PrivateConsent;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Altinn.Profile.Tests.Changelog
{
    public class SIUserAddressImportJobTests
    {
        [Fact]
        public async Task RunAsync_ProcessesChangeLogFromClient()
        {
            // Arrange
            var logger = Mock.Of<ILogger<SIUserAddressImportJob>>();
            var timeProvider = TimeProvider.System;

            var changeLogClient = new Mock<IChangeLogClient>();
            var changelogSyncMetadataRepository = new Mock<IChangelogSyncMetadataRepository>();
            var contactInfoSyncRepository = new Mock<ISIUserContactInfoSyncRepository>();
            var registerClient = new Mock<IRegisterClient>();
            var addressVerificationRepository = new Mock<IAddressVerificationRepository>();

            var testChangeDate = DateTime.UtcNow.AddDays(-1);

            changelogSyncMetadataRepository
                .Setup(r => r.GetLatestSyncTimestampAsync(DataType.PrivateConsentProfile, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testChangeDate);

            var expectedUserId = 123;
            var expectedUserUuid = Guid.NewGuid();
            var contactSettingsJson = $@"{{
                ""userId"": {expectedUserId},
                ""userName"": ""testuser"",
                ""emailAddress"": ""test@example.com"",
                ""phoneNumber"": ""12345678""
            }}";

            var changeLogItem = new ChangeLogItem
            {
                ProfileChangeLogId = 1,
                ChangeDatetime = DateTime.UtcNow,
                OperationType = OperationType.Insert,
                DataObject = contactSettingsJson,
                DataType = DataType.PrivateConsentProfile
            };

            var changeLog = new ChangeLog
            {
                ProfileChangeLogList = new List<ChangeLogItem> { changeLogItem }
            };

            changeLogClient
                .SetupSequence(c => c.GetChangeLog(It.IsAny<DateTime>(), DataType.PrivateConsentProfile, It.IsAny<CancellationToken>()))
                .ReturnsAsync(changeLog)
                .ReturnsAsync(new ChangeLog { ProfileChangeLogList = new List<ChangeLogItem>() });

            registerClient
                .Setup(r => r.GetUserUuid(expectedUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedUserUuid);

            contactInfoSyncRepository
                .Setup(r => r.InsertOrUpdate(It.IsAny<SiUserContactSettings>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserContactInfo
                {
                    UserId = expectedUserId,
                    UserUuid = expectedUserUuid,
                    Username = "testuser",
                    CreatedAt = DateTime.UtcNow,
                    EmailAddress = "test@example.com",
                    PhoneNumber = "+4712345678",
                });

            var job = new TestableSIUserAddressImportJob(
                logger,
                changeLogClient.Object,
                timeProvider,
                changelogSyncMetadataRepository.Object,
                contactInfoSyncRepository.Object,
                registerClient.Object,
                addressVerificationRepository.Object,
                null);

            // Act
            await job.InvokeRunAsync(TestContext.Current.CancellationToken);

            // Assert
            contactInfoSyncRepository.Verify(
                r => r.InsertOrUpdate(
                    It.Is<SiUserContactSettings>(s => s.UserId == expectedUserId && s.UserUuid == expectedUserUuid),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            changelogSyncMetadataRepository.Verify(
                r => r.UpdateLatestChangeTimestampAsync(
                    It.IsAny<DateTime>(),
                    DataType.PrivateConsentProfile),
                Times.AtLeastOnce);

            changeLogClient.Verify(
                c => c.GetChangeLog(It.IsAny<DateTime>(), DataType.PrivateConsentProfile, It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task RunAsync_WhenEmailIdentifiedUser_ProcessesChangeLogFromClient()
        {
            // Arrange
            var logger = Mock.Of<ILogger<SIUserAddressImportJob>>();
            var timeProvider = TimeProvider.System;

            var changeLogClient = new Mock<IChangeLogClient>();
            var changelogSyncMetadataRepository = new Mock<IChangelogSyncMetadataRepository>();
            var contactInfoSyncRepository = new Mock<ISIUserContactInfoSyncRepository>();
            var registerClient = new Mock<IRegisterClient>();
            var addressVerificationRepository = new Mock<IAddressVerificationRepository>();

            var testChangeDate = DateTime.UtcNow.AddDays(-1);

            changelogSyncMetadataRepository
                .Setup(r => r.GetLatestSyncTimestampAsync(DataType.PrivateConsentProfile, It.IsAny<CancellationToken>()))
                .ReturnsAsync(testChangeDate);

            var expectedUserId = 123;
            var expectedUserUuid = Guid.NewGuid();
            var contactSettingsJson = $@"{{
                ""userId"": {expectedUserId},
                ""userName"": ""epost:test@example.com"",
                ""emailAddress"": ""test@example.com"",
                ""phoneNumber"": ""12345678""
            }}";

            var changeLogItem = new ChangeLogItem
            {
                ProfileChangeLogId = 1,
                ChangeDatetime = DateTime.UtcNow,
                OperationType = OperationType.Insert,
                DataObject = contactSettingsJson,
                DataType = DataType.PrivateConsentProfile
            };

            var changeLog = new ChangeLog
            {
                ProfileChangeLogList = new List<ChangeLogItem> { changeLogItem }
            };

            changeLogClient
                .SetupSequence(c => c.GetChangeLog(It.IsAny<DateTime>(), DataType.PrivateConsentProfile, It.IsAny<CancellationToken>()))
                .ReturnsAsync(changeLog)
                .ReturnsAsync(new ChangeLog { ProfileChangeLogList = new List<ChangeLogItem>() });

            registerClient
                .Setup(r => r.GetUserUuid(expectedUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedUserUuid);

            contactInfoSyncRepository
                .Setup(r => r.InsertOrUpdate(It.IsAny<SiUserContactSettings>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UserContactInfo
                {
                    UserId = expectedUserId,
                    UserUuid = expectedUserUuid,
                    Username = "testuser",
                    CreatedAt = DateTime.UtcNow,
                    EmailAddress = "test@example.com",
                    PhoneNumber = "+4712345678",
                });

            addressVerificationRepository
                .Setup(r => r.AddVerifiedAddressAsync(expectedUserId, AddressType.Email, "test@example.com", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var job = new TestableSIUserAddressImportJob(
                logger,
                changeLogClient.Object,
                timeProvider,
                changelogSyncMetadataRepository.Object,
                contactInfoSyncRepository.Object,
                registerClient.Object,
                addressVerificationRepository.Object,
                null);

            // Act
            await job.InvokeRunAsync(TestContext.Current.CancellationToken);

            // Assert
            contactInfoSyncRepository.Verify(
                r => r.InsertOrUpdate(
                    It.Is<SiUserContactSettings>(s => s.UserId == expectedUserId && s.UserUuid == expectedUserUuid),
                    It.IsAny<DateTime>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            changelogSyncMetadataRepository.Verify(
                r => r.UpdateLatestChangeTimestampAsync(
                    It.IsAny<DateTime>(),
                    DataType.PrivateConsentProfile),
                Times.AtLeastOnce);

            changeLogClient.Verify(
                c => c.GetChangeLog(It.IsAny<DateTime>(), DataType.PrivateConsentProfile, It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);

            addressVerificationRepository.Verify(
                r => r.AddVerifiedAddressAsync(expectedUserId, AddressType.Email, "test@example.com", It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task RunAsync_LogsWarningOnDeserializationFailure()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SIUserAddressImportJob>>();
            var timeProvider = TimeProvider.System;

            var changeLogClient = new Mock<IChangeLogClient>();
            var changelogSyncMetadataRepository = new Mock<IChangelogSyncMetadataRepository>();
            var contactInfoSyncRepository = new Mock<ISIUserContactInfoSyncRepository>();
            var registerClient = new Mock<IRegisterClient>();
            var addressVerificationRepository = new Mock<IAddressVerificationRepository>();

            changelogSyncMetadataRepository
                .Setup(r => r.GetLatestSyncTimestampAsync(DataType.PrivateConsentProfile, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DateTime?)null);

            var changeLogItem = new ChangeLogItem
            {
                ProfileChangeLogId = 1,
                ChangeDatetime = DateTime.UtcNow,
                OperationType = OperationType.Insert,
                DataObject = "this-is-not-json",
                DataType = DataType.PrivateConsentProfile
            };

            var changeLog = new ChangeLog
            {
                ProfileChangeLogList = new List<ChangeLogItem> { changeLogItem }
            };

            var callCount = 0;
            changeLogClient
                .Setup(c => c.GetChangeLog(It.IsAny<DateTime>(), DataType.PrivateConsentProfile, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    return callCount == 1 ? changeLog : new ChangeLog { ProfileChangeLogList = new List<ChangeLogItem>() };
                });

            var job = new TestableSIUserAddressImportJob(
                loggerMock.Object,
                changeLogClient.Object,
                timeProvider,
                changelogSyncMetadataRepository.Object,
                contactInfoSyncRepository.Object,
                registerClient.Object,
                addressVerificationRepository.Object,
                null);

            // Act
            await job.InvokeRunAsync(TestContext.Current.CancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to deserialize SiUserContactSettings change log item")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            contactInfoSyncRepository.Verify(
                r => r.InsertOrUpdate(It.IsAny<SiUserContactSettings>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Never);

            registerClient.Verify(
                r => r.GetUserUuid(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task RunAsync_LogsWarningWhenUserUuidNotFound()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SIUserAddressImportJob>>();
            var timeProvider = TimeProvider.System;

            var changeLogClient = new Mock<IChangeLogClient>();
            var changelogSyncMetadataRepository = new Mock<IChangelogSyncMetadataRepository>();
            var contactInfoSyncRepository = new Mock<ISIUserContactInfoSyncRepository>();
            var registerClient = new Mock<IRegisterClient>();
            var addressVerificationRepository = new Mock<IAddressVerificationRepository>();

            changelogSyncMetadataRepository
                .Setup(r => r.GetLatestSyncTimestampAsync(DataType.PrivateConsentProfile, It.IsAny<CancellationToken>()))
                .ReturnsAsync((DateTime?)null);

            var expectedUserId = 456;
            var contactSettingsJson = $@"{{
                ""userId"": {expectedUserId},
                ""userName"": ""unknownuser"",
                ""emailAddress"": ""some@value.com"",
                ""phoneNumber"": null
            }}";

            var changeLogItem = new ChangeLogItem
            {
                ProfileChangeLogId = 2,
                ChangeDatetime = DateTime.UtcNow,
                OperationType = OperationType.Insert,
                DataObject = contactSettingsJson,
                DataType = DataType.PrivateConsentProfile
            };

            var changeLog = new ChangeLog
            {
                ProfileChangeLogList = new List<ChangeLogItem> { changeLogItem }
            };

            var callCount = 0;
            changeLogClient
                .Setup(c => c.GetChangeLog(It.IsAny<DateTime>(), DataType.PrivateConsentProfile, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    return callCount == 1 ? changeLog : new ChangeLog { ProfileChangeLogList = new List<ChangeLogItem>() };
                });

            registerClient
                .Setup(r => r.GetUserUuid(expectedUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid?)null);

            var job = new TestableSIUserAddressImportJob(
                loggerMock.Object,
                changeLogClient.Object,
                timeProvider,
                changelogSyncMetadataRepository.Object,
                contactInfoSyncRepository.Object,
                registerClient.Object,
                addressVerificationRepository.Object,
                null);

            // Act
            await job.InvokeRunAsync(TestContext.Current.CancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Could not find user with id")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            contactInfoSyncRepository.Verify(
                r => r.InsertOrUpdate(It.IsAny<SiUserContactSettings>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        private class TestableSIUserAddressImportJob : SIUserAddressImportJob
        {
            public TestableSIUserAddressImportJob(
                ILogger<SIUserAddressImportJob> logger,
                IChangeLogClient changeLogClient,
                TimeProvider timeProvider,
                IChangelogSyncMetadataRepository changelogSyncMetadataRepository,
                ISIUserContactInfoSyncRepository userContactInfoSyncRepository,
                IRegisterClient registerClient,
                IAddressVerificationRepository addressVerificationRepository,
                Telemetry telemetry = null)
                : base(logger, changeLogClient, timeProvider, changelogSyncMetadataRepository, userContactInfoSyncRepository, registerClient, addressVerificationRepository, telemetry)
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
