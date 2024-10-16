using Altinn.Profile.Core;
using Altinn.Profile.Core.ContactRegister;
using Altinn.Profile.Core.ContactRegister;

namespace Altinn.Profile.Integrations.ContactRegister;

/// <summary>
/// Implementation of the change log service for handling changes in a person's contact preferences.
/// </summary>
internal class ContactRegisterService : IContactRegisterService
{
    private readonly IContactRegisterHttpClient _contactDetailsHttpClient;
    private readonly IContactRegisterSettings _contactRegisterSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContactRegisterService"/> class.
    /// </summary>
    /// <param name="contactDetailsHttpClient">The HTTP client used to retrieve contact details changes.</param>
    /// <param name="contactRegisterSettings">The settings used to configure the contact register.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="contactDetailsHttpClient"/> or <paramref name="contactRegisterSettings"/> is <c>null</c>.
    /// </exception>
    public ContactRegisterService(IContactRegisterHttpClient contactDetailsHttpClient, IContactRegisterSettings contactRegisterSettings)
    {
        _contactRegisterSettings = contactRegisterSettings ?? throw new ArgumentNullException(nameof(contactRegisterSettings));
        _contactDetailsHttpClient = contactDetailsHttpClient ?? throw new ArgumentNullException(nameof(contactDetailsHttpClient));
    }

    /// <summary>
    /// Asynchronously retrieves the changes in contact preferences for all persons starting from a given number.
    /// </summary>
    /// <param name="latestChangeNumber">The number from which to start retrieving the data.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains a <see cref="Result{TValue, TError}"/> object with the contact preferences change log of the person and a boolean indicating success or failure.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if the <see cref="IContactRegisterSettings.ChangesLogEndpoint"/> is <c>null</c> or empty.</exception>
    public async Task<Result<IContactRegisterChangesLog, bool>> RetrievePersonContactPreferencesChanges(long latestChangeNumber = 0)
    {
        if (string.IsNullOrWhiteSpace(_contactRegisterSettings.ChangesLogEndpoint))
        {
            throw new ArgumentNullException(nameof(_contactRegisterSettings.ChangesLogEndpoint));
        }

        return await _contactDetailsHttpClient.GetContactDetailsChangesAsync(_contactRegisterSettings.ChangesLogEndpoint, latestChangeNumber);
    }
}
