using Altinn.Profile.Integrations.Repositories;

namespace Altinn.Profile.Integrations.ContactRegister
{
    /// <summary>
    /// An implementation of the <see cref="IContactRegisterUpdateJob"/> interface that will retrieve 
    /// changes from the source registry and update the local contact information.
    /// </summary>
    /// <param name="contactRegisterSettings">Settings for the synchronization update job</param>
    /// <param name="contactRegisterHttpClient">A HTTP client that can be used to retrieve contact details changes</param>
    /// <param name="metadataRepository">A repository implementation for managing persistance of the job status between runs</param>
    /// <param name="personUpdater">A repository implementation for managing persistance for the local contact information</param>
    public class ContactRegisterUpdateJob(
        ContactRegisterSettings contactRegisterSettings,
        IContactRegisterHttpClient contactRegisterHttpClient,
        IMetadataRepository metadataRepository,
        IPersonUpdater personUpdater)
        : IContactRegisterUpdateJob
    {
        private readonly ContactRegisterSettings _contactRegisterSettings = contactRegisterSettings;
        private readonly IContactRegisterHttpClient _contactRegisterHttpClient = contactRegisterHttpClient;
        private readonly IMetadataRepository _metadataRepository = metadataRepository;
        private readonly IPersonUpdater _personUpdater = personUpdater;

        /// <summary>
        /// Retrieves all changes from the source registry and updates the local contact information.
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SyncContactInformationAsync()
        {
            if (string.IsNullOrWhiteSpace(_contactRegisterSettings.ChangesLogEndpoint))
            {
                throw new InvalidOperationException("The endpoint URL must not be null or empty.");
            }

            long finalGlobalChangeNumber;
            long lastChangeNumberInBatch;
            do
            {
                long previousChangeNumber = await _metadataRepository.GetLatestChangeNumberAsync();

                ContactRegisterChangesLog registerChanges =
                    await _contactRegisterHttpClient.GetContactDetailsChangesAsync(
                        _contactRegisterSettings.ChangesLogEndpoint, previousChangeNumber);

                int synchornizedRowCount = 
                    await _personUpdater.SyncPersonContactPreferencesAsync(registerChanges);

                // setting it high to escape the loop if something is wrong
                lastChangeNumberInBatch = long.MaxValue; 
                if (synchornizedRowCount > 0 && registerChanges.EndingIdentifier.HasValue)
                {
                    lastChangeNumberInBatch = registerChanges.EndingIdentifier.Value;
                    await _metadataRepository.UpdateLatestChangeNumberAsync(lastChangeNumberInBatch);
                }

                // defaulting to -1 to escape the loop if something is wrong
                finalGlobalChangeNumber = registerChanges.LatestChangeIdentifier ?? -1;
            }
            while (lastChangeNumberInBatch < finalGlobalChangeNumber);
        }
    }
}
