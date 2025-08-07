namespace Altinn.Profile.Integrations.ContactRegister;

/// <summary>
/// An HTTP client to interact with the contact register.
/// </summary>
public interface IContactRegisterHttpClient
{
    /// <summary>
    /// Retrieves the changes in persons' contact details from the specified endpoint.
    /// </summary>
    /// <param name="endpointUrl">The URL of the endpoint to retrieve contact details changes from.</param>
    /// <param name="startingIdentifier">The starting identifier for retrieving contact details changes.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    Task<ContactRegisterChangesLog?> GetContactDetailsChangesAsync(string endpointUrl, long startingIdentifier);
}
