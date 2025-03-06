using Altinn.Profile.Integrations.Repositories;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddress;

/// <summary>
/// An implementation of the <see cref="IOrganizationNotificationAddressUpdateJob"/> interface that will retrieve 
/// changes from the source registry and update the local contact information.
/// </summary>
/// <param name="organizationNotificationAddressSettings">Settings for the synchronization update job</param>
/// <param name="organizationNotificationAddressHttpClient">A HTTP client that can be used to retrieve contact details changes</param>
/// <param name="metadataRepository">A repository implementation for managing persistence of the job status between runs</param>
/// <param name="notificationAddressUpdater">A repository implementation for managing persistence for the local contact information</param>
public class OrganizationNotificationAddressUpdateJob(
    OrganizationNotificationAddressSettings organizationNotificationAddressSettings,
    IOrganizationNotificationAddressHttpClient organizationNotificationAddressHttpClient,
    IRegistrySyncMetadataRepository metadataRepository,
    IOrganizationNotificationAddressUpdater notificationAddressUpdater)
    : IOrganizationNotificationAddressUpdateJob
{
    private readonly OrganizationNotificationAddressSettings _organizationNotificationAddressSettings = organizationNotificationAddressSettings;
    private readonly IOrganizationNotificationAddressHttpClient _organizationNotificationAddressHttpClient = organizationNotificationAddressHttpClient;
    private readonly IRegistrySyncMetadataRepository _metadataRepository = metadataRepository;
    private readonly IOrganizationNotificationAddressUpdater _notificationAddressUpdater = notificationAddressUpdater;

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown when the endpoint URL is null or empty.</exception>
    public async Task SyncNotificationAddressesAsync()
    {
        if (string.IsNullOrWhiteSpace(_organizationNotificationAddressSettings.ChangesLogEndpoint))
        {
            throw new InvalidOperationException("The endpoint URL must not be null or empty.");
        }

        DateTime lastUpdated = await _metadataRepository.GetLatestSyncTimestampAsync();

        string? fullUrl = _organizationNotificationAddressSettings.ChangesLogEndpoint + $"?since={lastUpdated.ToString("s")}&pageSize={_organizationNotificationAddressSettings.ChangesLogPageSize}";

        do
        {
            NotificationAddressChangesLog changesLog = await _organizationNotificationAddressHttpClient.GetAddressChangesAsync(fullUrl);

            var noChangesSinceLastCheck = changesLog.OrganizationNotificationAddressList?.Count == 0;
            if (noChangesSinceLastCheck)
            {
                break;
            }

            int updatedRowsCount = await _notificationAddressUpdater.SyncNotificationAddressesAsync(changesLog);

            if (updatedRowsCount > 0 && changesLog.Updated.HasValue)
            {
                var lastUpdatedTimestamp = changesLog.Updated;
                await _metadataRepository.UpdateLatestChangeTimestampAsync((DateTime)lastUpdatedTimestamp);
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
