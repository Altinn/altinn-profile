using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations.Services;

/// <summary>
/// Implementation of the change log service.
/// </summary>
internal class ChangesLogService : IChangesLogService
{
    private readonly IContactDetailsHttpClient _contactDetailsHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChangesLogService"/> class.
    /// </summary>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="contactDetailsHttpClient"/> is <c>null</c>.
    /// </exception>
    public ChangesLogService(IContactDetailsHttpClient contactDetailsHttpClient)
    {
        _contactDetailsHttpClient = contactDetailsHttpClient ?? throw new ArgumentNullException(nameof(contactDetailsHttpClient));
    }

    /// <summary>
    /// Asynchronously gets the notification status change log for a person.
    /// </summary>
    /// <param name="personIdentifier">The identifier of the person.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the person's notification status change log.
    /// </returns>
    public async Task<IPersonNotificationStatusChangeLog> GetPersonNotificationStatusAsync(string personIdentifier)
    {
        var changes = await _contactDetailsHttpClient.GetContactDetailsChangesAsync("https://test.kontaktregisteret.no/rest/v2/krr/hentEndringer", 0);

        return null;
    }
}
