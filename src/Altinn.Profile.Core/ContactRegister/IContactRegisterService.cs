namespace Altinn.Profile.Core.ContactRegister;

/// <summary>
/// Interface for handling changes in a person's contact preferences.
/// </summary>
public interface IContactRegisterService
{
    /// <summary>
    /// Asynchronously retrieves the changes in contact preferences for all persons starting from a given number.
    /// </summary>
    /// <param name="startingIdentifier">The identifier from which to start retrieving the data.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// </returns>
    Task<ContactRegisterChangesLog> RetrieveContactDetailsChangesAsync(long startingIdentifier = 0);
}
