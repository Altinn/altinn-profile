using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Altinn.Authorization.ServiceDefaults.Jobs;
using Altinn.Profile.Core.Telemetry;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Integrations.SblBridge.Changelog;
using Microsoft.Extensions.Logging;
using static Altinn.Profile.Integrations.SblBridge.Changelog.ChangeLogItem;

namespace Altinn.Profile.Changelog
{
    /// <summary>
    /// A job that imports notificationSettings from A2.
    /// </summary>
    public partial class NotificationSettingImportJob : Job
    {
        private readonly ILogger<NotificationSettingImportJob> _logger;
        private readonly TimeProvider _timeProvider;
        private readonly IChangeLogClient _changeLogClient;
        private readonly IChangelogSyncMetadataRepository _changelogSyncMetadataRepository;
        private readonly INotificationSettingSyncRepository _notificationSettingSyncRepository;
        private readonly Telemetry _telemetry;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationSettingImportJob"/> class.
        /// </summary>
        public NotificationSettingImportJob(
            ILogger<NotificationSettingImportJob> logger,
            IChangeLogClient changeLogClient,
            TimeProvider timeProvider,
            IChangelogSyncMetadataRepository changelogSyncMetadataRepository, 
            INotificationSettingSyncRepository notificationSettingSyncRepository,
            Telemetry telemetry = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _changeLogClient = changeLogClient ?? throw new ArgumentNullException(nameof(changeLogClient));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            _changelogSyncMetadataRepository = changelogSyncMetadataRepository ?? throw new ArgumentNullException(nameof(changelogSyncMetadataRepository));
            _notificationSettingSyncRepository = notificationSettingSyncRepository ?? throw new ArgumentNullException(nameof(notificationSettingSyncRepository));
            _telemetry = telemetry;
        }

        /// <inheritdoc/>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var start = _timeProvider.GetTimestamp();
            Log.StartingNotificationSettingsImport(_logger);

            using var activity = _telemetry?.StartA2ImportJob("NotificationSettings");

            await RunNotificationSettingsImport(cancellationToken);
            var duration = _timeProvider.GetElapsedTime(start);

            Log.FinishedNotificationSettingsImport(_logger, duration);
        }

        private async Task RunNotificationSettingsImport(CancellationToken cancellationToken)
        {
            var lastChangeDate = await _changelogSyncMetadataRepository.GetLatestSyncTimestampAsync(DataType.ProfessionalNotificationSettings, cancellationToken);

            var changes = GetChangeLogPage(lastChangeDate ?? DateTime.MinValue, cancellationToken);

            await foreach (var page in changes.WithCancellation(cancellationToken))
            {
                if (page.ProfileChangeLogList.Count == 0)
                {
                    // Skip empty pages.
                    continue;
                }

                foreach (var change in page.ProfileChangeLogList)
                {
                    var notificationSetting = ProfessionalNotificationSettings.Deserialize(change.DataObject);
                    if (notificationSetting == null)
                    {
                        _logger.LogWarning("Failed to deserialize notificationSetting change log item with id {ChangeId}", change.ProfileChangeLogId);
                        continue;
                    }

                    if (change.OperationType == OperationType.Insert)
                    {
                        await _notificationSettingSyncRepository.AddPartyToNotificationSettings(notificationSetting.UserId, notificationSetting.PartyUuid, change.ChangeDatetime, cancellationToken);
                    }
                    else if (change.OperationType == OperationType.Delete)
                    {
                        await _notificationSettingSyncRepository.DeleteFromNotificationSettings(notificationSetting.UserId, notificationSetting.PartyUuid, cancellationToken);
                    }
                }

                var lastChange = page.ProfileChangeLogList.Last().ChangeDatetime;
                await _changelogSyncMetadataRepository.UpdateLatestChangeTimestampAsync(lastChange, DataType.ProfessionalNotificationSettings);
            }
        }

        private async IAsyncEnumerable<ChangeLog> GetChangeLogPage(DateTime from, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ChangeLog response;
            while (true)
            {
                response = await _changeLogClient.GetChangeLog(from, DataType.ProfessionalNotificationSettings, cancellationToken);

                if (response == null || response.ProfileChangeLogList.Count == 0)
                {
                    break;
                }

                yield return response;

                from = response.ProfileChangeLogList.Last().ChangeDatetime;
            }
        }

        private static partial class Log
        {
            [LoggerMessage(3, LogLevel.Information, "Starting notificationSettings import.")]
            public static partial void StartingNotificationSettingsImport(ILogger logger);

            [LoggerMessage(4, LogLevel.Information, "Finished notificationSettings import in {Duration}.")]
            public static partial void FinishedNotificationSettingsImport(ILogger logger, TimeSpan duration);
        }
    }
}
