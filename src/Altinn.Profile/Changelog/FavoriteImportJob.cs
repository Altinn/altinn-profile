using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Altinn.Authorization.ServiceDefaults.Jobs;
using Altinn.Profile.Core.Integrations;
using Altinn.Profile.Integrations.Repositories;
using Altinn.Profile.Integrations.SblBridge.Changelog;

using Microsoft.Extensions.Logging;

using OpenTelemetry.Trace;

using static Altinn.Profile.Integrations.SblBridge.Changelog.ChangeLogItem;

namespace Altinn.Profile.Changelog
{
    /// <summary>
    /// A job that imports favorites from A2.
    /// </summary>
    public class FavoriteImportJob : Job
    {
        private readonly ILogger<FavoriteImportJob> _logger;
        private readonly TimeProvider _timeProvider;
        private readonly IChangeLogClient _changeLogClient;
        private readonly IChangelogSyncMetadataRepository _changelogSyncMetadataRepository;
        private readonly IFavoriteSyncRepository _favoriteSyncRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="FavoriteImportJob"/> class.
        /// </summary>
        public FavoriteImportJob(
            ILogger<FavoriteImportJob> logger,
            IChangeLogClient changeLogClient,
            TimeProvider timeProvider,
            IChangelogSyncMetadataRepository changelogSyncMetadataRepository, 
            IFavoriteSyncRepository favoriteSyncRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _changeLogClient = changeLogClient ?? throw new ArgumentNullException(nameof(changeLogClient));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            _changelogSyncMetadataRepository = changelogSyncMetadataRepository ?? throw new ArgumentNullException(nameof(changelogSyncMetadataRepository));
            _favoriteSyncRepository = favoriteSyncRepository ?? throw new ArgumentNullException(nameof(favoriteSyncRepository));
        }

        /// <inheritdoc/>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            var start = _timeProvider.GetTimestamp();

            await RunFavoritesImport(cancellationToken);
            var duration = _timeProvider.GetElapsedTime(start);
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

                if (response.ProfileChangeLogList.Count == 0)
                {
                    break;
                }

                yield return response;

                from = response.ProfileChangeLogList.Last().ChangeDatetime;
            }
        }
    }
}
