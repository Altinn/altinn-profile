using Altinn.Profile.Integrations.Repositories;

namespace Altinn.Profile.Integrations.OrganizationNotificationAddressRegistry;

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
        DateTime lastUpdated = await _metadataRepository.GetLatestSyncTimestampAsync();

        // Time should be in iso8601 format. Example: 2018-02-15T11:07:12Z
        string? fullUrl = _organizationNotificationAddressSettings.ChangesLogEndpoint + $"?since={lastUpdated.ToString("yyyy-MM-ddTHH\\:mm\\:ssZ")}&pageSize={_organizationNotificationAddressSettings.ChangesLogPageSize}";

        do
        {
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
