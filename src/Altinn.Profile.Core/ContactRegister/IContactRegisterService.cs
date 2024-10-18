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
    /// A task that represents the asynchronous operation. The task result contains a <see cref="Result{TValue, TError}"/> object, where <see cref="IContactRegisterChangesLog"/> represents the successful result and <see cref="bool"/> indicates a failure.
    /// </returns>
    Task<Result<IContactRegisterChangesLog, bool>> RetrieveContactDetailsChangesAsync(long startingIdentifier = 0);
}
