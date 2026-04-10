using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Authorization.ServiceDefaults.Jobs;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Core.Telemetry;
using Altinn.Profile.Integrations.Repositories.A2Sync;
using Altinn.Profile.Integrations.SblBridge.Changelog;
using Altinn.Profile.Integrations.SblBridge.User.PrivateConsent;

using Microsoft.Extensions.Logging;

namespace Altinn.Profile.Changelog
{
    /// <summary>
    /// A job that imports addresses for SI users from A2.
    /// </summary>
    /// <remarks>Can be removed when Altinn2 is decommissioned</remarks>
    public partial class SIUserAddressImportJob : Job
    {
        private readonly ILogger<SIUserAddressImportJob> _logger;
        private readonly TimeProvider _timeProvider;
        private readonly IChangeLogClient _changeLogClient;
        private readonly IChangelogSyncMetadataRepository _changelogSyncMetadataRepository;
        private readonly Telemetry _telemetry;
        private readonly ISIUserContactInfoSyncRepository _siUserContactInfoSyncRepository;
        private readonly IRegisterClient _registerClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="SIUserAddressImportJob"/> class.
        /// </summary>
        public SIUserAddressImportJob(
            ILogger<SIUserAddressImportJob> logger,
            IChangeLogClient changeLogClient,
            TimeProvider timeProvider,
            IChangelogSyncMetadataRepository changelogSyncMetadataRepository,
            ISIUserContactInfoSyncRepository userContactInfoSyncRepository,
            IRegisterClient registerClient,
            Telemetry telemetry = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _changeLogClient = changeLogClient ?? throw new ArgumentNullException(nameof(changeLogClient));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            _changelogSyncMetadataRepository = changelogSyncMetadataRepository ?? throw new ArgumentNullException(nameof(changelogSyncMetadataRepository));
            _siUserContactInfoSyncRepository = userContactInfoSyncRepository ?? throw new ArgumentNullException(nameof(userContactInfoSyncRepository));
            _registerClient = registerClient ?? throw new ArgumentNullException(nameof(registerClient));
            _telemetry = telemetry;
        }

        /// <inheritdoc/>
        protected override async Task RunAsync(CancellationToken cancellationToken = default)
        {
            var start = _timeProvider.GetTimestamp();
            Log.StartingSIUserAddressImport(_logger);

            using var activity = _telemetry?.StartA2ImportJob("SIUserAddress");

            await RunSIUserContactSettingsImport(cancellationToken);
            var duration = _timeProvider.GetElapsedTime(start);

            Log.FinishedSIUserAddressImport(_logger, duration);
        }

        private async Task RunSIUserContactSettingsImport(CancellationToken cancellationToken)
        {
            var lastChangeDate = await _changelogSyncMetadataRepository.GetLatestSyncTimestampAsync(DataType.PrivateConsentProfile, cancellationToken);

            var changes = GetChangeLogPage(lastChangeDate ?? DateTime.MinValue, cancellationToken);

            await foreach (var page in changes.WithCancellation(cancellationToken))
            {
                var lastChange = page.ProfileChangeLogList[^1].ChangeDatetime.ToUniversalTime();

                if (page.ProfileChangeLogList.Count == 0)
                {
                    // Skip empty pages.
                    continue;
                }

                foreach (var change in page.ProfileChangeLogList)
                {
                    SiUserContactSettings contactSettings = Deserialize(change);
                    if (contactSettings == null)
                    {
                        continue;
                    }

                    var userUuid = await _registerClient.GetUserUuid(contactSettings.UserId, cancellationToken);
                    if (userUuid == null)
                    {
                        await _changelogSyncMetadataRepository.UpdateLatestChangeTimestampAsync(lastChange, DataType.PrivateConsentProfile);
                        _logger.LogWarning("Could not find user with id {UserId} in Register, breaking the import. Change log item id: {ChangeId}", contactSettings.UserId, change.ProfileChangeLogId);
                        return;
                    }

                    contactSettings.UserUuid = (Guid)userUuid;

                    if (change.OperationType is OperationType.Insert or OperationType.Update)
                    {
                        await _siUserContactInfoSyncRepository.InsertOrUpdate(contactSettings, change.ChangeDatetime, cancellationToken);
                    }

                    lastChange = change.ChangeDatetime.ToUniversalTime();
                }

                await _changelogSyncMetadataRepository.UpdateLatestChangeTimestampAsync(lastChange, DataType.PrivateConsentProfile);
            }
        }

        private async IAsyncEnumerable<ChangeLog> GetChangeLogPage(DateTime from, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ChangeLog response;
            while (true)
            {
                response = await _changeLogClient.GetChangeLog(from, DataType.PrivateConsentProfile, cancellationToken);

                if (response == null || response.ProfileChangeLogList.Count == 0)
                {
                    break;
                }

                yield return response;

                from = response.ProfileChangeLogList[^1].ChangeDatetime;
            }
        }

        private SiUserContactSettings Deserialize(ChangeLogItem change)
        {
            SiUserContactSettings contactSettings;
            try
            {
                contactSettings = SiUserContactSettings.Deserialize(change.DataObject);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize SiUserContactSettings change log item with id {ChangeId}", change.ProfileChangeLogId);
                return null;
            }

            return contactSettings;
        }

        private static partial class Log
        {
            [LoggerMessage(3, LogLevel.Information, "Starting SIUser Address import.")]
            public static partial void StartingSIUserAddressImport(ILogger logger);

            [LoggerMessage(4, LogLevel.Information, "Finished SIUser Address import in {Duration}.")]
            public static partial void FinishedSIUserAddressImport(ILogger logger, TimeSpan duration);
        }
    }
}
