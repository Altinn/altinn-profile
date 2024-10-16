using Altinn.Profile.Core;
using Altinn.Profile.Core.ContactRegsiter;
using Altinn.Profile.Core.Person.ContactPreferences;
using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations.Services;

/// <summary>
/// Implementation of the change log service.
/// </summary>
internal class ChangesLogService : IContactRegisterService
{
    private readonly IPersonContactPreferencesHttpClient _contactDetailsHttpClient;
    private readonly IContactRegisterSettings _contactRegisterSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangesLogService"/> class.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="contactDetailsHttpClient"/> is <c>null</c>.
    /// </exception>
    public ChangesLogService(IPersonContactPreferencesHttpClient contactDetailsHttpClient, IContactRegisterSettings contactRegisterSettings)
    {
        _contactRegisterSettings = contactRegisterSettings ?? throw new ArgumentNullException(nameof(contactRegisterSettings));
        _contactDetailsHttpClient = contactDetailsHttpClient ?? throw new ArgumentNullException(nameof(contactDetailsHttpClient));
    }

    /// <summary>
    /// Asynchronously retrieves the notification status change log for a specified person starting from a given index.
    /// </summary>
    /// <param name="latestChangeNumber">The index from which to start retrieving the data.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains the notification status change log of the person.
    /// </returns>
    public async Task<IPersonContactPreferencesChangesLog?> RetrievePersonContactPreferencesChanges(long latestChangeNumber)
    {
        if (string.IsNullOrWhiteSpace(_contactRegisterSettings.ChangesLogEndpoint))
        {
            throw new ArgumentNullException();
        }

        return await _contactDetailsHttpClient.GetContactDetailsChangesAsync(_contactRegisterSettings.ChangesLogEndpoint, latestChangeNumber);
    }

    Task<Result<IPersonContactPreferencesChangesLog, bool>> IContactRegisterService.RetrievePersonContactPreferencesChanges(long latestChangeNumber)
    {
        throw new NotImplementedException();
    }
}
