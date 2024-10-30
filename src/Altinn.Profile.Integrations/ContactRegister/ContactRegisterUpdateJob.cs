using Altinn.Profile.Core;
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
    /// <param name="personRepository">A repository implementation for managing persistance for the local contact information</param>
    public class ContactRegisterUpdateJob(
        ContactRegisterSettings contactRegisterSettings,
        IContactRegisterHttpClient contactRegisterHttpClient,
        IMetadataRepository metadataRepository,
        IPersonRepository personRepository)
        : IContactRegisterUpdateJob
    {
        private readonly ContactRegisterSettings _contactRegisterSettings = contactRegisterSettings;
        private readonly IContactRegisterHttpClient _contactRegisterHttpClient = contactRegisterHttpClient;
        private readonly IMetadataRepository _metadataRepository = metadataRepository;
        private readonly IPersonRepository _personRepository = personRepository;

        /// <summary>
        /// Retrieves all changes from the source registry and updates the local contact information.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">Not implemented yet</exception>
        public async Task SyncContactInformationAsync()
        {
            // Retrieve the changes in contact preferences from the changes log.
            if (string.IsNullOrWhiteSpace(_contactRegisterSettings.ChangesLogEndpoint))
            {
                throw new InvalidOperationException("The endpoint URL must not be null or empty.");
            }

            long latestChangeNumber = 0;
            Result<long, bool> latestChangeNumberGetter = await _metadataRepository.GetLatestChangeNumberAsync();
            latestChangeNumberGetter.Match(e => latestChangeNumber = e, _ => latestChangeNumber = 0);

            ContactRegisterChangesLog contactDetailsChanges =
                await _contactRegisterHttpClient.GetContactDetailsChangesAsync(
                    _contactRegisterSettings.ChangesLogEndpoint, latestChangeNumber);

            int synchornizedRowCount = await _personRepository.SyncPersonContactPreferencesAsync(contactDetailsChanges);
            if (synchornizedRowCount > 0 && contactDetailsChanges.EndingIdentifier.HasValue)
            {
                await _metadataRepository.UpdateLatestChangeNumberAsync(contactDetailsChanges.EndingIdentifier.Value);
            }
        }
    }
}
