using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations.Services;

/// <summary>
/// Interface for handling change logs.
/// </summary>
public interface IChangesLogService
{
    /// <summary>
    /// Asynchronously retrieves the notification status change log for a specified person starting from a given index.
    /// </summary>
    /// <param name="margin">The index from which to start retrieving the data.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains the notification status change log of the person.
    /// </returns>
    Task<IEnumerable<IPersonNotificationStatusChangeLog>> GetPersonNotificationStatusAsync(string margin);
}
