using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Authorization.ServiceDefaults.Jobs;
using Altinn.Profile.Core.Telemetry;
using Altinn.Profile.Core.User.ProfileSettings;
using Altinn.Profile.Integrations.Repositories.A2Sync;
using Altinn.Profile.Integrations.SblBridge.Changelog;

using Microsoft.Extensions.Logging;

using static Altinn.Profile.Integrations.SblBridge.Changelog.ChangeLogItem;

namespace Altinn.Profile.Changelog
{
    /// <summary>
    /// A job that imports ProfileSettings from A2.
    /// </summary>
    /// <remarks>Can be removed when Altinn2 is decommissioned</remarks>
    public partial class ProfileSettingImportJob : Job
    {
        private readonly ILogger<ProfileSettingImportJob> _logger;
        private readonly TimeProvider _timeProvider;
        private readonly IChangeLogClient _changeLogClient;
        private readonly IChangelogSyncMetadataRepository _changelogSyncMetadataRepository;
        private readonly Telemetry _telemetry;
        private readonly IProfileSettingsSyncRepository _profileSettingsSyncRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileSettingImportJob"/> class.
        /// </summary>
        public ProfileSettingImportJob(
            ILogger<ProfileSettingImportJob> logger,
            IChangeLogClient changeLogClient,
            TimeProvider timeProvider,
            IChangelogSyncMetadataRepository changelogSyncMetadataRepository,
            IProfileSettingsSyncRepository profileSettingsSyncRepository,
            Telemetry telemetry = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _changeLogClient = changeLogClient ?? throw new ArgumentNullException(nameof(changeLogClient));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            _changelogSyncMetadataRepository = changelogSyncMetadataRepository ?? throw new ArgumentNullException(nameof(changelogSyncMetadataRepository));
            _profileSettingsSyncRepository = profileSettingsSyncRepository ?? throw new ArgumentNullException(nameof(profileSettingsSyncRepository));
            _telemetry = telemetry;
        }

        /// <inheritdoc/>
        protected override async Task RunAsync(CancellationToken cancellationToken = default)
        {
            var start = _timeProvider.GetTimestamp();
            Log.StartingProfileSettingsImport(_logger);

            using var activity = _telemetry?.StartA2ImportJob("ProfileSettings");

            await RunProfileSettingsImport(cancellationToken);
            var duration = _timeProvider.GetElapsedTime(start);

            Log.FinishedProfileSettingsImport(_logger, duration);
        }

        private async Task RunProfileSettingsImport(CancellationToken cancellationToken)
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
                        _logger.LogWarning("Failed to deserialize ProfileSetting change log item with id {ChangeId}", change.ProfileChangeLogId);
                        continue;
                    }

                    var profileSettings = new ProfileSettings
                    {
                        LanguageType = LanguageType.GetFromAltinn2Code(portalSetting.LanguageType),
                        UserId = portalSetting.UserId,
                        DoNotPromptForParty = portalSetting.DoNotPromptForParty,
                        PreselectedPartyUuid = portalSetting.PreselectedPartyUuid,
                        ShowClientUnits = portalSetting.ShowClientUnits,
                        ShouldShowSubEntities = portalSetting.ShouldShowSubEntities,
                        ShouldShowDeletedEntities = portalSetting.ShouldShowDeletedEntities,
                        IgnoreUnitProfileDateTime = portalSetting.IgnoreUnitProfileDateTime?.ToUniversalTime(),
                    };

                    change.ChangeDatetime = change.ChangeDatetime.ToUniversalTime();
                    if (change.OperationType is OperationType.Insert or OperationType.Update)
                    {
                        await _profileSettingsSyncRepository.UpdateProfileSettings(profileSettings);
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
            [LoggerMessage(3, LogLevel.Information, "Starting ProfileSettings import.")]
            public static partial void StartingProfileSettingsImport(ILogger logger);

            [LoggerMessage(4, LogLevel.Information, "Finished ProfileSettings import in {Duration}.")]
            public static partial void FinishedProfileSettingsImport(ILogger logger, TimeSpan duration);
        }
    }
}
