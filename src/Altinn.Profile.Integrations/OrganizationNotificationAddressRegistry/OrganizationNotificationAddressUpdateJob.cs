using Altinn.Profile.Core.Telemetry;
using Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry.Models;
using Altinn.Profile.Integrations.Repositories;
using Microsoft.Extensions.Logging;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;

/// <summary>
/// An implementation of the <see cref="IOrganizationNotificationAddressSyncJob"/> interface that will retrieve 
/// changes from the source registry and update the local contact information.
/// </summary>
/// <param name="organizationNotificationAddressHttpClient">A HTTP client that can be used to retrieve contact details changes</param>
/// <param name="metadataRepository">A repository implementation for managing persistence of the job status between runs</param>
/// <param name="notificationAddressUpdater">A repository implementation for managing persistence for the local contact information</param>
/// <param name="logger">A logger to log detailed information.</param>
/// <param name="telemetry">Optional telemetry instance for tracking job execution.</param>
public class OrganizationNotificationAddressUpdateJob(
    IOrganizationNotificationAddressSyncClient organizationNotificationAddressHttpClient,
    IRegistrySyncMetadataRepository metadataRepository,
    IOrganizationNotificationAddressUpdater notificationAddressUpdater,
    ILogger<OrganizationNotificationAddressUpdateJob> logger,
    Telemetry? telemetry = null)
    : IOrganizationNotificationAddressSyncJob
{
    private readonly IOrganizationNotificationAddressSyncClient _organizationNotificationAddressHttpClient = organizationNotificationAddressHttpClient;
    private readonly IRegistrySyncMetadataRepository _metadataRepository = metadataRepository;
    private readonly IOrganizationNotificationAddressUpdater _notificationAddressUpdater = notificationAddressUpdater;
    private readonly ILogger<OrganizationNotificationAddressUpdateJob> _logger = logger;
    private readonly Telemetry? _telemetry = telemetry;

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown when the endpoint URL is null or empty.</exception>
    public async Task SyncNotificationAddressesAsync(CancellationToken cancellationToken = default)
    {
        using var activity = _telemetry?.StartOrganizationNotificationAddressUpdateJob();

        DateTime? lastUpdated = await _metadataRepository.GetLatestSyncTimestampAsync(cancellationToken);

        var fullUrl = _organizationNotificationAddressHttpClient.GetInitialUrl(lastUpdated);

        do
        {
            cancellationToken.ThrowIfCancellationRequested();

            _logger.LogInformation("Fetch data from brreg at url: {FullUrl}", fullUrl);

            NotificationAddressChangesLog? changesLog = await _organizationNotificationAddressHttpClient.GetAddressChangesAsync(fullUrl, cancellationToken);

            var entries = changesLog?.OrganizationNotificationAddressList;
            if (entries is null || entries.Count == 0)
            {
                // We have caught up with the feed, so there is nothing to move the watermark to.
                break;
            }

            int updatedRowsCount = await _notificationAddressUpdater.SyncNotificationAddressesAsync(changesLog!, cancellationToken);

            fullUrl = changesLog!.NextPage?.ToString();

            // The feed is sorted ascending on "updated" and "since" is exclusive, so a processed page is
            // committed by storing a timestamp from it. This has to happen even when the page caused no
            // database writes, for instance when every entry has an identifier type we ignore. Otherwise the
            // exact same page is requested on every later run and the sync never progresses.
            var watermark = GetCommittableWatermark(entries, hasMorePages: !string.IsNullOrEmpty(fullUrl));
            if (watermark.HasValue)
            {
                await _metadataRepository.UpdateLatestChangeTimestampAsync(watermark.Value, cancellationToken);
            }

            _logger.LogInformation(
                "Processed {EntryCount} entries from brreg up to {LastUpdated}, {UpdatedRowsCount} rows written",
                entries.Count,
                watermark,
                updatedRowsCount);
        }
        while (!string.IsNullOrEmpty(fullUrl));
    }

    /// <summary>
    /// Picks the timestamp that can safely be stored as the new watermark for a processed page.
    /// </summary>
    /// <param name="entries">The entries of the page, sorted ascending on their updated timestamp.</param>
    /// <param name="hasMorePages">Whether the feed has further pages after this one.</param>
    /// <returns>The timestamp to store, or null when this page must not move the watermark.</returns>
    /// <remarks>
    /// Several entries can share the same updated timestamp, and such a group can be split across a page
    /// boundary. Because "since" is exclusive, storing the last timestamp of a page that has more pages behind
    /// it would make an interrupted run resume after that group, and the remainder of the group would never be
    /// read again. While more pages follow we therefore only commit up to the last timestamp we know is
    /// complete. On the final page there is nothing left to split, so the last entry is committed as is.
    /// </remarks>
    private static DateTime? GetCommittableWatermark(List<Entry> entries, bool hasMorePages)
    {
        var lastUpdated = entries[^1].Updated;

        if (!hasMorePages)
        {
            return lastUpdated;
        }

        for (var i = entries.Count - 1; i >= 0; i--)
        {
            if (entries[i].Updated < lastUpdated)
            {
                return entries[i].Updated;
            }
        }

        // Every entry on this page shares one timestamp, so no part of it is known to be complete. The run
        // continues on the next page, which will move the watermark past this group.
        return null;
    }
}
