using Altinn.Profile.Core.Telemetry;
using Altinn.Profile.Integrations.Repositories;

namespace Altinn.Profile.Integrations.ContactRegister;

/// <summary>
/// An implementation of the <see cref="IContactRegisterUpdateJob"/> interface that will retrieve 
/// changes from the source registry and update the local contact information.
/// </summary>
/// <param name="contactRegisterSettings">Settings for the synchronization update job</param>
/// <param name="contactRegisterHttpClient">A HTTP client that can be used to retrieve contact details changes</param>
/// <param name="metadataRepository">A repository implementation for managing persistence of the job status between runs</param>
/// <param name="personUpdater">A repository implementation for managing persistence for the local contact information</param>
/// <param name="telemetry">The application Telemetry instance.</param>
public class ContactRegisterUpdateJob(
    ContactRegisterSettings contactRegisterSettings,
    IContactRegisterHttpClient contactRegisterHttpClient,
    IMetadataRepository metadataRepository,
    IPersonUpdater personUpdater,
    Telemetry? telemetry = null)
    : IContactRegisterUpdateJob
{
    private readonly ContactRegisterSettings _contactRegisterSettings = contactRegisterSettings;
    private readonly IContactRegisterHttpClient _contactRegisterHttpClient = contactRegisterHttpClient;
    private readonly IMetadataRepository _metadataRepository = metadataRepository;
    private readonly IPersonUpdater _personUpdater = personUpdater;
    private readonly Telemetry? _telemetry = telemetry;

    /// <summary>
    /// Retrieves all changes from the source registry and updates the local contact information.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the endpoint URL is null or empty.</exception>
    public async Task SyncContactInformationAsync()
    {
        using var activity = _telemetry?.StartContactRegistryUpdateJob();

        if (string.IsNullOrWhiteSpace(_contactRegisterSettings.ChangesLogEndpoint))
        {
            throw new InvalidOperationException("The endpoint URL must not be null or empty.");
        }

        long finalGlobalChangeNumber;
        long lastProcessedChangeNumber;

        do
        {
            long previousChangeNumber = await _metadataRepository.GetLatestChangeNumberAsync();

            ContactRegisterChangesLog? changesLog = await _contactRegisterHttpClient.GetContactDetailsChangesAsync(_contactRegisterSettings.ChangesLogEndpoint, previousChangeNumber);

            if (changesLog == null)
            {
                break;
            }

            int updatedRowsCount = await _personUpdater.SyncPersonContactPreferencesAsync(changesLog);

            if (updatedRowsCount > 0 && changesLog.EndingIdentifier.HasValue)
            {
                lastProcessedChangeNumber = changesLog.EndingIdentifier.Value;
                await _metadataRepository.UpdateLatestChangeNumberAsync(lastProcessedChangeNumber);
            }
            else
            {
                break;
            }

            finalGlobalChangeNumber = changesLog.LatestChangeIdentifier ?? lastProcessedChangeNumber;
        }
        while (lastProcessedChangeNumber < finalGlobalChangeNumber);
    }
}
