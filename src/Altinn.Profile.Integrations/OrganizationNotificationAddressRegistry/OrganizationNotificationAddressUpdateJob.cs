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
    public async Task SyncNotificationAddressesAsync()
    {
        using var activity = _telemetry?.StartOrganizationNotificationAddressUpdateJob();

        DateTime? lastUpdated = await _metadataRepository.GetLatestSyncTimestampAsync();

        var fullUrl = _organizationNotificationAddressHttpClient.GetInitialUrl(lastUpdated);

        do
        {
            _logger.LogInformation("Fetch data from brreg at url: {FullUrl}", fullUrl);

            NotificationAddressChangesLog? changesLog = await _organizationNotificationAddressHttpClient.GetAddressChangesAsync(fullUrl);

            var entries = changesLog?.OrganizationNotificationAddressList;
            if (entries is null || entries.Count == 0)
            {
                // We have caught up with the feed, so there is nothing to move the watermark to.
                break;
            }

            int updatedRowsCount = await _notificationAddressUpdater.SyncNotificationAddressesAsync(changesLog!);

            // The feed is sorted ascending on "updated" and "since" is exclusive, so a processed page is
            // committed by storing the timestamp of its last entry. This has to happen even when the page
            // caused no database writes, for instance when every entry has an identifier type we ignore.
            // Otherwise the exact same page is requested on every later run and the sync never progresses.
            var lastUpdatedTimestamp = entries[^1].Updated;
            await _metadataRepository.UpdateLatestChangeTimestampAsync(lastUpdatedTimestamp);

            _logger.LogInformation(
                "Processed {EntryCount} entries from brreg up to {LastUpdated}, {UpdatedRowsCount} rows written",
                entries.Count,
                lastUpdatedTimestamp,
                updatedRowsCount);

            fullUrl = changesLog!.NextPage?.ToString();
        }
        while (!string.IsNullOrEmpty(fullUrl));
    }
}
