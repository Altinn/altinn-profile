using Altinn.Profile.Core;
using Altinn.Profile.Core.ContactRegister;

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
    /// <param name="latestChangeNumber">The starting index for retrieving contact details changes.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object, where <see cref="IContactRegisterChangesLog"/> represents the successful result and <see cref="bool"/> indicates a failure.
    /// </returns>
    Task<Result<IContactRegisterChangesLog, bool>> GetContactDetailsChangesAsync(string endpointUrl, long latestChangeNumber);
}
