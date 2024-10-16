using Altinn.Profile.Core.Person.ContactPreferences;

namespace Altinn.Profile.Core.ContactRegsiter;

/// <summary>
/// Interface for handling change logs.
/// </summary>
public interface IContactRegisterService
{
    /// <summary>
    /// Asynchronously retrieves the notification status change log for a specified person starting from a given index.
    /// </summary>
    /// <param name="latestChangeNumber">The index from which to start retrieving the data.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains the notification status change log of the person.
    /// </returns>
    Task<IPersonContactPreferencesChangesLog?> GetPersonNotificationStatusAsync(long latestChangeNumber);
}
