using Altinn.Profile.Integrations.Repositories;

namespace Altinn.Profile.Integrations.OfficialAddressRegister;

/// <summary>
/// An implementation of the <see cref="IOfficialAddressRegisterUpdateJob"/> interface that will retrieve 
/// changes from the source registry and update the local contact information.
/// </summary>
/// <param name="OfficialAddressRegisterSettings">Settings for the synchronization update job</param>
/// <param name="OfficialAddressRegisterHttpClient">A HTTP client that can be used to retrieve contact details changes</param>
/// <param name="metadataRepository">A repository implementation for managing persistence of the job status between runs</param>
/// <param name="officialContactUpdater">A repository implementation for managing persistence for the local contact information</param>
public class OfficialAddressRegisterUpdateJob(
    OfficialAddressRegisterSettings OfficialAddressRegisterSettings,
    IOfficialAddressHttpClient OfficialAddressRegisterHttpClient,
    IOfficialAddressMetadataRepository metadataRepository,
    IOfficialAddressUpdater officialContactUpdater)
    : IOfficialAddressRegisterUpdateJob
{
    private readonly OfficialAddressRegisterSettings _officialAddressRegisterSettings = OfficialAddressRegisterSettings;
    private readonly IOfficialAddressHttpClient _officialAddressRegisterHttpClient = OfficialAddressRegisterHttpClient;
    private readonly IOfficialAddressMetadataRepository _metadataRepository = metadataRepository;
    private readonly IOfficialAddressUpdater _officialContactUpdater = officialContactUpdater;

    /// <summary>
    /// Retrieves all changes from the source registry and updates the local contact information.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the endpoint URL is null or empty.</exception>
    public async Task SyncContactInformationAsync()
    {
        if (string.IsNullOrWhiteSpace(_officialAddressRegisterSettings.ChangesLogEndpoint))
        {
            throw new InvalidOperationException("The endpoint URL must not be null or empty.");
        }

        DateTime lastUpdated = await _metadataRepository.GetLatestSyncTimestampAsync();

        string? fullUrl = _officialAddressRegisterSettings.ChangesLogEndpoint + $"?since={lastUpdated.ToString("s")}&pageSize={_officialAddressRegisterSettings.ChangesLogPageSize}";

        do
        {
            OfficialAddressRegisterChangesLog changesLog = await _officialAddressRegisterHttpClient.GetAddressChangesAsync(fullUrl);

            if (changesLog?.OfficialAddressList == null || changesLog.OfficialAddressList?.Count == 0)
            {
                break;
            }
            
            int updatedRowsCount = await _officialContactUpdater.SyncOfficialContactsAsync(changesLog);

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
