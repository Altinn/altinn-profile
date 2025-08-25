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
    /// A job that imports favorites from A2.
    /// </summary>
    public partial class FavoriteImportJob : Job
    {
        private readonly ILogger<FavoriteImportJob> _logger;
        private readonly TimeProvider _timeProvider;
        private readonly IChangeLogClient _changeLogClient;
        private readonly IChangelogSyncMetadataRepository _changelogSyncMetadataRepository;
        private readonly IFavoriteSyncRepository _favoriteSyncRepository;
        private readonly Telemetry _telemetry;

        /// <summary>
        /// Initializes a new instance of the <see cref="FavoriteImportJob"/> class.
        /// </summary>
        public FavoriteImportJob(
            ILogger<FavoriteImportJob> logger,
            IChangeLogClient changeLogClient,
            TimeProvider timeProvider,
            IChangelogSyncMetadataRepository changelogSyncMetadataRepository, 
            IFavoriteSyncRepository favoriteSyncRepository,
            Telemetry telemetry = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _changeLogClient = changeLogClient ?? throw new ArgumentNullException(nameof(changeLogClient));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            _changelogSyncMetadataRepository = changelogSyncMetadataRepository ?? throw new ArgumentNullException(nameof(changelogSyncMetadataRepository));
            _favoriteSyncRepository = favoriteSyncRepository ?? throw new ArgumentNullException(nameof(favoriteSyncRepository));
            _telemetry = telemetry;
        }

        /// <inheritdoc/>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var start = _timeProvider.GetTimestamp();
            Log.StartingFavoritesImport(_logger);

            using var activity = _telemetry?.StartA2ImportJob("Favorites");

            await RunFavoritesImport(cancellationToken);
            var duration = _timeProvider.GetElapsedTime(start);

            Log.FinishedFavoritesImport(_logger, duration);
        }

        private async Task RunFavoritesImport(CancellationToken cancellationToken)
        {
            var lastChangeDate = await _changelogSyncMetadataRepository.GetLatestSyncTimestampAsync(DataType.Favorites, cancellationToken);

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
                    var favorite = Favorite.Deserialize(change.DataObject);
                    if (favorite == null)
                    {
                        _logger.LogWarning("Failed to deserialize favorite change log item with id {ChangeId}", change.ProfileChangeLogId);
                        continue;
                    }

                    if (change.OperationType == OperationType.Insert)
                    {
                        await _favoriteSyncRepository.AddPartyToFavorites(favorite.UserId, favorite.PartyUuid, change.ChangeDatetime, cancellationToken);
                    }
                    else if (change.OperationType == OperationType.Delete)
                    {
                        await _favoriteSyncRepository.DeleteFromFavorites(favorite.UserId, favorite.PartyUuid, cancellationToken);
                    }
                }

                var lastChange = page.ProfileChangeLogList.Last().ChangeDatetime;
                await _changelogSyncMetadataRepository.UpdateLatestChangeTimestampAsync(lastChange, DataType.Favorites);
            }
        }

        private async IAsyncEnumerable<ChangeLog> GetChangeLogPage(DateTime from, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ChangeLog response;
            while (true)
            {
                response = await _changeLogClient.GetChangeLog(from, DataType.Favorites, cancellationToken);

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
            [LoggerMessage(3, LogLevel.Information, "Starting favorites import.")]
            public static partial void StartingFavoritesImport(ILogger logger);

            [LoggerMessage(4, LogLevel.Information, "Finished favorites import in {Duration}.")]
            public static partial void FinishedFavoritesImport(ILogger logger, TimeSpan duration);
        }
    }
}
