using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations.Services;

/// <summary>
/// Implementation of the change log service.
/// </summary>
internal class ChangesLogService : IChangesLogService
{
    private readonly IPersonContactPreferencesHttpClient _contactDetailsHttpClient;
    private readonly IContactAndReservationSettings _contactRegisterSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangesLogService"/> class.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="contactDetailsHttpClient"/> is <c>null</c>.
    /// </exception>
    public ChangesLogService(IPersonContactPreferencesHttpClient contactDetailsHttpClient, IContactAndReservationSettings contactRegisterSettings)
    {
        _contactRegisterSettings = contactRegisterSettings ?? throw new ArgumentNullException(nameof(contactRegisterSettings));
        _contactDetailsHttpClient = contactDetailsHttpClient ?? throw new ArgumentNullException(nameof(contactDetailsHttpClient));
    }

    /// <summary>
    /// Asynchronously retrieves the notification status change log for a specified person starting from a given index.
    /// </summary>
    /// <param name="margin">The index from which to start retrieving the data.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains the notification status change log of the person.
    /// </returns>
    public async Task<IEnumerable<IPersonContactPreferencesSnapshot>?> GetPersonNotificationStatusAsync(string margin)
    {
        if (string.IsNullOrWhiteSpace(_contactRegisterSettings.ContactDetailsChangesEndpoint))
        {
            throw new ArgumentNullException();
        }

        return await _contactDetailsHttpClient.GetContactDetailsChangesAsync(_contactRegisterSettings.ContactDetailsChangesEndpoint, margin);
    }
}
