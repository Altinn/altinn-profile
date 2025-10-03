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
    /// A job that imports PortalSettings from A2.
    /// </summary>
    public partial class PortalSettingImportJob : Job
    {
        private readonly ILogger<PortalSettingImportJob> _logger;
        private readonly TimeProvider _timeProvider;
        private readonly IChangeLogClient _changeLogClient;
        private readonly IChangelogSyncMetadataRepository _changelogSyncMetadataRepository;
        private readonly Telemetry _telemetry;

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalSettingImportJob"/> class.
        /// </summary>
        public PortalSettingImportJob(
            ILogger<PortalSettingImportJob> logger,
            IChangeLogClient changeLogClient,
            TimeProvider timeProvider,
            IChangelogSyncMetadataRepository changelogSyncMetadataRepository,
            Telemetry telemetry = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _changeLogClient = changeLogClient ?? throw new ArgumentNullException(nameof(changeLogClient));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            _changelogSyncMetadataRepository = changelogSyncMetadataRepository ?? throw new ArgumentNullException(nameof(changelogSyncMetadataRepository));
            _telemetry = telemetry;
        }

        /// <inheritdoc/>
        protected override async Task RunAsync(CancellationToken cancellationToken = default)
        {
            var start = _timeProvider.GetTimestamp();
            Log.StartingPortalSettingsImport(_logger);

            using var activity = _telemetry?.StartA2ImportJob("PortalSettings");

            await RunPortalSettingsImport(cancellationToken);
            var duration = _timeProvider.GetElapsedTime(start);

            Log.FinishedPortalSettingsImport(_logger, duration);
        }

        private async Task RunPortalSettingsImport(CancellationToken cancellationToken)
        {
            var lastChangeDate = await _changelogSyncMetadataRepository.GetLatestSyncTimestampAsync(DataType.PortalSettingPreferences, cancellationToken);

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
                    var portalSetting = PortalSettings.Deserialize(change.DataObject);
                    if (portalSetting == null)
                    {
                        _logger.LogWarning("Failed to deserialize PortalSetting change log item with id {ChangeId}", change.ProfileChangeLogId);
                        continue;
                    }

                    change.ChangeDatetime = change.ChangeDatetime.ToUniversalTime();
                    if (change.OperationType is OperationType.Insert or OperationType.Update)
                    {
                        // Do nothing
                    }
                    else if (change.OperationType == OperationType.Delete)
                    {
                        // Do nothing
                    }
                }

                var lastChange = page.ProfileChangeLogList[^1].ChangeDatetime;
                await _changelogSyncMetadataRepository.UpdateLatestChangeTimestampAsync(lastChange, DataType.PortalSettingPreferences);
            }
        }

        private async IAsyncEnumerable<ChangeLog> GetChangeLogPage(DateTime from, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ChangeLog response;
            while (true)
            {
                response = await _changeLogClient.GetChangeLog(from, DataType.PortalSettingPreferences, cancellationToken);

                if (response == null || response.ProfileChangeLogList.Count == 0)
                {
                    break;
                }

                yield return response;

                from = response.ProfileChangeLogList[^1].ChangeDatetime;
            }
        }

        private static partial class Log
        {
            [LoggerMessage(3, LogLevel.Information, "Starting PortalSettings import.")]
            public static partial void StartingPortalSettingsImport(ILogger logger);

            [LoggerMessage(4, LogLevel.Information, "Finished PortalSettings import in {Duration}.")]
            public static partial void FinishedPortalSettingsImport(ILogger logger, TimeSpan duration);
        }
    }
}
