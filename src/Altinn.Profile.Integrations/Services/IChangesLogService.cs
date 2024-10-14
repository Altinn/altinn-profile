using Altinn.Profile.Integrations.Entities;

namespace Altinn.Profile.Integrations.Services;

/// <summary>
/// Interface for handling change logs.
/// </summary>
public interface IChangesLogService
{
    /// <summary>
    /// Asynchronously gets the notification status change log for a person.
    /// </summary>
    /// <param name="personIdentifier">The identifier of the person.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the person's notification status change log.</returns>
    Task<IPersonNotificationStatusChangeLog> GetPersonNotificationStatusAsync(string personIdentifier);
}
