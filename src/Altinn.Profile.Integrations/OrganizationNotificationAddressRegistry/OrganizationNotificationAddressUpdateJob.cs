﻿using Altinn.Profile.Integrations.Repositories;
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
public class OrganizationNotificationAddressUpdateJob(
    IOrganizationNotificationAddressSyncClient organizationNotificationAddressHttpClient,
    IRegistrySyncMetadataRepository metadataRepository,
    IOrganizationNotificationAddressUpdater notificationAddressUpdater,
    ILogger<OrganizationNotificationAddressUpdateJob> logger)
    : IOrganizationNotificationAddressSyncJob
{
    private readonly IOrganizationNotificationAddressSyncClient _organizationNotificationAddressHttpClient = organizationNotificationAddressHttpClient;
    private readonly IRegistrySyncMetadataRepository _metadataRepository = metadataRepository;
    private readonly IOrganizationNotificationAddressUpdater _notificationAddressUpdater = notificationAddressUpdater;
    private readonly ILogger<OrganizationNotificationAddressUpdateJob> _logger = logger;

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown when the endpoint URL is null or empty.</exception>
    public async Task SyncNotificationAddressesAsync()
    {
        DateTime? lastUpdated = await _metadataRepository.GetLatestSyncTimestampAsync();

        var fullUrl = _organizationNotificationAddressHttpClient.GetInitialUrl(lastUpdated);

        do
        {
            _logger.LogInformation("Fetch data from brreg at url: {FullUrl}", fullUrl);

            NotificationAddressChangesLog changesLog = await _organizationNotificationAddressHttpClient.GetAddressChangesAsync(fullUrl);

            var noChangesSinceLastCheck = changesLog.OrganizationNotificationAddressList == null || changesLog.OrganizationNotificationAddressList?.Count == 0;
            if (noChangesSinceLastCheck)
            {
                break;
            }

            int updatedRowsCount = await _notificationAddressUpdater.SyncNotificationAddressesAsync(changesLog);

            if (updatedRowsCount > 0)
            {
                var lastUpdatedTimestamp = changesLog.OrganizationNotificationAddressList![^1].Updated;
                await _metadataRepository.UpdateLatestChangeTimestampAsync(lastUpdatedTimestamp);
            }
            else
            {
                break;
            }

            fullUrl = changesLog.NextPage?.ToString();
        }
        while (!string.IsNullOrEmpty(fullUrl));
    }
}
