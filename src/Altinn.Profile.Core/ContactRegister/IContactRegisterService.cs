namespace Altinn.Profile.Core.ContactRegsiter;

/// <summary>
/// Interface for handling changes in a person's contact preferences.
/// </summary>
public interface IContactRegisterService
{
    /// <summary>
    /// Asynchronously retrieves the changes in contact preferences for all persons starting from a given number.
    /// </summary>
    /// <param name="latestChangeNumber">The number from which to start retrieving the data.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. 
    /// The task result contains a <see cref="Result{TValue, TError}"/> object with the contact preferences change log of the person and a boolean indicating success or failure.
    /// </returns>
    Task<Result<IContactRegisterChangesLog, bool>> RetrievePersonContactPreferencesChanges(long latestChangeNumber);
}
