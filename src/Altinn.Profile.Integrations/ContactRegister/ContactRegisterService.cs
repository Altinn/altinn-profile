using Altinn.Profile.Core.ContactRegister;

namespace Altinn.Profile.Integrations.ContactRegister;

/// <summary>
/// Implementation of the change log service for handling changes in a person's contact preferences.
/// </summary>
internal class ContactRegisterService : IContactRegisterService
{
    private readonly IContactRegisterHttpClient _contactRegisterHttpClient;
    private readonly IContactRegisterSettings _contactRegisterSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContactRegisterService"/> class.
    /// </summary>
    /// <param name="contactRegisterSettings">The settings used to configure the contact register.</param>
    /// <param name="contactRegisterHttpClient">The HTTP client used to retrieve contact details changes.</param>
    public ContactRegisterService(IContactRegisterSettings contactRegisterSettings, IContactRegisterHttpClient contactRegisterHttpClient)
    {
        _contactRegisterSettings = contactRegisterSettings;
        _contactRegisterHttpClient = contactRegisterHttpClient;
    }

    /// <summary>
    /// Asynchronously retrieves the changes in contact preferences for all persons starting from a given number.
    /// </summary>
    /// <param name="startingIdentifier">The identifier from which to start retrieving the data.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the <see cref="IContactRegisterSettings.ChangesLogEndpoint"/> is <c>null</c> or empty.</exception>
    public async Task<ContactRegisterChangesLog> RetrieveContactDetailsChangesAsync(long startingIdentifier)
    {
        if (string.IsNullOrWhiteSpace(_contactRegisterSettings.ChangesLogEndpoint))
        {
            throw new InvalidOperationException("The endpoint URL must not be null or empty.");
        }

        return await _contactRegisterHttpClient.GetContactDetailsChangesAsync(_contactRegisterSettings.ChangesLogEndpoint, startingIdentifier);
    }
}
